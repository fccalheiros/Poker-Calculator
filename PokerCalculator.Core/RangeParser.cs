// Converts textual poker range notation into the bitmask hand combinations consumed by
// PEval.RandomHandRange. ParseHoldemRange reads two-card notation (e.g. "AKs", "AKo",
// "TT", "76"): a pair has 6 combos, a suited hand has 4, an offsuit hand has 12, and a
// rank pair with no suffix has all 16 combined. ParseOmahaRange reads four-card notation:
// either an exact combo (e.g. "AhKdQsJc") or a bare 4-rank pattern (e.g. "AAKK", "AKQJ"),
// optionally followed by a suitedness suffix - "r" (rainbow), "s" (single suited) or "ds"
// (double suited) - classified by how many distinct suits the 4 cards use (4/3/<=2). No
// suffix means every valid suit assignment for that rank multiset.
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerCalculator
{
    public static class RangeParser
    {
        // Parses a list of range tokens into a deduplicated array of two-card combos.
        public static ulong[] ParseHoldemRange(IEnumerable<string> tokens)
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
                    throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));

                int rank1 = PEval.CardRank(token[0]);
                int rank2 = PEval.CardRank(token[1]);
                if (rank1 < 0 || rank2 < 0)
                    throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));

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
                    throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));
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

        // Parses a list of range tokens into a deduplicated array of four-card combos.
        public static ulong[] ParseOmahaRange(IEnumerable<string> tokens)
        {
            var combos = new HashSet<ulong>();

            foreach (var rawToken in tokens)
            {
                var token = rawToken.Trim();
                if (token.Length == 0) continue;

                // Exact combo, e.g. "AhKdQsJc".
                if (token.Length == 8)
                {
                    combos.Add(PEval.ConvertStringToCardSet(token));
                    continue;
                }

                // Bare rank pattern, e.g. "AAKK", "AKQJ" - one char per hole card - with an
                // optional suitedness suffix: "AAKKr", "AAKKs", "AAKKds".
                if (token.Length < 4 || token.Length > 6)
                    throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));

                string suffix = token.Length > 4 ? token.Substring(4).ToLowerInvariant() : string.Empty;
                if (suffix != string.Empty && suffix != "r" && suffix != "s" && suffix != "ds")
                    throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));

                var ranks = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    ranks[i] = PEval.CardRank(token[i]);
                    if (ranks[i] < 0)
                        throw new FormatException(Messages.RangeParsing.InvalidRangeToken(rawToken));
                }

                foreach (var combo in RankPatternCombos(ranks))
                {
                    if (MatchesSuitQualifier(combo, suffix))
                        combos.Add(combo);
                }
            }

            return combos.ToArray();
        }

        // No suffix matches everything. Otherwise classifies a combo by how many distinct
        // suits its 4 cards use: rainbow (r) uses all 4 suits, single suited (s) uses
        // exactly 3 (one shared-suit pair), double suited (ds) uses 2 or fewer (two
        // shared-suit pairs - or, only reachable with an all-distinct rank pattern like
        // AKQJ, three or four cards sharing a suit).
        private static bool MatchesSuitQualifier(ulong combo, string suffix)
        {
            if (suffix == string.Empty) return true;

            int distinctSuits = CountDistinctSuits(combo);
            return suffix switch
            {
                "r" => distinctSuits == 4,
                "s" => distinctSuits == 3,
                "ds" => distinctSuits <= 2,
                _ => false
            };
        }

        private static int CountDistinctSuits(ulong combo)
        {
            int count = 0;
            for (int s = 0; s < 4; s++)
                if (((combo >> (s * 13)) & 0x1FFFUL) != 0) count++;
            return count;
        }

        // Expands a 4-rank pattern (repeats allowed, e.g. AAKK or AAAK) into every 4-card
        // combo using exactly those ranks: every valid way to pick distinct suits for each
        // repeated rank, combined across ranks.
        private static IEnumerable<ulong> RankPatternCombos(int[] ranks)
        {
            IEnumerable<ulong> combos = new ulong[] { 0 };

            foreach (var group in ranks.GroupBy(r => r))
            {
                int rank = group.Key;
                int count = group.Count();
                combos = from prefix in combos
                         from suits in SuitCombinations(count)
                         select suits.Aggregate(prefix, (acc, s) => acc | (CONSTANTS.ONE << (rank + s * 13)));
            }

            return combos;
        }

        // Every way to pick `count` distinct suits (0-3), as arrays of suit indices.
        private static IEnumerable<int[]> SuitCombinations(int count)
        {
            for (int mask = 0; mask < 16; mask++)
            {
                if (PEval.bitCount(mask) != count) continue;

                var suits = new List<int>(count);
                for (int s = 0; s < 4; s++)
                    if ((mask & (1 << s)) != 0) suits.Add(s);
                yield return suits.ToArray();
            }
        }
    }
}
