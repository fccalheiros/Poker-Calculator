namespace PokerCalculator.Api.Contracts;

public record EquityResponse(
    double WinPercent,
    double LossPercent,
    double TiePercent,
    double Equity,
    long SimulationsRun,
    bool FromCache);
