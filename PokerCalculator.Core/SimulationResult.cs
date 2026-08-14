namespace PokerCalculator
{
    // Aggregate outcome of a batch of Monte Carlo simulations. Shared by PokerMonteCarloServer's
    // callback and by any caller that needs to combine results from multiple batches.
    public record SimulationResult(ulong Win, ulong Loss, ulong Tie, float TieEquity)
    {
        public static SimulationResult Empty => new(0, 0, 0, 0);

        public static SimulationResult operator +(SimulationResult a, SimulationResult b) =>
            new(a.Win + b.Win, a.Loss + b.Loss, a.Tie + b.Tie, a.TieEquity + b.TieEquity);
    }
}
