namespace PokerCalculator.Api.Contracts;

// Same fields as EquityRequest minus Game - bound by the /api/equity/holdem and
// /api/equity/omaha convenience routes, where the game is implied by the URL rather than
// a request field.
public record GameSpecificEquityRequest(string Hero, string Board, List<List<string>>? VillainRanges)
{
    public EquityRequest WithGame(GameType game) => new(Hero, Board, VillainRanges, game);
}
