// Builds a canonical cache key so equivalent requests (same cards/ranges in a different
// order or letter case) hit the same cache entry. Uses PEval.OrderStringCardSet, the same
// canonicalizer Distribution.cs already relies on for hero/board strings.
using System.Linq;
using System.Text;

namespace PokerCalculator.Api.Simulation;

public static class EquityCacheKeyBuilder
{
    public static string Build(string hero, string board, List<List<string>> villainRanges)
    {
        var sb = new StringBuilder();
        sb.Append(PEval.OrderStringCardSet(hero));
        sb.Append('|');
        sb.Append(PEval.OrderStringCardSet(board));

        foreach (var range in villainRanges)
        {
            sb.Append('|');
            var sorted = range.Select(t => t.Trim().ToUpperInvariant()).OrderBy(t => t, StringComparer.Ordinal);
            sb.Append(string.Join(',', sorted));
        }

        return sb.ToString();
    }
}
