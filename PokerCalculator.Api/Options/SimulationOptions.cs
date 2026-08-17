// Tuning knobs for the simulation pool: worker thread count, admission queue size,
// how many simulations to run per request at full capacity, how far simulation count
// and parallelism are allowed to scale down as the queue fills up (MaxLoadDivisor), how
// much cheaper a starting point Omaha gets since each hand costs far more to evaluate
// than Hold'em (OmahaSimulationDivisor), the minimum simulation count worth caching, and
// request-shape limits (max villains, max range tokens per villain, max body size) that
// reject abusive payloads before they cost any real CPU/memory.
namespace PokerCalculator.Api.Options;

public class SimulationOptions
{
    public const string SectionName = "Simulation";

    public int WorkerCount { get; set; } = 4;
    public int MaxQueueSize { get; set; } = 200;
    public int Parallelism { get; set; } = 4;
    public long FullSimulations { get; set; } = 200_000;
    public int MaxLoadDivisor { get; set; } = 8;
    public int OmahaSimulationDivisor { get; set; } = 2;
    public long MinSimulationsToCache { get; set; } = 50_000;
    public int CacheSizeLimit { get; set; } = 5_000;

    // A real table seats at most ~9-10 players (8-9 villains). Omaha gets a lower ceiling
    // than Hold'em on both fronts: SimulateRangeN's per-simulation cost grows with villain
    // count regardless of game, and a single Omaha range token can expand into up to 256
    // combos (a bare 4-distinct-rank pattern) versus Hold'em's worst case of 16, so the
    // same token count is far more expensive to parse for Omaha.
    public int MaxVillainsHoldem { get; set; } = 8;
    public int MaxVillainsOmaha { get; set; } = 4;
    public int MaxRangeTokensPerVillainHoldem { get; set; } = 200;
    public int MaxRangeTokensPerVillainOmaha { get; set; } = 100;

    public long MaxRequestBodyBytes { get; set; } = 512 * 1024;
}
