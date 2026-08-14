namespace PokerCalculator.Api.Contracts;

// Hero's two hole cards, e.g. "AhKs".
// Board cards, 0, 3 or 4 cards, e.g. "" / "2h7c9d" / "2h7c9dTs".
// One entry per villain. Each entry is a list of range tokens (e.g. "AA", "AKs", "76o").
// An empty list for a villain means "unknown hand" (uniform random). Omit entirely for
// a single unknown villain.
public record EquityRequest(string Hero, string Board, List<List<string>>? VillainRanges);
