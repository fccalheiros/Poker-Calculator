// Converts textual poker range notation (e.g. "AKs", "AKo", "TT", "76") into the
// bitmask hand combinations consumed by HoldemEval.RandomHandRange. Each token expands
// into every concrete two-card combo it represents: a pair has 6 combos, a suited hand
// has 4, an offsuit hand has 12, and a rank pair with no suffix has all 16 combined.
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerCalculator
{
    public static class RangeParser
    {
        // Parses a list of range tokens into a deduplicated array of two-card combos.
        public static ulong[] ParseRange(IEnumerable<string> tokens)
        {
            var combos = new HashSet<ulong>();

            foreach (var rawToken in tokens)
            {
                var token = rawToken.Trim();
                if (token.Length == 0) continue;

                // Exact combo, e.g. "AhKd".
                if (token.Length == 4)
                {
                    combos.Add(PEval.ConvertStringToCardSet(token));
                    continue;
                }

                if (token.Length < 2 || token.Length > 3)
                    throw new FormatException($"Invalid range token: '{rawToken}'");

                int rank1 = PEval.CardRank(token[0]);
                int rank2 = PEval.CardRank(token[1]);
                if (rank1 < 0 || rank2 < 0)
                    throw new FormatException($"Invalid range token: '{rawToken}'");

                char suffix = token.Length == 3 ? char.ToLowerInvariant(token[2]) : '\0';

                if (rank1 == rank2)
                {
                    foreach (var combo in PairCombos(rank1))
                        combos.Add(combo);
                    continue;
                }

                if (suffix == 's' || suffix == '\0')
                {
                    foreach (var combo in SuitedCombos(rank1, rank2))
                        combos.Add(combo);
                }

                if (suffix == 'o' || suffix == '\0')
                {
                    foreach (var combo in OffsuitCombos(rank1, rank2))
                        combos.Add(combo);
                }

                if (suffix != 's' && suffix != 'o' && suffix != '\0')
                    throw new FormatException($"Invalid range token: '{rawToken}'");
            }

            return combos.ToArray();
        }

        private static IEnumerable<ulong> PairCombos(int rank)
        {
            for (int s1 = 0; s1 < 4; s1++)
                for (int s2 = s1 + 1; s2 < 4; s2++)
                    yield return (CONSTANTS.ONE << (rank + s1 * 13)) | (CONSTANTS.ONE << (rank + s2 * 13));
        }

        private static IEnumerable<ulong> SuitedCombos(int rank1, int rank2)
        {
            for (int s = 0; s < 4; s++)
                yield return (CONSTANTS.ONE << (rank1 + s * 13)) | (CONSTANTS.ONE << (rank2 + s * 13));
        }

        private static IEnumerable<ulong> OffsuitCombos(int rank1, int rank2)
        {
            for (int s1 = 0; s1 < 4; s1++)
                for (int s2 = 0; s2 < 4; s2++)
                {
                    if (s1 == s2) continue;
                    yield return (CONSTANTS.ONE << (rank1 + s1 * 13)) | (CONSTANTS.ONE << (rank2 + s2 * 13));
                }
        }
    }
}
