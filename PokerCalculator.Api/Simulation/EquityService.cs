// Orchestrates one equity request: cache lookup, input parsing, admission control,
// splitting the work into sub-tasks run on the fixed worker pool, joining the results,
// and reading/writing the cache. Kept synchronous from the caller's point of view (the
// endpoint awaits this call) per the current design - no async polling/callback queue yet.
using System.Diagnostics;
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
    private readonly UsageStats _usage;

    public EquityService(SimulationWorkerPool pool, EquityCache cache, IOptions<SimulationOptions> options, UsageStats usage)
    {
        _pool = pool;
        _cache = cache;
        _options = options.Value;
        _usage = usage;
    }

    public async Task<EquityResponse> ComputeEquity(EquityRequest request, CancellationToken cancellationToken = default)
    {
        _usage.RecordRequest(request.Game);

        ValidateCardStrings(request.Hero, request.Board, request.Game);

        var villainRanges = NormalizeVillainRanges(request);
        ValidateVillainRangeLimits(villainRanges, request.Game);
        string cacheKey = EquityCacheKeyBuilder.Build(request.Hero, request.Board, villainRanges, request.Game);

        // AllowExactEnumeration=false is a deliberate "compare both paths" testing request
        // (see EquityRequest) - it bypasses the cache entirely so it always sees a fresh
        // Monte Carlo computation instead of a result the other path may have cached.
        bool useCache = request.AllowExactEnumeration;
        if (useCache)
        {
            if (_cache.TryGet(cacheKey, out var cached))
            {
                _usage.RecordCacheHit();
                return ToResponse(cached.Result, cached.Simulations, fromCache: true, cached.Exact);
            }
            _usage.RecordCacheMiss();
        }

        (ulong hero, ulong board, int boardCardsLeft, ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize) =
            ParseInput(request, villainRanges);

        long[] simsPerTask = PlanSimulations(request.Game);
        long plannedSimsTotal = simsPerTask.Sum();

        bool useExact = request.AllowExactEnumeration &&
            DecideExact(hero, boardCardsLeft, request.Game, villainRanges.Count, singleRange, multiRangeSize, plannedSimsTotal);

        // Wall-clock around the whole (parallel) batch, not summed per sub-task - the
        // sub-tasks run concurrently on the worker pool, so summing their individual
        // durations would inflate the number well past what the caller actually waited.
        var stopwatch = Stopwatch.StartNew();
        SimulationResult total;
        long simulationsRun;
        if (useExact)
        {
            total = await RunExactBatch(
                hero, board, boardCardsLeft, villainRanges.Count, singleRange, multiRange, multiRangeSize, request.Game, cancellationToken);
            simulationsRun = (long)(total.Win + total.Loss + total.Tie);
        }
        else
        {
            total = await RunBatch(
                hero, board, boardCardsLeft, villainRanges.Count, singleRange, multiRange, multiRangeSize, request.Game, simsPerTask, cancellationToken);
            simulationsRun = plannedSimsTotal;
        }
        stopwatch.Stop();
        _usage.RecordComputeTime(request.Game, stopwatch.ElapsedTicks);

        if (useCache && simulationsRun >= _options.MinSimulationsToCache)
        {
            _cache.Set(cacheKey, new CachedEquityResult(total, simulationsRun, useExact));
        }

        return ToResponse(total, simulationsRun, fromCache: false, useExact);
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

    private static ulong[] ParseVillainRange(List<string> tokens, GameType game)
    {
        ulong[] combos = game == GameType.Omaha ? RangeParser.ParseOmahaRange(tokens) : RangeParser.ParseHoldemRange(tokens);
        if (combos.Length == 0)
            throw new FormatException(Messages.Validation.RangeProducedNoCombos(game));
        return combos;
    }

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

    // No worker in an exact-enumeration batch is handed fewer than this many hand
    // evaluations to do, unless the whole batch fits in one worker to begin with - splitting
    // a small batch into many tiny slices just adds dispatch overhead for no parallelism
    // benefit.
    private const long MinCalculationsPerWorker = 20_000;

    // Decides whether this request should run via exact enumeration instead of Monte Carlo.
    // Two shapes are always exact regardless of cost - both restricted to a single villain,
    // because beyond that the villain cross-product can blow up with zero disjuntor pruning
    // if hero happens to be unbeatable (see design notes - this was caught before
    // implementing it as an unconditional board-complete rule):
    //   - board already complete (boardCardsLeft == 0): zero-variance and, with one villain,
    //     at most ~990 (Hold'em) or ~123k (Omaha) evaluations - always far cheaper than any
    //     Monte Carlo batch.
    //   - Hold'em, river missing (boardCardsLeft == 1): at most 46 board combos x ~990
    //     villain combos - still tiny.
    // Every other shape (more than one villain, or fewer board cards known) is decided by
    // comparing an exact-cost estimate against what Monte Carlo would spend on this same
    // request, with an 80% margin: exact is used only when it's estimated to cost no more
    // than 80% of the Monte Carlo alternative.
    private bool DecideExact(
        ulong hero, int boardCardsLeft, GameType game, int villainCount,
        ulong[]? singleRange, int[]? multiRangeSize, long plannedSimsTotal)
    {
        if (villainCount == 1 && (boardCardsLeft == 0 || (game == GameType.Holdem && boardCardsLeft == 1)))
            return true;

        int heroCardCount = PEval.bitCount(hero);
        long boardCombosCount = BinomialCoefficient(47 - heroCardCount + boardCardsLeft, boardCardsLeft);

        // Monte Carlo's own per-request cost in the same unit (one hand evaluation): each
        // simulation evaluates every villain once (hero's own evaluation is comparatively
        // negligible on both sides of this comparison, so it's left out of both).
        long budget = (long)(0.8 * plannedSimsTotal * villainCount);

        long running = boardCombosCount;
        if (running > budget) return false;

        if (multiRangeSize is not null)
        {
            // BuildMultiRange requires every villain to have an explicit range once there's
            // more than one villain, so multiRangeSize always holds real combo counts here -
            // never the "unknown hand" ceiling below.
            foreach (int size in multiRangeSize)
            {
                running *= size;
                if (running > budget) return false;
            }
        }
        else if (singleRange is not null)
        {
            running *= singleRange.Length;
            if (running > budget) return false;
        }
        else
        {
            // Single villain, no range given: the real max is the "any hand" ceiling itself
            // - a specified range can never exceed it, since it's always a subset of it.
            long ceiling = BinomialCoefficient(47 - heroCardCount, heroCardCount);
            running *= ceiling;
            if (running > budget) return false;
        }

        return true;
    }

    // Standard iterative nCk - result stays integral at every step, so this never needs
    // intermediate fractional/rounded values. Only ever called with small n/k here (board
    // sizes and hand sizes), so no overflow risk.
    private static long BinomialCoefficient(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        long result = 1;
        for (int i = 0; i < k; i++)
            result = result * (n - i) / (i + 1);
        return result;
    }

    // The real (not margin-adjusted) total evaluation count DecideExact's chosen path will
    // do - used to size the partitioning below, once exact has already been decided on.
    // Safe from overflow: this is only called once DecideExact has already established the
    // true value is small (either one of the two fixed tiny shapes, or a value that passed
    // the budget check above).
    private static long EstimateExactWork(ulong hero, int boardCardsLeft, ulong[]? singleRange, int[]? multiRangeSize)
    {
        int heroCardCount = PEval.bitCount(hero);
        long boardCombosCount = BinomialCoefficient(47 - heroCardCount + boardCardsLeft, boardCardsLeft);

        long villainSpace = 1;
        if (multiRangeSize is not null)
        {
            foreach (int size in multiRangeSize) villainSpace *= size;
        }
        else if (singleRange is not null)
        {
            villainSpace = singleRange.Length;
        }
        else
        {
            villainSpace = BinomialCoefficient(47 - heroCardCount, heroCardCount);
        }

        return boardCombosCount * villainSpace;
    }

    // Enumerates every remaining board completion exactly, splitting the (materialized)
    // board-combo list across worker-pool jobs - the enumeration counterpart to RunBatch.
    // Partitioning respects MinCalculationsPerWorker (unless the whole batch is one slice to
    // begin with) on top of the same load-based parallelism target Monte Carlo uses.
    private async Task<SimulationResult> RunExactBatch(
        ulong hero, ulong board, int boardCardsLeft, int villainCount,
        ulong[]? singleRange, ulong[,]? multiRange, int[]? multiRangeSize,
        GameType game, CancellationToken cancellationToken)
    {
        ulong[] allBoardCombos = PEval.EnumerateCardCombinations(hero | board, boardCardsLeft).ToArray();

        long totalWork = EstimateExactWork(hero, boardCardsLeft, singleRange, multiRangeSize);
        int loadDivisor = ComputeLoadDivisor(_pool.PendingCount, _options.MaxQueueSize, _options.MaxLoadDivisor);
        int desiredParts = Math.Max(1, _options.Parallelism / loadDivisor);
        int maxPartsByFloor = (int)Math.Max(1, totalWork / MinCalculationsPerWorker);
        int parts = Math.Max(1, Math.Min(desiredParts, maxPartsByFloor));

        List<ulong[]> slices = PokerExactEnumerator.PartitionBoardCombos(allBoardCombos, parts);

        if (!_pool.TryReserve(slices.Count))
            throw new ServiceBusyException();

        var tasks = new List<Task<SimulationResult>>(slices.Count);

        foreach (var slice in slices)
        {
            var tcs = new TaskCompletionSource<SimulationResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            tasks.Add(tcs.Task);

            void Callback(SimulationResult result) => tcs.TrySetResult(result);

            void Job(PokerMonteCarloServer _, PokerExactEnumerator enumerator)
            {
                try
                {
                    if (multiRange is not null)
                    {
                        enumerator.Configure(hero, board, slice, multiRange, multiRangeSize!, villainCount, Callback, cancellationToken, game);
                        enumerator.EnumerateRangeN();
                    }
                    else if (singleRange is not null)
                    {
                        enumerator.Configure(hero, board, slice, singleRange, singleRange.Length, Callback, cancellationToken, game);
                        enumerator.EnumerateRange();
                    }
                    else
                    {
                        enumerator.Configure(hero, board, slice, Callback, cancellationToken, game);
                        enumerator.Enumerate();
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            _pool.Enqueue(Job);
        }

        var results = await Task.WhenAll(tasks).WaitAsync(cancellationToken);
        return results.Aggregate(SimulationResult.Empty, (acc, r) => acc + r);
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

            void Job(PokerMonteCarloServer server, PokerExactEnumerator _)
            {
                try
                {
                    if (multiRange is not null)
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, multiRange, multiRangeSize!, villainCount, Callback, cancellationToken, game, _options.MaxDealAttempts);
                        server.SimulateRangeN();
                    }
                    else if (singleRange is not null)
                    {
                        server.Configure(hero, (ulong)sims, board, boardCardsLeft, singleRange, singleRange.Length, Callback, cancellationToken, game, _options.MaxDealAttempts);
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

    private static EquityResponse ToResponse(SimulationResult result, long simulations, bool fromCache, bool exact)
    {
        double total = result.Win + result.Loss + result.Tie;
        if (total == 0)
            return new EquityResponse(0, 0, 0, 0, simulations, fromCache, exact);

        // Loss/tie are rounded first; hero's win percentage absorbs whatever rounding
        // slack that leaves behind, so the three always sum to exactly 100 instead of
        // drifting by a fraction of a percent once each is rounded independently.
        double lossPercent = Math.Round(result.Loss / total * 100, PercentDecimals);
        double tiePercent = Math.Round(result.Tie / total * 100, PercentDecimals);
        double winPercent = Math.Round(100 - lossPercent - tiePercent, PercentDecimals);

        double equity = Math.Round((result.Win + result.TieEquity) / total * 100, PercentDecimals);

        return new EquityResponse(winPercent, lossPercent, tiePercent, equity, simulations, fromCache, exact);
    }
}
