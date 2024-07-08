using PokerCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokerCalculator
{
     class HoldemEval : PEval
    {


        //Given hero and partial (or not) board cards, draw villain hand and the rest of the board
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static int ProcessCardSet(ulong cardSet) {
            return PEval.GeneralProcessCardSet(cardSet);
        }

        //Given hero and partial (or not) board cards, draw villain hand and the rest of the board
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessCardSet(ulong pocketpair, ulong board)
        {
            return PEval.GeneralProcessCardSet(pocketpair | board);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomHand(ulong heroCards, ulong currentBoard, int boardCardsLeft, out ulong boardCards, out ulong villainCards, Random R)
        {
            int next;
            ulong n;
            ulong allCards;
            int j = 0;

            allCards = heroCards | currentBoard;

            while (j < boardCardsLeft)
            {
                next = R.Next(0, CONSTANTS.CARDS_TOTAL);
                n = CONSTANTS.ONE << next;
                while ((allCards & n) > 0)
                {
                    next = R.Next(0, CONSTANTS.CARDS_TOTAL);
                    n = CONSTANTS.ONE << next;
                }
                allCards |= n;
                j++;
            }
            boardCards = allCards ^ heroCards;

            //villain first card
            next = R.Next(0, CONSTANTS.CARDS_TOTAL);
            n = CONSTANTS.ONE << next;
            while ((allCards & n) > 0)
            {
                next = R.Next(0, CONSTANTS.CARDS_TOTAL);
                n = CONSTANTS.ONE << next;
            }
            allCards |= n;

            //villain second card
            next = R.Next(0, CONSTANTS.CARDS_TOTAL);
            n = CONSTANTS.ONE << next;
            while ((allCards & n) > 0)
            {
                next = R.Next(0, CONSTANTS.CARDS_TOTAL);
                n = CONSTANTS.ONE << next;
            }
            allCards |= n;

            villainCards = allCards ^ heroCards ^ boardCards;

        }

        //Given hero and partial (or not) board cards and a villain guess hand, draw villain hand and the rest of the board

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomHandRange(ulong heroCards, ulong currentBoard, int boardCardsLeft, ulong[] range, int rangesize, out ulong boardCards, out ulong villainCards, Random R)
        {
            ulong n;
            ulong allCards;
            int j = 0;

            allCards = heroCards | currentBoard;

            //Vilain pocket cards
            villainCards = range[R.Next(0, rangesize)];
            while ((allCards & villainCards) > 0)
            {
                villainCards = range[R.Next(0, rangesize)];
            }

            allCards |= villainCards;

            //board
            while (j < boardCardsLeft)
            {
                n = CONSTANTS.ONE << R.Next(0, CONSTANTS.CARDS_TOTAL);
                while ((allCards & n) > 0)
                {
                    n = CONSTANTS.ONE << R.Next(0, CONSTANTS.CARDS_TOTAL);
                }
                allCards |= n;
                j++;
            }

            boardCards = allCards ^ heroCards ^ villainCards;

        }

        //Do the same as the previous for more than one villain
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomHandRange(ulong heroCards, ulong currentBoard, int boardCardsLeft, int nVillains, ulong[,] range, int[] rangesize, out ulong boardCards, out ulong[] villainCards, Random R)
        {
            ulong n;
            ulong allCards = 0;
            int j = 0;
            villainCards = new ulong[nVillains];


            int v;
            bool TryAgain = true;

            //Vilains pocket pair
            while (TryAgain)
            {
                TryAgain = false;
                v = 0;
                allCards = heroCards | currentBoard;
                // If a duplicate card is drawn, the process must start over to avoid errors in the probability calculations.
                // this may lead to infinite loops with large number of players with small and equal card range
                while (!TryAgain && v < nVillains)
                {
                    villainCards[v] = range[v, R.Next(0, rangesize[v])];
                    TryAgain = (allCards & villainCards[v]) > 0;
                    allCards |= villainCards[v];
                    v++;
                }

            }

            boardCards = currentBoard;
            //board
            while (j < boardCardsLeft)
            {
                n = CONSTANTS.ONE << R.Next(0, CONSTANTS.CARDS_TOTAL);
                while ((allCards & n) > 0)
                {
                    n = CONSTANTS.ONE << R.Next(0, CONSTANTS.CARDS_TOTAL);
                }
                allCards |= n;
                boardCards |= n;
                j++;
            }

        }
    }


}
