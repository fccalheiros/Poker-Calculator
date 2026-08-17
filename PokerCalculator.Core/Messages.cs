// Centralizes every caller-facing text string this project's active parsing/evaluation
// code produces, so wording changes happen in one place instead of scattered across throw
// sites. Kept separate from PokerCalculator.Api's Messages class - this project's messages
// are about hand/range parsing, the API's are about request-shape validation.
namespace PokerCalculator
{
    public static class Messages
    {
        public static class RangeParsing
        {
            public static string InvalidRangeToken(string token) => $"Invalid range token: '{token}'";
        }
    }
}
