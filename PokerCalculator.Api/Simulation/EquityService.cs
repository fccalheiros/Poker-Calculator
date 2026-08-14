// Orchestrates one equity request: cache lookup, input parsing, admission control,
// splitting the work into sub-tasks run on the fixed worker pool, joining the results,
// and reading/writing the cache. Kept synchronous from the caller's point of view (the
// endpoint awaits this call) per the current design - no async polling/callback queue yet.
using System.Linq;
using Microsoft.Extensions.Options;
using PokerCalculator.Api.Contracts;
using PokerCalculator.Api.Options;

namespace PokerCalculator.Api.Simulation;

public class EquityService
{
    private readonly SimulationWorkerPool _pool;
    private readonly EquityCache _cache;
    private readonly SimulationOptions _options;

    public EquityService(SimulationWorkerPool pool, EquityCache cache, IOptions<SimulationOptions> options)
    {
        _pool = pool;
        _cache = cache;
        _options = options.Value;
    }

    public async Task<EquityResponse> ComputeEquity(EquityRequest request, CancellationToken cancellationToken = default)
    {
        var villainRanges = NormalizeVillainRanges(request);
        string cacheKey = EquityCacheKeyBuilder.Build(request.Hero, request.Board, villainRanges);

        if (_cache.TryGet(cacheKey, out var cached))
        {
            return ToResponse(cached.Result, cached.Simulations, fromCache: true);
        }

        (ulong hero, ulong board, int boardCardsLeft, ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize) =
            ParseInput(request, villainRanges);

        long[] simsPerTask = PlanSimulations();

        SimulationResult total = await RunBatch(
            hero, board, boardCardsLeft, villainRanges.Count, singleRange, multiRange, multiRangeSize, simsPerTask, cancellationToken);

        long simulationsRun = simsPerTask.Sum();

        if (simulationsRun >= _options.MinSimulationsToCache)
        {
            _cache.Set(cacheKey, new CachedEquityResult(total, simulationsRun));
        }

        return ToResponse(total, simulationsRun, fromCache: false);
    }

    // An absent/empty VillainRanges means "one villain, unknown hand".
    private static List<List<string>> NormalizeVillainRanges(EquityRequest request) =>
        request.VillainRanges is { Count: > 0 } ? request.VillainRanges : new List<List<string>> { new() };

    // Converts the wire-format request (plain strings) into the bitmasks and parsed
    // ranges PokerMonteCarloServer.Configure expects.
    private static (ulong hero, ulong board, int boardCardsLeft, ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize)
        ParseInput(EquityRequest request, List<List<string>> villainRanges)
    {
        ulong hero = PEval.ConvertStringToCardSet(request.Hero);
        ulong board = string.IsNullOrWhiteSpace(request.Board) ? 0 : PEval.ConvertStringToCardSet(request.Board);
        int boardCardsLeft = 5 - PEval.bitCount(board);

        ulong[]? singleRange = villainRanges.Count == 1 && villainRanges[0].Count > 0
            ? RangeParser.ParseRange(villainRanges[0])
            : null;

        ulong[,]? multiRange = null;
        int[]? multiRangeSize = null;
        if (villainRanges.Count > 1)
        {
            multiRange = BuildMultiRange(villainRanges, out multiRangeSize);
        }

        return (hero, board, boardCardsLeft, singleRange, multiRange, multiRangeSize);
    }

    private static ulong[,] BuildMultiRange(List<List<string>> villainRanges, out int[] sizes)
    {
        var parsed = villainRanges
            .Select(r => r.Count > 0 ? RangeParser.ParseRange(r) : throw new FormatException("All villain ranges must be non-empty when more than one villain is specified."))
            .ToList();

        sizes = parsed.Select(p => p.Length).ToArray();
        int maxSize = sizes.Max();
        var range = new ulong[parsed.Count, maxSize];
        for (int v = 0; v < parsed.Count; v++)
            for (int c = 0; c < parsed[v].Length; c++)
                range[v, c] = parsed[v][c];
        return range;
    }

    // Decides how many simulations this request gets - full precision, or reduced under
    // load - and how that total is split across Parallelism sub-tasks.
    private long[] PlanSimulations()
    {
        int pending = _pool.PendingCount;
        bool underLoad = pending > _options.MaxQueueSize / 2;
        long totalSimulations = underLoad ? _options.ReducedSimulations : _options.FullSimulations;

        int parallelism = Math.Max(1, _options.Parallelism);
        return SplitEvenly(totalSimulations, parallelism).Where(s => s > 0).ToArray();
    }

    private static long[] SplitEvenly(long total, int parts)
    {
        var result = new long[parts];
        long baseShare = total / parts;
        long remainder = total % parts;
        for (int i = 0; i < parts; i++)
            result[i] = baseShare + (i < remainder ? 1 : 0);
        return result;
    }

    // Reserves capacity on the worker pool for the whole batch in one atomic step, enqueues
    // every sub-task, and joins their results. Either the whole batch fits, or the request
    // is rejected outright - no partially-enqueued batches.
    private async Task<SimulationResult> RunBatch(
        ulong hero, ulong board, int boardCardsLeft, int villainCount,
        ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize,
        long[] simsPerTask, CancellationToken cancellationToken)
    {
        if (!_pool.TryReserve(simsPerTask.Length))
            throw new ServiceBusyException();

        var tasks = new List<Task<SimulationResult>>(simsPerTask.Length);

        foreach (var sims in simsPerTask)
        {
            var tcs = new TaskCompletionSource<SimulationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            tasks.Add(tcs.Task);

            void Callback(SimulationResult result) => tcs.TrySetResult(result);

            void Job(PokerMonteCarloServer server)
            {
                try
                {
                    if (multiRange is not null)
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, multiRange, multiRangeSize!, villainCount, Callback, cancellationToken);
                        server.SimulateRangeN();
                    }
                    else if (singleRange is not null)
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, singleRange, singleRange.Length, Callback, cancellationToken);
                        server.SimulateRange();
                    }
                    else
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, Callback, cancellationToken);
                        server.Simulate();
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            _pool.Enqueue(Job);
        }

        // If the caller disconnects, stop waiting instead of blocking until every sub-task
        // finishes; the sub-tasks themselves also react to cancellationToken and exit early
        // (see PokerMonteCarloServer), freeing their pool reservation without running to completion.
        var results = await Task.WhenAll(tasks).WaitAsync(cancellationToken);

        return results.Aggregate(SimulationResult.Empty, (acc, r) => acc + r);
    }

    private static EquityResponse ToResponse(SimulationResult result, long simulations, bool fromCache)
    {
        double total = result.Win + result.Loss + result.Tie;
        if (total == 0)
            return new EquityResponse(0, 0, 0, 0, simulations, fromCache);

        double winPercent = result.Win / total * 100;
        double lossPercent = result.Loss / total * 100;
        double tiePercent = result.Tie / total * 100;
        double equity = (result.Win + result.TieEquity) / total * 100;

        return new EquityResponse(winPercent, lossPercent, tiePercent, equity, simulations, fromCache);
    }
}
