// Exact-equity counterpart to PokerMonteCarloServer: instead of sampling random deals,
// walks every remaining board completion x villain-hand assignment exhaustively and
// returns the true win/loss/tie counts (zero variance). Only safe to use where that space
// is small - callers are responsible for deciding when that's the case and for generating
// the board-combination list (PEval.EnumerateCardCombinations) and splitting it across
// worker-pool jobs (PartitionBoardCombos) exactly like PokerMonteCarloServer's simulation
// count is split today. Not wired into EquityService yet - Monte Carlo still handles every
// request; this class exists standalone until that decision is made.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PokerCalculator
{
    public class PokerExactEnumerator
    {
        private ulong herohand;
        private ulong currentBoard;
        private ulong[] boardCombos;
        private GameType gameType;

        private ulong win, tie, lost;
        private float tieEquity;

        private ulong[] range;
        private int rangeSize;

        private int nVillains;
        // Jagged, not the rectangular ulong[,] PokerMonteCarloServer uses - Configure sorts
        // villains ascending by range size here (smallest outermost), so a villain that
        // beats hero prunes the largest possible remaining subtree. See design notes: this
        // also lowers the no-prune baseline cost, since it keeps the running cross-product
        // smaller for longer.
        private ulong[][] rangesByVillain;
        private int[] villainResultScratch;

        private simulCallBack cb;
        private CancellationToken cancellationToken;

        // Same cadence as PokerMonteCarloServer.CancellationCheckInterval - checked across
        // both board combos and the (potentially much larger) villain-hand combos nested
        // inside each one.
        private const int CancellationCheckInterval = 4096;

        // Reconfigures this instance to enumerate against a uniformly random (unknown)
        // villain hand - the exact counterpart to PokerMonteCarloServer.Simulate().
        public void Configure(ulong h, ulong currboard, ulong[] combos, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem)
        {
            herohand = h;
            currentBoard = currboard;
            boardCombos = combos;
            cb = callb;
            cancellationToken = token;
            gameType = game;
        }

        // Reconfigures this instance to enumerate against a single villain range - the
        // exact counterpart to PokerMonteCarloServer.SimulateRange().
        public void Configure(ulong h, ulong currboard, ulong[] combos, ulong[] r, int rS, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem)
        {
            herohand = h;
            currentBoard = currboard;
            boardCombos = combos;
            range = r;
            rangeSize = rS;
            cb = callb;
            cancellationToken = token;
            gameType = game;
        }

        // Reconfigures this instance to enumerate against multiple villain ranges - the
        // exact counterpart to PokerMonteCarloServer.SimulateRangeN().
        public void Configure(ulong h, ulong currboard, ulong[] combos, ulong[,] r, int[] rS, int nV, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem)
        {
            herohand = h;
            currentBoard = currboard;
            boardCombos = combos;
            nVillains = nV;
            cb = callb;
            cancellationToken = token;
            gameType = game;

            int[] order = Enumerable.Range(0, nV).OrderBy(v => rS[v]).ToArray();
            rangesByVillain = new ulong[nV][];
            for (int i = 0; i < nV; i++)
            {
                int v = order[i];
                rangesByVillain[i] = new ulong[rS[v]];
                for (int c = 0; c < rS[v]; c++)
                    rangesByVillain[i][c] = r[v, c];
            }
        }

        // Splits combos into at most `parts` contiguous, roughly equal slices, one per
        // worker-pool job - the enumeration equivalent of EquityService.SplitEvenly, which
        // splits a simulation count instead of an already-materialized combo list. A combo
        // list of size 1 (the boardCardsLeft == 0 case: nothing left to complete) yields a
        // single slice regardless of `parts` - deliberately not worth parallelizing on its
        // own, see design notes.
        public static List<ulong[]> PartitionBoardCombos(ulong[] combos, int parts)
        {
            var result = new List<ulong[]>();
            if (combos.Length == 0) return result;

            parts = Math.Max(1, Math.Min(parts, combos.Length));
            int baseShare = combos.Length / parts;
            int remainder = combos.Length % parts;

            int offset = 0;
            for (int i = 0; i < parts; i++)
            {
                int share = baseShare + (i < remainder ? 1 : 0);
                if (share == 0) continue;
                var slice = new ulong[share];
                Array.Copy(combos, offset, slice, 0, share);
                result.Add(slice);
                offset += share;
            }
            return result;
        }

        public void Enumerate()
        {
            win = 0; tie = 0; lost = 0; tieEquity = 0;

            ulong[] heroStripped = null;
            if (gameType == GameType.Omaha) OmahaEval.StripCardSet(herohand, out heroStripped);

            int villainCardCount = PEval.bitCount(herohand);
            int checked_ = 0;

            foreach (ulong boardCombo in boardCombos)
            {
                ulong board = currentBoard | boardCombo;
                ulong[] boardStripped = null;
                if (gameType == GameType.Omaha) OmahaEval.StripCardSet(board, out boardStripped);

                int heroResult = gameType == GameType.Omaha
                    ? OmahaEval.ProcessCardSet(heroStripped, boardStripped)
                    : HoldemEval.ProcessCardSet(herohand, board);

                ulong forbidden = herohand | board;
                foreach (ulong villainHand in PEval.EnumerateCardCombinations(forbidden, villainCardCount))
                {
                    if (checked_++ % CancellationCheckInterval == 0 && cancellationToken.IsCancellationRequested) return;

                    int villainResult;
                    if (gameType == GameType.Omaha)
                    {
                        OmahaEval.StripCardSet(villainHand, out ulong[] villainStripped);
                        villainResult = OmahaEval.ProcessCardSet(villainStripped, boardStripped);
                    }
                    else
                    {
                        villainResult = HoldemEval.ProcessCardSet(villainHand, board);
                    }

                    if (heroResult > villainResult) win++;
                    else if (heroResult < villainResult) lost++;
                    else tie++;
                }
            }

            tieEquity = (float)tie / 2;
            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));
        }

        public void EnumerateRange()
        {
            win = 0; tie = 0; lost = 0; tieEquity = 0;

            ulong[] heroStripped = null;
            if (gameType == GameType.Omaha) OmahaEval.StripCardSet(herohand, out heroStripped);

            int checked_ = 0;

            foreach (ulong boardCombo in boardCombos)
            {
                ulong board = currentBoard | boardCombo;
                ulong[] boardStripped = null;
                if (gameType == GameType.Omaha) OmahaEval.StripCardSet(board, out boardStripped);

                int heroResult = gameType == GameType.Omaha
                    ? OmahaEval.ProcessCardSet(heroStripped, boardStripped)
                    : HoldemEval.ProcessCardSet(herohand, board);

                ulong usedMask = herohand | board;

                for (int c = 0; c < rangeSize; c++)
                {
                    if (checked_++ % CancellationCheckInterval == 0 && cancellationToken.IsCancellationRequested) return;

                    ulong villainHand = range[c];
                    if ((villainHand & usedMask) != 0) continue; // collides with hero/board, not a real combo here

                    int villainResult;
                    if (gameType == GameType.Omaha)
                    {
                        OmahaEval.StripCardSet(villainHand, out ulong[] villainStripped);
                        villainResult = OmahaEval.ProcessCardSet(villainStripped, boardStripped);
                    }
                    else
                    {
                        villainResult = HoldemEval.ProcessCardSet(villainHand, board);
                    }

                    if (heroResult > villainResult) win++;
                    else if (heroResult < villainResult) lost++;
                    else tie++;
                }
            }

            tieEquity = (float)tie / 2;
            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));
        }

        public void EnumerateRangeN()
        {
            win = 0; tie = 0; lost = 0; tieEquity = 0;
            villainResultScratch = new int[nVillains];

            ulong[] heroStripped = null;
            if (gameType == GameType.Omaha) OmahaEval.StripCardSet(herohand, out heroStripped);

            foreach (ulong boardCombo in boardCombos)
            {
                if (cancellationToken.IsCancellationRequested) return;

                ulong board = currentBoard | boardCombo;
                ulong[] boardStripped = null;
                if (gameType == GameType.Omaha) OmahaEval.StripCardSet(board, out boardStripped);

                // Hero's hand is fixed for this board, independent of every villain
                // assignment checked below it - evaluated once here, reused for all of them.
                int heroResult = gameType == GameType.Omaha
                    ? OmahaEval.ProcessCardSet(heroStripped, boardStripped)
                    : HoldemEval.ProcessCardSet(herohand, board);

                ulong usedMask = herohand | board;

                EvaluateVillain(0, usedMask, heroResult, board, boardStripped);
            }

            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));
        }

        // Walks villain villainIdx's range (already sorted ascending by size in Configure).
        // Each villain's hand is evaluated once per node at its own depth and reused by
        // every combination nested below it - never recomputed on the way down. A villain
        // that beats hero decides the rest of this branch (loss, regardless of what the
        // unassigned villains hold), so evaluation switches to CountRemainingCombos instead
        // of recursing normally; a tie or a hero-still-ahead result can't take that shortcut
        // - a later villain could still turn a tie into a loss, or change how many-way the
        // tie is, so every remaining villain still has to be evaluated for real.
        private void EvaluateVillain(int villainIdx, ulong usedMask, int heroResult, ulong board, ulong[] boardStripped)
        {
            if (villainIdx == nVillains)
            {
                int best = 0;
                for (int v = 0; v < nVillains; v++)
                    if (villainResultScratch[v] > best) best = villainResultScratch[v];

                if (heroResult > best)
                    win++;
                else // heroResult == best: no villain here ever beat hero, by construction
                {
                    tie++;
                    int nTies = 0;
                    for (int v = 0; v < nVillains; v++)
                        if (villainResultScratch[v] == heroResult) nTies++;
                    tieEquity += (float)(1 / ((float)nTies + 1));
                }
                return;
            }

            foreach (ulong combo in rangesByVillain[villainIdx])
            {
                if ((combo & usedMask) != 0) continue; // collides with cards already committed

                int villainResult;
                if (gameType == GameType.Omaha)
                {
                    OmahaEval.StripCardSet(combo, out ulong[] villainStripped);
                    villainResult = OmahaEval.ProcessCardSet(villainStripped, boardStripped);
                }
                else
                {
                    villainResult = HoldemEval.ProcessCardSet(combo, board);
                }

                if (villainResult > heroResult)
                {
                    lost += CountRemainingCombos(villainIdx + 1, usedMask | combo);
                    continue;
                }

                villainResultScratch[villainIdx] = villainResult;
                EvaluateVillain(villainIdx + 1, usedMask | combo, heroResult, board, boardStripped);
            }
        }

        // Counts valid (collision-free) completions for villains fromVillainIndex..N-1
        // without evaluating any of their hands - used once a villain has already beaten
        // hero and the outcome for the rest of this branch is settled as a loss either way.
        private ulong CountRemainingCombos(int fromVillainIndex, ulong usedMask)
        {
            if (fromVillainIndex == nVillains) return 1;

            ulong total = 0;
            foreach (ulong combo in rangesByVillain[fromVillainIndex])
            {
                if ((combo & usedMask) != 0) continue;
                total += CountRemainingCombos(fromVillainIndex + 1, usedMask | combo);
            }
            return total;
        }
    }
}
