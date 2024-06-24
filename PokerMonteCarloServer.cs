using System;
using System.Threading;

namespace PokerCalculator
{
    public delegate void simulCallBack(ulong w, ulong l, ulong t, float eqTie);

    class PokerMonteCarloServer
    {
        ulong herohand;
        ulong currentBoard;
        int boardCardsLeft;

        ulong nSimul;

        ulong win, tie, lost;
        float eqTie;


        ulong[] range;
        int rangeSize;
        int nVillains;
        ulong[,] rangeN;
        int[] rangeSizeN;

        private simulCallBack cb;

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, simulCallBack callb)
        {
            herohand = h;
            nSimul = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            cb = callb;
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[] r, int rS, simulCallBack callb)
        {
            herohand = h;
            nSimul = n;
            currentBoard = currboard;
            boardCardsLeft = bcl;
            range = r;
            rangeSize = rS;
            cb = callb;
        }

        public PokerMonteCarloServer(ulong h, ulong n, ulong currboard, int bcl, ulong[,] r, int[] rS, int nV, simulCallBack callb)
        {
            herohand = h;
            nSimul = n;
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

            for (ulong i = 0; i < nSimul; i++)
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
            eqTie = (float)tie / 2;

            if (cb != null) cb(win, lost, tie, eqTie);

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
            eqTie = 0;

            for (ulong i = 0; i < nSimul; i++)
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

            eqTie = (float)tie / 2;

            if (cb != null) cb(win, lost, tie, eqTie);

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
            eqTie = 0;

            for (ulong i = 0; i < nSimul; i++)
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
                    eqTie += (float)(1 / ((float)nTies + 1));
                }
            }

            if (cb != null) cb(win, lost, tie, eqTie);

        }

    }
}
