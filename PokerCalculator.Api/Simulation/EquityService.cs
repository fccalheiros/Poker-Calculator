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
        ValidateCardStrings(request.Hero, request.Board, request.Game);

        var villainRanges = NormalizeVillainRanges(request);
        ValidateVillainRangeLimits(villainRanges, request.Game);
        string cacheKey = EquityCacheKeyBuilder.Build(request.Hero, request.Board, villainRanges, request.Game);

        if (_cache.TryGet(cacheKey, out var cached))
        {
            return ToResponse(cached.Result, cached.Simulations, fromCache: true);
        }

        (ulong hero, ulong board, int boardCardsLeft, ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize) =
            ParseInput(request, villainRanges);

        long[] simsPerTask = PlanSimulations(request.Game);

        SimulationResult total = await RunBatch(
            hero, board, boardCardsLeft, villainRanges.Count, singleRange, multiRange, multiRangeSize, request.Game, simsPerTask, cancellationToken);

        long simulationsRun = simsPerTask.Sum();

        if (simulationsRun >= _options.MinSimulationsToCache)
        {
            _cache.Set(cacheKey, new CachedEquityResult(total, simulationsRun));
        }

        return ToResponse(total, simulationsRun, fromCache: false);
    }

    // Rejects malformed Hero/Board before anything downstream (cache key building, then
    // parsing) touches them. PEval.ConvertStringToCardSet/OrderStringCardSet trust their
    // input to be well-formed - an odd-length or garbage-character string crashes the
    // former and corrupts the latter's cache key instead of failing cleanly, so every
    // request has to pass this check first. Hero must be exactly 2 distinct cards for
    // Hold'em or 4 for Omaha; board, if present, must be 0 (omitted), 3, 4, or 5 distinct
    // cards that don't overlap hero.
    private static void ValidateCardStrings(string hero, string board, GameType game)
    {
        int expectedHeroCards = game == GameType.Omaha ? 4 : 2;

        ulong heroSet = ParseCompleteCardString(hero, "hero");
        if (PEval.bitCount(heroSet) != expectedHeroCards)
            throw new FormatException(Messages.Validation.WrongHeroCardCount(expectedHeroCards, game, hero));

        if (string.IsNullOrWhiteSpace(board))
            return;

        ulong boardSet = ParseCompleteCardString(board, "board");
        int boardCount = PEval.bitCount(boardSet);
        if (boardCount != 3 && boardCount != 4 && boardCount != 5)
            throw new FormatException(Messages.Validation.InvalidBoardCardCount(board));

        if ((heroSet & boardSet) != 0)
            throw new FormatException(Messages.Validation.HeroBoardOverlap);
    }

    // Stricter than PEval.IsValidStringCardSet, which deliberately tolerates a trailing
    // half-typed card so the WinForms textboxes stay usable while the user is still
    // typing. An API request is either a complete, well-formed card string or it's
    // rejected outright.
    private static ulong ParseCompleteCardString(string s, string fieldName)
    {
        string value = s ?? string.Empty;
        string trimmed = value.Replace(" ", "");
        if (trimmed.Length == 0 || trimmed.Length % 2 != 0)
            throw new FormatException(Messages.Validation.InvalidCards(fieldName, value));

        for (int i = 0; i < trimmed.Length; i += 2)
        {
            if (PEval.CardRank(trimmed[i]) < 0 || PEval.CardSuit(trimmed[i + 1]) < 0)
                throw new FormatException(Messages.Validation.InvalidCards(fieldName, value));
        }

        return PEval.ConvertStringToCardSet(trimmed);
    }

    // Bounds VillainRanges (after dedup) before any expensive work happens: too many
    // villains inflates per-simulation cost (SimulateRangeN loops every villain each
    // iteration), and too many range tokens for one villain inflates RangeParser's
    // combinatorial expansion cost - a bare Omaha rank pattern like "AKQJ" alone expands to
    // 256 combos versus Hold'em's worst case of 16, hence the lower Omaha ceilings.
    private void ValidateVillainRangeLimits(List<List<string>> villainRanges, GameType game)
    {
        int maxVillains = game == GameType.Omaha ? _options.MaxVillainsOmaha : _options.MaxVillainsHoldem;
        if (villainRanges.Count > maxVillains)
            throw new FormatException(Messages.Validation.TooManyVillains(villainRanges.Count, maxVillains, game));

        int maxTokens = game == GameType.Omaha ? _options.MaxRangeTokensPerVillainOmaha : _options.MaxRangeTokensPerVillainHoldem;
        foreach (var range in villainRanges)
        {
            if (range.Count > maxTokens)
                throw new FormatException(Messages.Validation.TooManyRangeTokens(range.Count, maxTokens, game));
        }
    }

    // An absent/empty VillainRanges means "one villain, unknown hand". Tokens are
    // deduplicated and sorted per villain (trimmed, case-insensitive) - both to avoid
    // wasted RangeParser work on repeated tokens (a range with the same token repeated
    // thousands of times would otherwise burn CPU on redundant parsing for no change in
    // the result) and so the resulting cache key is stable regardless of token order or
    // repeats. Villains are then sorted by their normalized range so the cache key is also
    // stable regardless of which array slot a given villain's range was submitted in -
    // villain order carries no meaning in the response (hero vs. best-of-field win/loss/tie
    // has no per-villain breakdown), so reordering here can't change the simulated result.
    private static List<List<string>> NormalizeVillainRanges(EquityRequest request)
    {
        var ranges = request.VillainRanges is { Count: > 0 } ? request.VillainRanges : new List<List<string>> { new() };

        return ranges
            .Select(range => range
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
                .ToList())
            .OrderBy(range => string.Join(',', range), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Converts the wire-format request (plain strings) into the bitmasks and parsed
    // ranges PokerMonteCarloServer.Configure expects.
    private static (ulong hero, ulong board, int boardCardsLeft, ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize)
        ParseInput(EquityRequest request, List<List<string>> villainRanges)
    {
        ulong hero = PEval.ConvertStringToCardSet(request.Hero);
        ulong board = string.IsNullOrWhiteSpace(request.Board) ? 0 : PEval.ConvertStringToCardSet(request.Board);
        int boardCardsLeft = 5 - PEval.bitCount(board);

        ulong[]? singleRange = villainRanges.Count == 1 && villainRanges[0].Count > 0
            ? ParseVillainRange(villainRanges[0], request.Game)
            : null;

        ulong[,]? multiRange = null;
        int[]? multiRangeSize = null;
        if (villainRanges.Count > 1)
        {
            multiRange = BuildMultiRange(villainRanges, request.Game, out multiRangeSize);
        }

        return (hero, board, boardCardsLeft, singleRange, multiRange, multiRangeSize);
    }

    private static ulong[] ParseVillainRange(List<string> tokens, GameType game) =>
        game == GameType.Omaha ? RangeParser.ParseOmahaRange(tokens) : RangeParser.ParseHoldemRange(tokens);

    private static ulong[,] BuildMultiRange(List<List<string>> villainRanges, GameType game, out int[] sizes)
    {
        var parsed = villainRanges
            .Select(r => r.Count > 0 ? ParseVillainRange(r, game) : throw new FormatException(Messages.Validation.EmptyVillainRangeInMultiway))
            .ToList();

        sizes = parsed.Select(p => p.Length).ToArray();
        int maxSize = sizes.Max();
        var range = new ulong[parsed.Count, maxSize];
        for (int v = 0; v < parsed.Count; v++)
            for (int c = 0; c < parsed[v].Length; c++)
                range[v, c] = parsed[v][c];
        return range;
    }

    // Decides how many simulations this request gets and how that total is split across
    // sub-tasks. Omaha evaluates a hand by trying every 2-of-4-pocket x 3-of-5-board
    // combination (OmahaEval.ProcessCardSet) instead of Hold'em's single pass, so it starts
    // from a fraction of FullSimulations (OmahaSimulationDivisor) to keep per-request cost
    // comparable. On top of that, simulation count and parallelism scale down together as
    // the queue fills up: every time the queue's free capacity halves again, the load
    // divisor doubles (2, 4, 8, ...), capped at MaxLoadDivisor so a nearly-full queue still
    // gets a usable batch instead of being throttled to a single simulation.
    private long[] PlanSimulations(GameType game)
    {
        int loadDivisor = ComputeLoadDivisor(_pool.PendingCount, _options.MaxQueueSize, _options.MaxLoadDivisor);
        int gameDivisor = game == GameType.Omaha ? Math.Max(1, _options.OmahaSimulationDivisor) : 1;

        long totalSimulations = Math.Max(1, _options.FullSimulations / gameDivisor / loadDivisor);
        int parallelism = Math.Max(1, _options.Parallelism / loadDivisor);

        return SplitEvenly(totalSimulations, parallelism).Where(s => s > 0).ToArray();
    }

    // Returns 1 while at least half the queue is free. Each time the free capacity halves
    // again (1/4 free, 1/8 free, ...) the divisor doubles, up to maxLoadDivisor.
    private static int ComputeLoadDivisor(int pending, int maxQueueSize, int maxLoadDivisor)
    {
        if (maxQueueSize <= 0) return 1;

        long freeSlots = Math.Max(0, maxQueueSize - pending);

        int divisor = 1;
        while (divisor < maxLoadDivisor && freeSlots * (divisor * 2) <= maxQueueSize)
            divisor *= 2;
        return divisor;
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
        GameType game, long[] simsPerTask, CancellationToken cancellationToken)
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
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, multiRange, multiRangeSize!, villainCount, Callback, cancellationToken, game);
                        server.SimulateRangeN();
                    }
                    else if (singleRange is not null)
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, singleRange, singleRange.Length, Callback, cancellationToken, game);
                        server.SimulateRange();
                    }
                    else
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, Callback, cancellationToken, game);
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

    private const int PercentDecimals = 4;

    private static EquityResponse ToResponse(SimulationResult result, long simulations, bool fromCache)
    {
        double total = result.Win + result.Loss + result.Tie;
        if (total == 0)
            return new EquityResponse(0, 0, 0, 0, simulations, fromCache);

        // Loss/tie are rounded first; hero's win percentage absorbs whatever rounding
        // slack that leaves behind, so the three always sum to exactly 100 instead of
        // drifting by a fraction of a percent once each is rounded independently.
        double lossPercent = Math.Round(result.Loss / total * 100, PercentDecimals);
        double tiePercent = Math.Round(result.Tie / total * 100, PercentDecimals);
        double winPercent = Math.Round(100 - lossPercent - tiePercent, PercentDecimals);

        double equity = Math.Round((result.Win + result.TieEquity) / total * 100, PercentDecimals);

        return new EquityResponse(winPercent, lossPercent, tiePercent, equity, simulations, fromCache);
    }
}
