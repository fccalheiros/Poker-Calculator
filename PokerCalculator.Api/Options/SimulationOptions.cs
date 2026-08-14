// Tuning knobs for the simulation pool: worker thread count, admission queue size,
// how many simulations to run per request (full vs. reduced under load), how many
// sub-tasks to split a request into, and the minimum simulation count worth caching.
namespace PokerCalculator.Api.Options;

public class SimulationOptions
{
    public const string SectionName = "Simulation";

    public int WorkerCount { get; set; } = 4;
    public int MaxQueueSize { get; set; } = 200;
    public int Parallelism { get; set; } = 4;
    public long FullSimulations { get; set; } = 200_000;
    public long ReducedSimulations { get; set; } = 20_000;
    public long MinSimulationsToCache { get; set; } = 50_000;
    public int CacheSizeLimit { get; set; } = 5_000;
}
