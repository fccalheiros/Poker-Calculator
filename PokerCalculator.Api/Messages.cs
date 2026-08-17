// Centralizes every caller-facing text string this project produces (validation error
// messages returned as 400 bodies), so wording changes happen in one place instead of
// scattered across throw sites. Kept separate from PokerCalculator.Core's Messages class -
// this project's messages are about API request-shape validation, Core's are about
// hand/range parsing.
namespace PokerCalculator.Api
{
    public static class Messages
    {
        public static class Validation
        {
            public static string InvalidCards(string fieldName, string value) =>
                $"Invalid {fieldName} cards: '{value}'";

            public static string WrongHeroCardCount(int expected, GameType game, string hero) =>
                $"Hero must have exactly {expected} distinct cards for {game}: '{hero}'";

            public static string InvalidBoardCardCount(string board) =>
                $"Board must have 0, 3, 4 or 5 distinct cards: '{board}'";

            public const string HeroBoardOverlap = "Hero and board cannot share a card.";

            public static string TooManyVillains(int count, int max, GameType game) =>
                $"Too many villains: {count} (max {max} for {game}).";

            public static string TooManyRangeTokens(int count, int max, GameType game) =>
                $"Too many range tokens for one villain: {count} (max {max} for {game}).";

            public const string EmptyVillainRangeInMultiway =
                "All villain ranges must be non-empty when more than one villain is specified.";
        }
    }
}
