// Tuning knobs for the simulation pool: worker thread count, admission queue size,
// how many simulations to run per request at full capacity, how far simulation count
// and parallelism are allowed to scale down as the queue fills up (MaxLoadDivisor), and
// the minimum simulation count worth caching.
namespace PokerCalculator.Api.Options;

public class SimulationOptions
{
    public const string SectionName = "Simulation";

    public int WorkerCount { get; set; } = 4;
    public int MaxQueueSize { get; set; } = 200;
    public int Parallelism { get; set; } = 4;
    public long FullSimulations { get; set; } = 200_000;
    public int MaxLoadDivisor { get; set; } = 8;
    public long MinSimulationsToCache { get; set; } = 50_000;
    public int CacheSizeLimit { get; set; } = 5_000;
}
