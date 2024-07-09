using PokerCalculator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // this is god, but slower
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Enumerate5Cards(ulong heroCards, ulong villainCards, out int win, out int loss, out int tie)
        {

            ulong c0, c1, c2, c3, c4;

            ulong max0 = CONSTANTS.ONE << 51;
            ulong min4 = 0;
            ulong min3 = min4 << 1;
            ulong min2 = min3 << 1;
            ulong min1 = min2 << 1;
            ulong min0 = min1 << 1;

            ulong cardset = 0;
            
            ulong allInitialCards = heroCards | villainCards;

            win = 0; loss = 0; tie = 0;

            for (c0 = max0; c0 > min0; c0 >>= 1)
            {
                cardset |= c0;
                for (c1 = c0 >> 1; c1 > min1; c1 >>= 1)
                {
                    cardset |= c1;
                    for (c2 = c1 >> 1; c2 > min2; c2 >>= 1)
                    {
                        cardset |= c2;
                        for (c3 = c2 >> 1; c3 > min3; c3 >>= 1)
                        {
                            cardset |= c3;
                            for (c4 = c3 >> 1; c4 > min4; c4 >>= 1)
                            {
                                cardset |= c4;
                                if ((cardset & allInitialCards) == 0)
                                {
                                    int heroResult = ProcessCardSet(heroCards, cardset);
                                    int villainResult = ProcessCardSet(villainCards, cardset);
                                    if (heroResult > villainResult) win++;
                                    else if (heroResult < villainResult) loss++;
                                    else tie++;
                                }
                                cardset ^= c4;
                            }
                            cardset ^= c3;
                        }
                        cardset ^= c2;
                    }
                    cardset ^= c1;
                }
                cardset ^= c0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Enumerate2Cards(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {

            ulong c0, c1;

            ulong max0 = CONSTANTS.ONE << 51;
            ulong min1 = 0;
            ulong min0 = min1 << 1;

            ulong cardset = 0;
            ulong allInitialCards = heroCards | villainCards | boardCards;

            win = 0; loss = 0; tie = 0;

            for (c0 = max0; c0 > min0; c0 >>= 1)
            {
                cardset |= c0;
                for (c1 = c0 >> 1; c1 > min1; c1 >>= 1)
                {
                    cardset |= c1;
                    if ((cardset & allInitialCards) == 0)
                    {
                        int heroResult = ProcessCardSet(heroCards, cardset | boardCards);
                        int villainResult = ProcessCardSet(villainCards, cardset | boardCards);
                        if (heroResult > villainResult) win++;
                        else if (heroResult < villainResult) loss++;
                        else tie++;
                    }
                    cardset ^= c1;
                }
                cardset ^= c0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnumerateOneCard(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {

            ulong c0;

            ulong max0 = CONSTANTS.ONE << 51;
            ulong min0 = 0;

            ulong cardset = 0;

            ulong allInitialCards = heroCards | villainCards | boardCards;

            win = 0; loss = 0; tie = 0;

            for (c0 = max0; c0 > min0; c0 >>= 1)
            {
                cardset |= c0;
                if ((cardset & allInitialCards) == 0)
                {
                    int heroResult = ProcessCardSet(heroCards, cardset | boardCards);
                    int villainResult = ProcessCardSet(villainCards, cardset | boardCards);
                    if (heroResult > villainResult) win++;
                    else if (heroResult < villainResult) loss++;
                    else tie++;
                }
                cardset ^= c0;
            }
        }

        //this option is faster
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnumerateFast(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {
            win = 0; loss = 0; tie = 0;
            int boardCount = bitCount(boardCards);
            if (boardCount == 0) Enumerate5Cards(heroCards, villainCards, out win, out loss, out tie);
            else if (boardCount == 3) Enumerate2Cards(heroCards, villainCards, boardCards, out win, out loss, out tie);
            else if (boardCount == 4) EnumerateOneCard(heroCards, villainCards,boardCards, out win, out loss, out tie);
        }
    }


}
