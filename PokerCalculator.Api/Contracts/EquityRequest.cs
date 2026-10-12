namespace PokerCalculator.Api.Contracts;

// Hero's hole cards: 2 for Hold'em, 4 for Omaha, e.g. "AhKs" or "AhKsQdJc".
// Board cards, 0, 3, 4 or 5 cards, e.g. "" / "2h7c9d" / "2h7c9dTs".
// One entry per villain. Each entry is a list of range tokens - Hold'em notation (e.g.
// "AA", "AKs", "76o") or Omaha notation (e.g. "AAKK", exact "AhKsQdJc") depending on Game.
// An empty list for a villain means "unknown hand" (uniform random). Omit entirely for
// a single unknown villain. Game defaults to Hold'em so existing callers of POST
// /api/equity keep working unchanged.
// AllowExactEnumeration: true (default) lets EquityService pick exact enumeration over
// Monte Carlo when it decides that's cheaper/warranted. Set to false to force Monte Carlo
// regardless - used to compare both paths on the same input (requests with it set to
// false bypass the cache, so the comparison always sees a fresh computation).
public record EquityRequest(string Hero, string Board, List<List<string>>? VillainRanges, GameType Game = GameType.Holdem, bool AllowExactEnumeration = true);
