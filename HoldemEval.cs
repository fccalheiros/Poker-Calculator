using PokerCalculator;
using System;
using System.Runtime.CompilerServices;

namespace PokerCalculator
{
    class HoldemEval : PEval
    {

        // Evaluate a complete card set.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessCardSet(ulong cardSet)
        {
            return PEval.GeneralProcessCardSet(cardSet);
        }

        // Evaluate pocket cards with the current board.
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

            // Villain first card.
            next = R.Next(0, CONSTANTS.CARDS_TOTAL);
            n = CONSTANTS.ONE << next;
            while ((allCards & n) > 0)
            {
                next = R.Next(0, CONSTANTS.CARDS_TOTAL);
                n = CONSTANTS.ONE << next;
            }
            allCards |= n;

            // Villain second card.
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

        // Draw one villain hand from a range, then draw the rest of the board.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomHandRange(ulong heroCards, ulong currentBoard, int boardCardsLeft, ulong[] range, int rangesize, out ulong boardCards, out ulong villainCards, Random R)
        {
            ulong n;
            ulong allCards;
            int j = 0;

            allCards = heroCards | currentBoard;

            // Villain pocket cards.
            villainCards = range[R.Next(0, rangesize)];
            while ((allCards & villainCards) > 0)
            {
                villainCards = range[R.Next(0, rangesize)];
            }

            allCards |= villainCards;

            // Board cards.
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

        // Draw one hand per villain from each range, then draw the rest of the board.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RandomHandRange(ulong heroCards, ulong currentBoard, int boardCardsLeft, int nVillains, ulong[,] range, int[] rangesize, out ulong boardCards, out ulong[] villainCards, Random R)
        {
            ulong n;
            ulong allCards = 0;
            int j = 0;
            villainCards = new ulong[nVillains];


            int v;
            bool TryAgain = true;

            // Villain pocket pairs.
            while (TryAgain)
            {
                TryAgain = false;
                v = 0;
                allCards = heroCards | currentBoard;
                // If a duplicate card is drawn, the process must start over to avoid errors in the probability calculations.
                // This may lead to infinite loops with many players and small, overlapping ranges.
                while (!TryAgain && v < nVillains)
                {
                    villainCards[v] = range[v, R.Next(0, rangesize[v])];
                    TryAgain = (allCards & villainCards[v]) > 0;
                    allCards |= villainCards[v];
                    v++;
                }

            }

            boardCards = currentBoard;
            // Board cards.
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

        public static MatchupResult SimulateMatchup(ulong heroHand, ulong currentBoard, int boardCardsLeft, Random random)
        {
            ulong board;
            ulong villainHand;

            RandomHand(heroHand, currentBoard, boardCardsLeft, out board, out villainHand, random);
            int heroResult = ProcessCardSet(heroHand, board);
            int villainResult = ProcessCardSet(villainHand, board);

            if (heroResult > villainResult)
                return MatchupResult.Win;

            if (heroResult < villainResult)
                return MatchupResult.Loss;

            return MatchupResult.Tie;
        }

        // The board must have 0, 3, or 4 cards.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enumerate(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {
            win = 0; loss = 0; tie = 0;

            int i, j;
            int boardCount = bitCount(boardCards);
            int cardsLeft = 5 - bitCount(boardCards);
            int[] iterators = new int[6];
            ulong enumeratedCards;
            ulong allInitialCards = heroCards | villainCards | boardCards;


            for (i = 1; i < 6; i++) { iterators[i] = 53; }
            i = 0;
            iterators[0] = 0;

            while (!((i == 0) && (iterators[0] + 1 == iterators[1])))
            {
                iterators[i]++;
                for (j = i + 1; j < cardsLeft; j++) { iterators[j] = iterators[j - 1] + 1; }
                i = cardsLeft - 1;

                enumeratedCards = 0;
                // Build the missing board cards.
                for (j = 0; j < cardsLeft; j++) {
                    enumeratedCards |= CONSTANTS.ONE << (iterators[j] - 1);
                }
                // Skip combinations with repeated cards.
                if ((enumeratedCards & allInitialCards) == 0)
                {
                    int heroResult = ProcessCardSet(heroCards, enumeratedCards | boardCards);
                    int villainResult= ProcessCardSet(villainCards, enumeratedCards | boardCards);
                    if (heroResult > villainResult) win++;
                    else if (heroResult < villainResult) loss++;
                    else tie++;
                }

                while ((i > 0) & (iterators[i] + 1 == iterators[i + 1]))
                    i = i - 1;
                
            }


        }

        // This is good, but slower.
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

        // This option is faster.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnumerateFast(ulong heroCards, ulong villainCards, ulong boardCards, out int win, out int loss, out int tie)
        {
            win = 0; loss = 0; tie = 0;
            int boardCount = bitCount(boardCards);
            if (boardCount == 0) Enumerate5Cards(heroCards, villainCards, out win, out loss, out tie);
            else if (boardCount == 3) Enumerate2Cards(heroCards, villainCards, boardCards, out win, out loss, out tie);
            else if (boardCount == 4) EnumerateOneCard(heroCards, villainCards, boardCards, out win, out loss, out tie);
        }
    }


}
