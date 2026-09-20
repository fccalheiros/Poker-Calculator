using PokerCalculator.Api.Options;

namespace PokerCalculator.Api.Contracts;

public record CacheStatus(int Count, int SizeLimit);

public record UsageSnapshot(
    DateTimeOffset StartedAt,
    double UptimeSeconds,
    long TotalRequests,
    long HoldemRequests,
    long OmahaRequests,
    long CacheHits,
    long CacheMisses,
    long RejectedBusy,
    long ValidationErrors,
    double HoldemAvgComputeMs,
    double OmahaAvgComputeMs);

public record AppStatusResponse(SimulationOptions Config, CacheStatus Cache, UsageSnapshot Usage);
