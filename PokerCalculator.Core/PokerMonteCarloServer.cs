// Runs a batch of Monte Carlo simulations (heads-up, range vs. hand, or range vs. range)
// on its own thread and reports win/loss/tie counts through a callback when done. Supports
// both Hold'em and Omaha via the GameType passed to Configure (defaults to Hold'em).
using System;
using System.Threading;

namespace PokerCalculator
{
    public delegate void simulCallBack(SimulationResult result);

    public class PokerMonteCarloServer
    {
        private ulong herohand;
        private ulong currentBoard;
        private int boardCardsLeft;
        private ulong numberOfSimulations;
        private GameType gameType;

        private ulong win, tie, lost;
        private float tieEquity;

        private ulong[] range;
        private int rangeSize;
        private int nVillains;
        private ulong[,] rangeN;
        private int[] rangeSizeN;
        private int maxDealAttempts;

        private simulCallBack cb;
        private CancellationToken cancellationToken;

        // Fallback used only when a caller doesn't pass its own value (see Configure
        // overloads below); PokerCalculator.Api always supplies its configured value.
        private const int DefaultMaxDealAttempts = 1000;

        // A server with nothing configured yet; useful when a worker thread owns one
        // long-lived instance and reconfigures it via Configure(...) for each job.
        public PokerMonteCarloServer()
        {
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, simulCallBack callb)
        {
            Configure(h, n, currboard, bcl, callb);
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[] r, int rS, simulCallBack callb)
        {
            Configure(h, n, currboard, bcl, r, rS, callb);
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[,] r, int[] rS, int nV, simulCallBack callb)
        {
            Configure(h, n, currboard, bcl, r, rS, nV, callb);
        }

        // Reconfigures this instance for a heads-up simulation against a uniformly random villain hand.
        // The cancellation token is checked periodically during the run so an abandoned request
        // (e.g. the caller disconnected) stops early instead of grinding through every simulation.
        public void Configure(ulong h, ulong n, ulong currboard, int bcl, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            cb = callb;
            cancellationToken = token;
            gameType = game;
        }

        // Reconfigures this instance for a simulation against a single villain range.
        // maxDealAttempts bounds PEval.RandomHandRange's collision-retry loop - see its
        // doc comment and UnsatisfiableRangeException.
        public void Configure(ulong h, ulong n, ulong currboard, int bcl, ulong[] r, int rS, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem, int maxDealAttempts = DefaultMaxDealAttempts)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            range = r;
            rangeSize = rS;
            cb = callb;
            cancellationToken = token;
            gameType = game;
            this.maxDealAttempts = maxDealAttempts;
        }

        // Reconfigures this instance for a simulation against multiple villain ranges.
        public void Configure(ulong h, ulong n, ulong currboard, int bcl, ulong[,] r, int[] rS, int nV, simulCallBack callb, CancellationToken token = default, GameType game = GameType.Holdem, int maxDealAttempts = DefaultMaxDealAttempts)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            rangeN = r;
            rangeSizeN = rS;
            nVillains = nV;
            cb = callb;
            cancellationToken = token;
            gameType = game;
            this.maxDealAttempts = maxDealAttempts;
        }

        // How often (in iterations) the simulation loops poll the cancellation token.
        // Checking every iteration would add measurable overhead to a hot loop; this keeps
        // the check cheap while still reacting within a few thousand simulations.
        private const ulong CancellationCheckInterval = 4096;


        // The method that will be called when the thread is started.
        public void Simulate()
        {
            Thread.Sleep(1);
            Random R = new Random(DateTime.Now.Millisecond);
            ulong[] results = new ulong[(int)MatchupResult.Count];
            Array.Clear(results, 0, results.Length);

            win = 0; tie = 0; lost = 0;

            for (ulong i = 0; i < numberOfSimulations; i++)
            {
                if (i % CancellationCheckInterval == 0 && cancellationToken.IsCancellationRequested) return;

                results[(int)(gameType == GameType.Omaha
                    ? OmahaEval.SimulateMatchup(herohand, currentBoard, boardCardsLeft, R)
                    : HoldemEval.SimulateMatchup(herohand, currentBoard, boardCardsLeft, R))]++;
            }
            win = results[(int)MatchupResult.Win];
            lost = results[(int)MatchupResult.Loss];
            tie = results[(int)MatchupResult.Tie];
            tieEquity = (float)tie / 2;

            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));

        }

        public void SimulateRange()
        {
            ulong villainhand;
            ulong board;
            int heroResult;
            int villainResult;

            Thread.Sleep(1);
            Random R = new Random(DateTime.Now.Millisecond);

            win = 0; tie = 0; lost = 0;
            tieEquity = 0;

            for (ulong i = 0; i < numberOfSimulations; i++)
            {
                if (i % CancellationCheckInterval == 0 && cancellationToken.IsCancellationRequested) return;

                PEval.RandomHandRange(herohand, currentBoard, boardCardsLeft, range, rangeSize, maxDealAttempts, out board, out villainhand, R);

                if (gameType == GameType.Omaha)
                {
                    heroResult = OmahaEval.ProcessCardSet(herohand, board);
                    villainResult = OmahaEval.ProcessCardSet(villainhand, board);
                }
                else
                {
                    heroResult = HoldemEval.ProcessCardSet(herohand, board);
                    villainResult = HoldemEval.ProcessCardSet(villainhand, board);
                }

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;

            }

            tieEquity = (float)tie / 2;

            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));

        }

        public void SimulateRangeN()
        {
            ulong[] villainhand;
            ulong board;
            int heroResult;
            int bestvillainResult;
            int[] villainResult;


            Thread.Sleep(1);
            villainResult = new int[nVillains];
            villainhand = new ulong[nVillains];
            Random R = new Random(DateTime.Now.Millisecond);

            win = 0; tie = 0; lost = 0;
            tieEquity = 0;

            for (ulong i = 0; i < numberOfSimulations; i++)
            {
                if (i % CancellationCheckInterval == 0 && cancellationToken.IsCancellationRequested) return;

                PEval.RandomHandRange(herohand, currentBoard, boardCardsLeft, nVillains, rangeN, rangeSizeN, maxDealAttempts, out board, out villainhand, R);

                heroResult = gameType == GameType.Omaha
                    ? OmahaEval.ProcessCardSet(herohand, board)
                    : HoldemEval.ProcessCardSet(herohand, board);
                bestvillainResult = 0;

                for (int v = 0; v < nVillains; v++)
                {
                    villainResult[v] = gameType == GameType.Omaha
                        ? OmahaEval.ProcessCardSet(villainhand[v], board)
                        : HoldemEval.ProcessCardSet(villainhand[v], board);
                    if (villainResult[v] > bestvillainResult) bestvillainResult = villainResult[v];
                }


                if (heroResult > bestvillainResult)
                    win++;
                else if (heroResult < bestvillainResult)
                    lost++;
                else
                {
                    tie++;
                    int nTies = 0;
                    for (int v = 0; v < nVillains; v++)
                    {
                        if (heroResult == villainResult[v])
                        {
                            nTies++;
                        }

                    }
                    tieEquity += (float)(1 / ((float)nTies + 1));
                }
            }

            cb?.Invoke(new SimulationResult(win, lost, tie, tieEquity));

        }

    }
}
