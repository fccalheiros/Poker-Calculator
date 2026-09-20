// In-process counters only - reset on restart, no time-series/history. Same reasoning as
// EquityCache not needing anything fancier than IMemoryCache's own eviction: this is a
// status endpoint for a small, low-traffic app, not a metrics pipeline.
using System.Diagnostics;
using PokerCalculator.Api.Contracts;

namespace PokerCalculator.Api.Simulation;

public class UsageStats
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;
    private long _totalRequests;
    private long _holdemRequests;
    private long _omahaRequests;
    private long _cacheHits;
    private long _cacheMisses;
    private long _rejectedBusy;
    private long _validationErrors;
    private long _holdemComputeTicks;
    private long _holdemComputeCount;
    private long _omahaComputeTicks;
    private long _omahaComputeCount;

    public void RecordRequest(GameType game)
    {
        Interlocked.Increment(ref _totalRequests);
        if (game == GameType.Omaha)
            Interlocked.Increment(ref _omahaRequests);
        else
            Interlocked.Increment(ref _holdemRequests);
    }

    public void RecordCacheHit() => Interlocked.Increment(ref _cacheHits);
    public void RecordCacheMiss() => Interlocked.Increment(ref _cacheMisses);
    public void RecordRejectedBusy() => Interlocked.Increment(ref _rejectedBusy);
    public void RecordValidationError() => Interlocked.Increment(ref _validationErrors);

    // elapsedTicks: Stopwatch.ElapsedTicks (Stopwatch's own tick rate, via Stopwatch.Frequency -
    // NOT DateTime/TimeSpan ticks) for one full (parallel) batch - never per sub-task, and
    // never for a cache hit (those don't run the batch at all, and would skew the average
    // down misleadingly if mixed in).
    public void RecordComputeTime(GameType game, long elapsedTicks)
    {
        if (game == GameType.Omaha)
        {
            Interlocked.Add(ref _omahaComputeTicks, elapsedTicks);
            Interlocked.Increment(ref _omahaComputeCount);
        }
        else
        {
            Interlocked.Add(ref _holdemComputeTicks, elapsedTicks);
            Interlocked.Increment(ref _holdemComputeCount);
        }
    }

    public UsageSnapshot Snapshot() => new(
        _startedAt,
        (DateTimeOffset.UtcNow - _startedAt).TotalSeconds,
        Interlocked.Read(ref _totalRequests),
        Interlocked.Read(ref _holdemRequests),
        Interlocked.Read(ref _omahaRequests),
        Interlocked.Read(ref _cacheHits),
        Interlocked.Read(ref _cacheMisses),
        Interlocked.Read(ref _rejectedBusy),
        Interlocked.Read(ref _validationErrors),
        AverageMs(Interlocked.Read(ref _holdemComputeTicks), Interlocked.Read(ref _holdemComputeCount)),
        AverageMs(Interlocked.Read(ref _omahaComputeTicks), Interlocked.Read(ref _omahaComputeCount)));

    private static double AverageMs(long totalTicks, long count) =>
        count == 0 ? 0 : (double)totalTicks / count / Stopwatch.Frequency * 1000;
}
