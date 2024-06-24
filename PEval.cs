using System;
using System.Runtime.CompilerServices;

namespace PokerCalculator
{
    public static class CONSTANTS
    {

        public static int[] MultiplyDeBruijnBitPosition = { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 };
        public static ulong ONE = 1;

        public static int CARDS_TOTAL = 52;

        public static int HIGH_CARD = 0x0000000;
        public static int PAIR = 0x4000000;
        public static int TWOPAIR = 0x8000000;
        public static int TRIPS = 0xC000000;
        public static int STRAIGHT = 0x10000000;
        public static int FLUSH = 0x14000000;
        public static int FULLHOUSE = 0x18000000;
        public static int FOUR = 0x1C000000;
        public static int STRAIGHTFLUSH = 0x20000000;

    }

    class PEval
    {

        //52 bit number each representing a card on the deck
        //From left to right 2-A  and Clubs, Hearts, Spades, Diamonds
        //private ulong _cardSet;

        //adapted from pokerstove gitlab project
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int bitCount(int subset)
        {
            // subset - 13 bits from right to left 2 - A
            int c;
            for (c = 0; subset > 0; c++)
            {
                subset &= subset - 1;
            }
            return c;
        }

        // Cleans lower significant bit
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int cleanLSB(int subset)
        {
            // subset - 13 bits from right to left 2 - A

            return subset & (subset - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int cleanLSB2(int subset)
        {
            // subset - 13 bits from right to left 2 - A 
            int c = subset & (subset - 1);
            return c & (c - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int cleanLSB(int subset, int times)
        {
            // subset - 13 bits from right to left 2 - A
            int c = subset;

            for (int i = 0; i < times; i++)
            {
                c &= c - 1;
            }
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int lastbit(int subset)
        {
            return CONSTANTS.MultiplyDeBruijnBitPosition[((UInt32)((subset & -subset) * 0x077CB531U)) >> 27];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int straight(int subset)
        {
            //replicate A at the end to find lower sequences
            subset = ((subset & 0b1000000000000) >> 12) | (subset << 1);

            //subset &= subset << 1;
            subset &= subset << 2;
            subset &= subset << 1;
            subset &= subset << 1;

            subset >>= 1;

            if (subset == 0) return 0;

            // now set all bits after the Most Significant 
            subset |= subset >> 1;
            subset |= subset >> 2;
            subset |= subset >> 4;
            subset |= subset >> 8;

            return subset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int flush(int C, int H, int S, int D, out int bitcc)
        {
            if ((bitcc = bitCount(C)) >= 5) return C;
            if ((bitcc = bitCount(H)) >= 5) return H;
            if ((bitcc = bitCount(S)) >= 5) return S;
            if ((bitcc = bitCount(D)) >= 5) return D;
            return 0;
        }

        // works better than the previous when testing all hands.
        public static int flush2(int C, int H, int S, int D, out int bitcc)
        {
            bitcc = bitCount(C);

            if (bitcc >= 5) return C;
            if (bitcc > 2) return 0;

            int acum = bitcc;

            bitcc = bitCount(H);
            if (bitcc >= 5) return H;
            acum += bitcc;
            if (acum > 2) return 0;

            bitcc = bitCount(S);
            if (bitcc >= 5) return S;
            if (acum + bitcc > 2) return 0;

            bitcc = 7 - acum - bitcc;
            return D;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ProcessCardSet(ulong cardSet)
        {
            // uses bit 29,28 and 27 to store the hand strength
            // uses 26 bits to tiebreaker. How?
            // Straights - last 13 bits - all bits from the highest card are set to 1
            // Ace High or flush - last 13 bits - 5 highest cards are set  to 1
            // Four, Full, Trips, Two Pair
            //     bits 26-14 - the highest value game bits are set to 1 
            //     last 13 bits - the bits of tiebreaker cards are set to 1

            int C, H, S, D;
            int CHSD_OR;
            int cc, flushcc;
            int st, fl, stfl;


            C = (int)((cardSet) & 0b1111111111111);
            H = (int)((cardSet >> 13) & 0b1111111111111);
            S = (int)((cardSet >> 26) & 0b1111111111111);
            D = (int)((cardSet >> 39) & 0b1111111111111);

            CHSD_OR = C | H | S | D;
            cc = bitCount(CHSD_OR);

            if (cc >= 5)
            {
                st = straight(CHSD_OR);
                fl = flush2(C, H, S, D, out flushcc);

                if ((st & fl) > 0)
                {
                    stfl = straight(fl);
                    if (stfl > 0)
                    {
                        // second term treats royal flush
                        return CONSTANTS.STRAIGHTFLUSH | ((stfl & 0x1000) << 14) | stfl;
                    }
                }
                if (fl > 0)
                {
                    return CONSTANTS.FLUSH | cleanLSB(fl, flushcc - 5);
                }
                if (st > 0)
                {
                    return CONSTANTS.STRAIGHT | st;
                }

                // Pair
                if (cc == 6)
                {
                    int pair = (C ^ H ^ S ^ D) ^ CHSD_OR;
                    return CONSTANTS.PAIR | pair << 13 | cleanLSB2(pair ^ CHSD_OR);
                }

                //  High Card
                if (cc == 7)
                {
                    //cleant two LSB
                    return CONSTANTS.HIGH_CARD | cleanLSB2(CHSD_OR);
                }

                //Trips or 2 pais
                if (cc == 5)
                {
                    //C & H & S | C & H & D | C & S & D | H & S & D;
                    int trips = (C & H & (S | D)) | (S & D & (C | H));
                    if (trips > 0) return CONSTANTS.TRIPS | trips << 13 | cleanLSB2(trips ^ CHSD_OR);
                    int pair = (C ^ H ^ S ^ D) ^ CHSD_OR;
                    return CONSTANTS.TWOPAIR | pair << 13 | cleanLSB2(pair ^ CHSD_OR);
                }

            }

            //cc < 5

            // Four or full "clean" or "3" pairs
            if (cc == 4)
            {
                int four = C & H & S & D;

                if (four > 0) return CONSTANTS.FOUR | four << 13 | cleanLSB2(four ^ CHSD_OR);

                int trips = (C & H & (S | D)) | (S & D & (C | H));
                int pair = (C ^ H ^ S ^ D) ^ CHSD_OR;
                if (trips > 0) return CONSTANTS.FULLHOUSE | trips << 13 | pair;

                int hightwopair = cleanLSB(pair);
                return CONSTANTS.TWOPAIR | hightwopair << 13 | cleanLSB(hightwopair ^ CHSD_OR);

            }

            //four + "pair" or full + "2 trips" or full + two pair
            if (cc == 3)
            {
                int four = C & H & S & D;
                if (four > 0) return CONSTANTS.FOUR | four << 13 | cleanLSB(four ^ CHSD_OR);

                int trips = (C & H & (S | D)) | (S & D & (C | H));
                int pair = (C ^ H ^ S ^ D) ^ CHSD_OR;

                if (pair == 0)
                {
                    int highcard = cleanLSB(trips);
                    return CONSTANTS.FULLHOUSE | highcard << 13 | highcard ^ trips;
                }
                return CONSTANTS.FULLHOUSE | trips << 13 | cleanLSB(pair);
            }

            // four + "trips"
            if (cc == 2)
            {
                int four = C & H & S & D;
                return CONSTANTS.FOUR | four << 13 | (four ^ CHSD_OR);
            }

            // we could never get here
            return 0;
        }

        //Hero against random villain hand
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

        //Hero against range villain hand
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

        //Hero against N range villain hand
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

        // cards in array from 1 to 52 A to K
        // Card set must be from 2 to A
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ConvertArrayToCardSet(ref int[] c)
        {
            ulong cardset = 0;
            int card;

            for (int i = 0; i < c.Length; i++)
            {
                card = c[i];
                card--;
                if (card % 13 == 0) card += 12;
                else card--;

                cardset |= CONSTANTS.ONE << card;
            }

            return cardset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ConvertArrayToCardSet(ref int[] c, int end)
        {
            ulong cardset = 0;
            int card;

            for (int i = 0; i < end; i++)
            {
                card = c[i];
                card--;
                if (card % 13 == 0) card += 12;
                else card--;

                cardset |= CONSTANTS.ONE << card;
            }

            return cardset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CardRank(char c)
        {
            int rank;
            string s = c + " ";
            switch (c)
            {
                case 'A': rank = 12; break;
                case 'K': rank = 11; break;
                case 'Q': rank = 10; break;
                case 'J': rank = 9; break;
                case 'T': rank = 8; break;
                case 'a': rank = 12; break;
                case 'k': rank = 11; break;
                case 'q': rank = 10; break;
                case 'j': rank = 9; break;
                case 't': rank = 8; break;
                default:
                    {
                        rank = -1;
                        if (int.TryParse(s, out int i))
                        {
                            if (i <= 9 && i >= 2)
                                rank = i - 2;
                        }
                        break;
                    }
            }
            return rank;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CardSuit(char c)
        {
            int suit;
            switch (c)
            {
                case 'C': suit = 0; break;
                case 'H': suit = 1; break;
                case 'S': suit = 2; break;
                case 'D': suit = 3; break;
                case 'c': suit = 0; break;
                case 'h': suit = 1; break;
                case 's': suit = 2; break;
                case 'd': suit = 3; break;
                default: suit = -1; break;
            }
            return suit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ConvertStringToCardSet(string s)
        {
            ulong cardset = 0;
            int rank;
            int suit = 0;

            s = s.Replace(" ", "").ToUpper();

            for (int i = 0; i < s.Length; i += 2)
            {
                rank = CardRank(s[i]);
                suit = CardSuit(s[i + 1]);
                cardset |= CONSTANTS.ONE << (suit * 13 + rank);
            }
            return cardset;
        }

        public static string OrderStringCardSet(string s)
        {
            string res = "";
            s = s.Replace(" ", "").ToUpper();
            while (s.Length > 0)
            {
                int highrank = -1;
                string card = "";
                for (int i = 0; i < s.Length / 2; i++)
                {
                    int next = 10 * CardRank(s[2 * i]) + CardSuit(s[2 * i + 1]);
                    if (next > highrank)
                    {
                        highrank = next;
                        card = s.Substring(2 * i, 2);
                    }
                }
                res += card[0] + card.Substring(1, 1).ToLower() + " ";
                s = s.Replace(card, "");
            }


            return res;
        }

        public static bool IsValidStringCardSet(string s)
        {
            bool valid = true;
            s = s.Replace(" ", "").ToUpper();
            while (valid && s.Length > 1)
            {
                valid = (CardRank(s[0]) >= 0 && CardSuit(s[1]) >= 0);
                s = s.Substring(2);
            }
            if (s.Length > 0)
                valid = valid & CardRank(s[0]) >= 0;
            return valid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(ulong cardset)
        {
            string s = "";
            int rank = 0;
            int suit = 0;
            while (cardset > 0)
            {
                if ((cardset & 1) != 0)
                {
                    switch (rank)
                    {
                        case 12: s += "A"; break;
                        case 11: s += "K"; break;
                        case 10: s += "Q"; break;
                        case 9: s += "J"; break;
                        case 8: s += "T"; break;
                        default: s += (rank + 2).ToString(); break;
                    }
                    switch (suit)
                    {
                        case 0: s += "c"; break;
                        case 1: s += "h"; break;
                        case 2: s += "s"; break;
                        case 3: s += "d"; break;
                    }

                }
                rank++;
                rank %= 13;
                if (rank == 0) suit++;
                cardset >>= 1;
            }

            return s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReturnHandPower(int handNumber)
        {
            return (handNumber >> 26);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] EvaluateAllCombinations()
        {
            int[] res = new int[10];
            ulong c0, c1, c2, c3, c4, c5, c6;

            ulong max0 = CONSTANTS.ONE << 46;
            ulong max1 = max0 << 1;
            ulong max2 = max1 << 1;
            ulong max3 = max2 << 1;
            ulong max4 = max3 << 1;
            ulong max5 = max4 << 1;
            ulong max6 = max5 << 1;

            ulong cardset = 0;

            for (int i = 0; i < 10; i++) res[i] = 0;

            for (c0 = 1; c0 < max0; c0 <<= 1)
            {
                cardset |= c0;
                for (c1 = c0 << 1; c1 < max1; c1 <<= 1)
                {
                    cardset |= c1;
                    for (c2 = c1 << 1; c2 < max2; c2 <<= 1)
                    {
                        cardset |= c2;
                        for (c3 = c2 << 1; c3 < max3; c3 <<= 1)
                        {
                            cardset |= c3;
                            for (c4 = c3 << 1; c4 < max4; c4 <<= 1)
                            {
                                cardset |= c4;
                                for (c5 = c4 << 1; c5 < max5; c5 <<= 1)
                                {
                                    cardset |= c5;
                                    for (c6 = c5 << 1; c6 < max6; c6 <<= 1)
                                    {
                                        cardset |= c6;

                                        res[PEval.ReturnHandPower(PEval.ProcessCardSet(cardset))]++;

                                        cardset ^= c6;
                                    }
                                    cardset ^= c5;
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


            return res;
        }


    }
}
