using System;
using System.Threading;

namespace PokerCalculator
{
    public delegate void simulCallBack(ulong w, ulong l, ulong t, float tieEquity);

    class PokerMonteCarloServer
    {
        private readonly ulong herohand;
        private readonly ulong currentBoard;
        private readonly int boardCardsLeft;
        private readonly ulong numberOfSimulations;

        private ulong win, tie, lost;
        private float tieEquity;

        private readonly ulong[] range;
        private readonly int rangeSize;
        private readonly int nVillains;
        private readonly ulong[,] rangeN;
        private readonly int[] rangeSizeN;

        private readonly simulCallBack cb;

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, simulCallBack callb)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            cb = callb;
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[] r, int rS, simulCallBack callb)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            range = r;
            rangeSize = rS;
            cb = callb;
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[,] r, int[] rS, int nV, simulCallBack callb)
        {
            herohand = h;
            numberOfSimulations = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            rangeN = r;
            rangeSizeN = rS;
            nVillains = nV;
            cb = callb;
        }


        // The method that will be called when the thread is started.
        public void Simulate()
        {
            ulong villainhand;
            ulong board;
            int heroResult;
            int villainResult;

            Thread.Sleep(1);
            Random R = new Random(DateTime.Now.Millisecond);

            win = 0; tie = 0; lost = 0;

            for (ulong i = 0; i < numberOfSimulations; i++)
            {

                PEval.RandomHand(herohand, currentBoard, boardCardsLeft, out board, out villainhand, R);
                heroResult = PEval.ProcessCardSet(herohand | board);
                villainResult = PEval.ProcessCardSet(villainhand | board);

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;

            }
            tieEquity = (float)tie / 2;

            if (cb != null) cb(win, lost, tie, tieEquity);

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

                PEval.RandomHandRange(herohand, currentBoard, boardCardsLeft, range, rangeSize, out board, out villainhand, R);

                heroResult = PEval.ProcessCardSet(herohand | board);
                villainResult = PEval.ProcessCardSet(villainhand | board);

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;

            }

            tieEquity = (float)tie / 2;

            if (cb != null) cb(win, lost, tie, tieEquity);

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

                PEval.RandomHandRange(herohand, currentBoard, boardCardsLeft, nVillains, rangeN, rangeSizeN, out board, out villainhand, R);

                heroResult = PEval.ProcessCardSet(herohand | board);
                bestvillainResult = 0;

                for (int v = 0; v < nVillains; v++)
                {
                    villainResult[v] = PEval.ProcessCardSet(villainhand[v] | board);
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

            if (cb != null) cb(win, lost, tie, tieEquity);

        }

    }
}
