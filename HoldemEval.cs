using PokerCalculator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        // board must have 0, 3 or 4 cards
        public static void Enumerate(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {
            win = 0; loss = 0; tie = 0;

            int i,j;
            int boardCount = bitCount(boardCards);
            int cardsLeft = 5 - bitCount(boardCards);
            int[] iterators = new int[6];
            ulong enumeratedCards;
            ulong allInitialCards = heroCards | villainCards| boardCards;


            for (i = 1; i < 6; i++) { iterators[i] = 53; }
            i = 0;
            iterators[0] = 0;

            while (  ! ((i == 0) && (iterators[0]+1 == iterators[1])) )
            {
                iterators[i]++;
                for (j = i + 1; j < cardsLeft; j++) { iterators[j] = iterators[j-1] + 1; }
                i = cardsLeft - 1;

                enumeratedCards = 0;
                // construct the hole board
                for ( j = 0; j < cardsLeft; j++) { 
                    enumeratedCards |= (CONSTANTS.ONE << iterators[j]-1);
                }
                // otherwise there are repeated cards
                if ((enumeratedCards & allInitialCards) == 0)
                {
                    int heroResult = ProcessCardSet(heroCards, enumeratedCards | boardCards);
                    int villainResult= ProcessCardSet(villainCards, enumeratedCards | boardCards);
                    if (heroResult > villainResult) win++;
                    else if (heroResult < villainResult) loss++;
                    else tie++;
                }

                while( (i>0) & (iterators[i]+1 == iterators[i+1]))  
                    i = i - 1;
                
            }


        }
    }


}
