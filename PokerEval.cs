using System;

namespace Confidencial
{
    class PokerEval
    {

        private readonly int[] MAP = { 7, 8, 4, 5, 0, 1, 2, 9, 3, 6 };
        private readonly int[] MAP7CARDS = { 6, 4, 5, 7, 7, 8, 9, 0, 1, 2, 2, 3, 6, 6 };
        private readonly ulong[] DISAMB_ARRAY = { HIGH_CARD_DISAMB, PAIR_DISAMB, PAIR_DISAMB, TRIPS_DISAMB, 0, HIGH_CARD_DISAMB, TRIPS_DISAMB, FOUR_DISAMB, 0, 0 };

        private readonly int[] DISAMB_SHIFT = { 0, 1, 1, 2, 0, 0, 2, 3, 0, 0 };

        private readonly bool[] DISAMB_NEED = { true, true, true, true, false, true, true, true, false, false };
        private readonly ulong[] DISAMB_CARDS_GAME = { 5, 1, 2, 1, 5, 5, 1, 1, 5, 5 };
        private readonly ulong[] DISAMB_CARDS_LEFT = { 0, 3, 1, 2, 0, 0, 1, 1, 0, 0 };


        private const ulong CLUBS_MASK = 1;
        private const ulong HEARTS_MASK = 8192;
        private const ulong SPADES_MASK = 67108864;
        private const ulong DIAMONDS_MASK = 549755813888;

        private const ulong UM = 1;
        private const ulong MAX_NUM = 4503599627370495;

        private const ulong HIGH_CARD_DISAMB = 300239975158033;
        private const ulong SEQ_DISAMB = 281474976710656;
        private const ulong PAIR_DISAMB = 600479950316066;
        private const ulong TRIPS_DISAMB = 1200959900632132;
        private const ulong FOUR_DISAMB = 2401919801264264;

        private const int MIN_SEQ_DISAMB = 4095;
        private const int SEQ_PATTERN = 4111;
        private const int MAX_SEQ_PATTERN = 7936;
        private const int MAX_SEQ_PATTERN_HBIT = 1 << 12;

        private const int HALF_MAX_SEQ_PATTERN = 3968;

        private const ulong INIT_FLUSH = 1649468792835;
        private const ulong FLUSHMASK = 4398583447560;

        private const int DECK_CARDS_NUMBER = 52;

        // cards --- 1 to 52 
        private int[] _cards; public int[] Cards { get { return _cards; } set { _cards = value; } }
        private int[] _cardsValues; public int[] CardsValues { get { return _cardsValues; } set { _cardsValues = value; } }

        private ulong[] _cardsSuits; public ulong[] CardsSuits { get { return _cardsSuits; } set { _cardsSuits = value; } }

        private ulong _handNumber; public ulong HandNumber { get { return _handNumber; } set { _handNumber = value; } }

        private int _sequenceNumber; public int SequenceNumber { get { return _sequenceNumber; } set { _sequenceNumber = value; } }

        private ulong _flushNumber; public ulong FlushNumber { get { return _flushNumber; } set { _flushNumber = value; } }

        private int _handPower; public int HandPower { get { return _handPower; } set { _handPower = value; } }

        private bool _sequenceFound; public bool SequenceFound { get { return _sequenceFound; } set { _sequenceFound = value; } }

        public PokerEval()
        {
            _cards = new int[7];
            _cardsValues = new int[7];
            _cardsSuits = new ulong[7];
        }

        public void Copy(PokerEval p, int count)
        {
            _handNumber = p.HandNumber;
            _sequenceNumber = p.SequenceNumber;
            _flushNumber = p.FlushNumber;
            for (int i = 0; i < count; i++)
            {
                _cards[i] = p.Cards[i];
                _cardsValues[i] = p.CardsValues[i];
                _cardsSuits[i] = p.CardsSuits[i];
            }
        }

        public bool Compare(PokerEval p)
        {
            if (_handNumber != p.HandNumber) return false;
            if (_sequenceNumber != p.SequenceNumber) return false;
            if (_flushNumber != p.FlushNumber) return false;
            if (_handPower != p.HandPower) return false;
            if (_sequenceFound != p.SequenceFound) return false;
            for (int i = 0; i < 7; i++)
            {
                if (_cards[i] != p.Cards[i]) return false;
                if (_cardsValues[i] != p.CardsValues[i]) return false;
                if (_cardsSuits[i] != p.CardsSuits[i]) return false;
            }
            return true;
        }


        public override string ToString()
        {
            string s = "";

            for (int i = 0; i < 7; i++)
            {
                switch (_cardsValues[i])
                {
                    case 8: s += "T"; break;
                    case 9: s += "J"; break;
                    case 10: s += "Q"; break;
                    case 11: s += "K"; break;
                    case 12: s += "A"; break;
                    default: s += (_cardsValues[i] + 2).ToString(); break;
                }
                switch (_cardsSuits[i])
                {
                    case CLUBS_MASK: s += "c"; break;
                    case HEARTS_MASK: s += "h"; break;
                    case SPADES_MASK: s += "s"; break;
                    case DIAMONDS_MASK: s += "d"; break;
                }
                if (i != 7) s += " ";
            }

            return s;
        }

        //------------- Testes de possibilidades --------------------------------
        public void SetCards(ref int[] cartas, int ini, int fim, int index)
        {

            if (index == 0)
            {
                _handNumber = 0;
                _sequenceNumber = 0;
                _flushNumber = INIT_FLUSH;
            }

            for (int j = ini; j < fim + 1; j++)
            {
                _cards[index] = cartas[j];
                _cardsValues[index] = (cartas[j] - 1) % 13 - 1;
                if (_cardsValues[index] == -1) _cardsValues[index] = 12;
                _cardsSuits[index] = UM << 13 * ((cartas[j] - 1) / 13);

                int shift = (_cardsValues[index]) << 2;
                _handNumber |= (((_handNumber >> shift) & 15) << 1 | 1) << shift;
                _sequenceNumber |= 1 << (_cardsValues[index]);
                _flushNumber += _cardsSuits[index];

                index++;
            }


        }

        private void RankPokerHand7CardsFinal()
        {
            _flushNumber = (_flushNumber & FLUSHMASK) >> 3;

            int seqAux = _sequenceNumber;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;

            _sequenceFound = seqAux > 0;
            _handPower = 1; // Set to sequence to save inner loop set instructions    

            if (!_sequenceFound)
            {
                if (((_sequenceNumber & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                {
                    _sequenceNumber = 8; // clean the ACE in the sequence so that the most significant digit is the max card in straight
                    _sequenceFound = true;
                    //_handPower = 1;
                }
                else _handPower = (int)(_handNumber % 15);
            }
            else
            {

                seqAux |= seqAux >> 1;
                seqAux |= seqAux >> 2;
                seqAux |= seqAux >> 4;
                seqAux |= seqAux >> 8;
                seqAux += 1;
                seqAux >>= 1;

                _sequenceNumber = seqAux;

            }

            if (_flushNumber > 0)
            {
                _handPower = 2;
                if (_sequenceFound)
                {
                    int seqflushInt = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (_cardsSuits[i] == _flushNumber) seqflushInt |= 1 << (_cardsValues[i]);
                    }

                    if (((seqflushInt & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = 15; // clean the ACE in the sequence so that the most significant digit is the max card in strainght
                        _handPower = 8;
                        return;
                    }

                    if (((seqflushInt & MAX_SEQ_PATTERN) ^ MAX_SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = MAX_SEQ_PATTERN_HBIT;
                        _handPower = 9;
                        return;
                    }

                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;

                    // Have to test if a flush sequence was found
                    if (seqflushInt > 0)
                    {
                        seqflushInt |= seqflushInt >> 1;
                        seqflushInt |= seqflushInt >> 2;
                        seqflushInt |= seqflushInt >> 4;
                        seqflushInt |= seqflushInt >> 8;
                        seqflushInt += 1;
                        seqflushInt >>= 1;

                        _sequenceNumber = seqflushInt;
                        _handPower = 8;
                        return;
                    }

                }
            }

            if (_handPower == 7)
                if ((_handNumber & FOUR_DISAMB) > 0)
                {
                    //_handPower = 7; Coincidently the HandPower is already equal to hand strength.
                    return;
                }

            //if the function is not returned yet the index must be translated to hand strength
            _handPower = MAP7CARDS[_handPower];

        }

        public static void SetRandomHandNew(PokerEval p1, PokerEval p2, Random R, int[] c)
        {
            int next;
            ulong n;
            int j = 1;
            ulong sort = 0;

            next = R.Next(1, DECK_CARDS_NUMBER + 1);
            sort |= UM << next;
            c[0] = next;

            while (j < c.Length)
            {
                next = R.Next(1, DECK_CARDS_NUMBER + 1);
                n = UM << next;

                while ((sort & n) > 0)
                {
                    next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    n = UM << next;
                }
                sort |= n;
                c[j] = next;
                j++;
            }

            p1.SetCards(ref c, 0, 4, 0);
            p2.Copy(p1, 5);
            p1.SetCards(ref c, 5, 6, 5);
            p2.SetCards(ref c, 7, 8, 5);

            p1.RankPokerHand7CardsFinal();
            p2.RankPokerHand7CardsFinal();

            /* Only for test purposes.
            PokerEval p3 = new PokerEval();
            PokerEval p4 = new PokerEval();

            p3.RankPokerHandSEVENCards(ref c);
            c[5] = c[7];
            c[6] = c[8];
            p4.RankPokerHandSEVENCards(ref c);

            if (!p1.Compare(p3) | !p2.Compare(p4))
            {
                c[5] = c[7];
            }
            */
        }

        // ----------------------------------------------------------------------


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int HandCompare7Cards(PokerEval p2)
        {
            if (_handPower > p2.HandPower) return 1;
            if (_handPower < p2.HandPower) return -1;

            if (_handPower == (int)PokerHands.Straight | _handPower == (int)PokerHands.StraightFlush | _handPower == (int)PokerHands.RoyalStraightFlush)
            {
                if (_sequenceNumber > p2.SequenceNumber) return 1;
                if (_sequenceNumber < p2.SequenceNumber) return -1;
                return 0;
            }

            HandDisambiguation7Cards(out ulong h11, out ulong h12);
            p2.HandDisambiguation7Cards(out ulong h21, out ulong h22);

            if (h11 > h21) return 1;
            if (h11 < h21) return -1;

            if (h12 > h22) return 1;
            if (h12 < h22) return -1;

            return 0;

        }

        public int RankPokerHandSEVENCards(ref int[] cartas)
        {
            _handNumber = 0;
            _sequenceNumber = 0;
            _flushNumber = INIT_FLUSH;
            int shift;

            for (int j = 0; j < 7; j++)
            {
                _cards[j] = cartas[j];
                _cardsValues[j] = (cartas[j] - 1) % 13 - 1;
                if (_cardsValues[j] == -1) _cardsValues[j] = 12;
                _cardsSuits[j] = UM << 13 * ((cartas[j] - 1) / 13);
                shift = (_cardsValues[j]) << 2;
                _handNumber |= (((_handNumber >> shift) & 15) << 1 | 1) << shift;
                _sequenceNumber |= 1 << (_cardsValues[j]);
                _flushNumber += _cardsSuits[j];
            }

            _flushNumber = (_flushNumber & FLUSHMASK) >> 3;

            int seqAux = _sequenceNumber;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;

            _sequenceFound = seqAux > 0;
            _handPower = 1; // Set to sequence to save inner loop set instructions    

            if (!_sequenceFound)
            {
                if (((_sequenceNumber & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                {
                    _sequenceNumber = 8; // clean the ACE in the sequence so that the most significant digit is the max card in straight
                    _sequenceFound = true;
                    //_handPower = 1;
                }
                else _handPower = (int)(_handNumber % 15);
            }
            else
            {

                seqAux |= seqAux >> 1;
                seqAux |= seqAux >> 2;
                seqAux |= seqAux >> 4;
                seqAux |= seqAux >> 8;
                seqAux += 1;
                seqAux >>= 1;

                _sequenceNumber = seqAux;

            }

            if (_flushNumber > 0)
            {
                _handPower = 2;
                if (_sequenceFound)
                {
                    int seqflushInt = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (_cardsSuits[i] == _flushNumber) seqflushInt |= 1 << (_cardsValues[i]);
                    }


                    if (((seqflushInt & MAX_SEQ_PATTERN) ^ MAX_SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = MAX_SEQ_PATTERN_HBIT;
                        _handPower = 9;
                        return _handPower;
                    }

                    int seqflushIntSave = seqflushInt;

                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;

                    // Have to test if a flush sequence was found
                    if (seqflushInt > 0)
                    {
                        seqflushInt |= seqflushInt >> 1;
                        seqflushInt |= seqflushInt >> 2;
                        seqflushInt |= seqflushInt >> 4;
                        seqflushInt |= seqflushInt >> 8;
                        seqflushInt += 1;
                        seqflushInt >>= 1;

                        _sequenceNumber = seqflushInt;
                        _handPower = 8;
                        return _handPower;
                    }

                    if (((seqflushIntSave & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = 15; // clean the ACE in the sequence so that the most significant digit is the max card in strainght
                        _handPower = 8;
                        return _handPower;
                    }

                }
            }

            if (_handPower == 7)
                if ((_handNumber & FOUR_DISAMB) > 0)
                {
                    //_handPower = 7; Coincidently the HandPower is already equal to hand strength.
                    return _handPower;
                }

            //if the function is not returned yet the index must be translated to hand strength
            _handPower = MAP7CARDS[_handPower];

            return _handPower;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void HandDisambiguation7Cards(out ulong hand1, out ulong hand2)
        {
            int i;
            hand1 = (_handNumber & DISAMB_ARRAY[_handPower]) >> DISAMB_SHIFT[_handPower];
            ulong clean = (hand1 % 15) - DISAMB_CARDS_GAME[_handPower];

            //clean up other suit cards
            if (FlushNumber > 0)
            {
                hand2 = 0;

                if (clean == 0) return;

                hand1 = 0;
                for (i = 0; i < 7; i++)
                    if (_cardsSuits[i] == _flushNumber)
                        hand1 |= UM << (_cardsValues[i] << 2);

                clean = (hand1 % 15) - DISAMB_CARDS_GAME[_handPower];

                if (clean == 0) return;
            }
            // clean lower games 
            i = 0;
            while (clean > 0)
            {
                clean -= (hand1 & 1);
                i += 4;
                hand1 >>= 4;
            }
            hand1 <<= i;

            if (DISAMB_CARDS_LEFT[_handPower] == 0)
            {
                hand2 = 0;
                return;
            }

            //Tratamento especial para full house
            if (_handPower != (int)PokerHands.FullHouse)
                hand2 = (hand1 ^ MAX_NUM) & _handNumber & HIGH_CARD_DISAMB;
            else
                hand2 = (((hand1 << 1) ^ MAX_NUM) & (_handNumber & PAIR_DISAMB)) >> 1;

            //clean up unused cards
            i = 0;
            clean = (hand2 % 15) - DISAMB_CARDS_LEFT[_handPower];
            while (clean > 0)
            {
                clean -= (hand2 & 1);
                i += 4;
                hand2 >>= 4;
            }
            hand2 <<= i;


        }

        public static void SetRandomHand(PokerEval p1, PokerEval p2, Random R, int[] c)
        {
            int next;
            ulong n;
            int j = 1;
            ulong sort;

            next = R.Next(1, DECK_CARDS_NUMBER + 1);
            sort = UM << next;
            c[0] = next;

            while (j < c.Length)
            {
                next = R.Next(1, DECK_CARDS_NUMBER + 1);
                n = UM << next;

                while ((sort & n) > 0)
                {
                    next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    n = UM << next;
                }
                sort |= n;
                c[j] = next;
                j++;
            }

            p1.RankPokerHandSEVENCards(ref c);
            c[5] = c[7];
            c[6] = c[8];
            p2.RankPokerHandSEVENCards(ref c);

        }

        // the first 2 cards are already set
        public static void RandomHand(int[] c, Random R)
        {
            int next;
            ulong n;
            int j = 2;
            ulong sort;

            sort = UM << c[0] | UM << c[1];

            while (j < c.Length)
            {
                next = R.Next(1, DECK_CARDS_NUMBER + 1);
                n = UM << next;

                while ((sort & n) > 0)
                {
                    next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    n = UM << next;
                }
                sort |= n;
                c[j] = next;
                j++;
            }

        }

        public static void RandomHand(int[] c, int[,] Range, int RangeSize, Random R)
        {
            int next;
            ulong n;
            int j = 2;
            ulong sort;

            sort = UM << c[0] | UM << c[1];

            //Vilain pocket pair
            next = R.Next(0, RangeSize);
            n = UM << Range[next, 0] | UM << Range[next, 1];
            while ((sort & n) > 0)
            {
                next = R.Next(0, RangeSize);
                n = UM << Range[next, 0] | UM << Range[next, 1];
            }
            c[7] = Range[next, 0];
            c[8] = Range[next, 1];
            sort |= n;

            //board
            while (j < 7)
            {
                next = R.Next(1, DECK_CARDS_NUMBER + 1);
                n = UM << next;

                while ((sort & n) > 0)
                {
                    next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    n = UM << next;
                }
                sort |= n;
                c[j] = next;
                j++;
            }

        }

        //----------- Five CARDS - DEPRECATED ?  ----------------//

        /*  public ulong HandDisambiguationHand(ulong hand1, ulong hand2, int index)
          {
              ulong p1, p2, r1, r2;

              p1 = hand1 & DISAMB_ARRAY[index];
              p2 = hand2 & DISAMB_ARRAY[index];

              if (p1 < p2) return hand2;
              if (p1 > p2) return hand1;

              p1 &= p1 >> DISAMB_SHIFT[index, 0];
              p2 &= p2 >> DISAMB_SHIFT[index, 0];

              p1 &= p1 >> DISAMB_SHIFT[index, 1];
              p2 &= p2 >> DISAMB_SHIFT[index, 1];

              p1 &= p1 >> DISAMB_SHIFT[index, 2];
              p2 &= p2 >> DISAMB_SHIFT[index, 2];

              r1 = (p1 ^ MAX_NUM) & hand1;
              r2 = (p2 ^ MAX_NUM) & hand2;

              if (r1 < r2) return hand2;
              if (r2 > r1) return hand1;

              return hand1;
          }
        */

        public int HandDisambiguation5Cards(ulong hand1, ulong hand2, int index)
        {
            ulong p1, p2, r1, r2;

            p1 = hand1 & DISAMB_ARRAY[index];
            p2 = hand2 & DISAMB_ARRAY[index];

            if (p1 < p2) return -1;
            if (p1 > p2) return 1;

            p1 >>= DISAMB_SHIFT[index];
            p2 >>= DISAMB_SHIFT[index];

            if (index != 8)
            {
                r1 = (p1 ^ MAX_NUM) & hand1 & HIGH_CARD_DISAMB;
                r2 = (p2 ^ MAX_NUM) & hand2 & HIGH_CARD_DISAMB;
            }
            else
            {
                r1 = (p1 ^ MAX_NUM) & hand1 & PAIR_DISAMB;
                r2 = (p2 ^ MAX_NUM) & hand2 & PAIR_DISAMB;
            }

            if (r1 < r2) return -1;
            if (r2 > r1) return 1;

            return 0;
        }

        // novo
        public void RankPokerFIVECardsHand(int[] cartas, out ulong hand, out ulong hand1, out int index)
        {
            //(cardNumber - 1) % 13
            // (cardNumber - valor) / 13

            int[] num = new int[5];
            int[] suit = new int[5];
            int[] i = new int[5];
            int[] cartasnum = new int[7];
            int[] cartassuit = new int[7];


            ulong besthand = 0;
            ulong besthand1 = 0;
            int bestindex = -1;

            for (int j = 0; j < 7; j++)
            {
                cartasnum[j] = (cartas[j] - 1) % 13 - 1;
                if (cartasnum[j] == -1) cartasnum[j] = 12;
                cartassuit[j] = 1 << ((cartas[j] - 1) / 13);
            }

            for (i[0] = 0; i[0] < 3; i[0]++)
            {
                num[0] = cartasnum[i[0]];
                suit[0] = cartassuit[i[0]];
                for (i[1] = i[0] + 1; i[1] < 4; i[1]++)
                {
                    num[1] = cartasnum[i[1]];
                    suit[1] = cartassuit[i[1]];
                    for (i[2] = i[1] + 1; i[2] < 5; i[2]++)
                    {
                        num[2] = cartasnum[i[2]];
                        suit[2] = cartassuit[i[2]];
                        for (i[3] = i[2] + 1; i[3] < 6; i[3]++)
                        {
                            num[3] = cartasnum[i[3]];
                            suit[3] = cartassuit[i[3]];
                            for (i[4] = i[3] + 1; i[4] < 7; i[4]++)
                            {
                                num[4] = cartasnum[i[4]];
                                suit[4] = cartassuit[i[4]];
                                RankPokerFIVECardsHand(ref num, ref suit, out hand, out hand1, out index);
                                if (index > bestindex)
                                {
                                    bestindex = index;
                                    besthand = hand;
                                    besthand1 = hand1;
                                }
                                else if (index == bestindex)
                                {
                                    if (hand > besthand)
                                    {
                                        besthand = hand;
                                        besthand1 = hand1;
                                    }
                                    else if (hand1 > besthand)
                                    {
                                        besthand1 = hand1;
                                    }

                                }
                            }
                        }
                    }
                }
            }
            hand = besthand;
            hand1 = besthand1;
            index = bestindex;
        }
        // novo

        public void RankPokerHandFIVECards(ref int[] num, ref int[] suit, out ulong hand, out int seq, out int flush, out int index)
        {

            hand = 0;
            seq = 0;
            flush = 15;

            int shift;

            for (int i = 0; i < 5; i++)
            {
                shift = 4 * (num[i]);
                hand |= (((hand >> shift) & 15) << 1 | 1) << shift;
                seq |= 1 << (num[i]);
                flush &= suit[i];
            }

            //((s / (s & -s) == 31) || (s == 0x403c) ? 3 : 1);
            //{ 7, 8, 4, 5, 0, 1, 2, 9, 3 }
            //hands=["4 of a Kind", "Straight Flush", "Straight", "Flush", "High Card", "1 Pair", "2 Pair", "Royal Flush", "3 of a Kind", "Full House" ];

            index = (int)(hand % 15);
            index -= (seq / (seq & -seq) == 31) | (seq == SEQ_PATTERN) ? 3 : 1;
            index -= (flush > 0 ? 1 : 0) * (seq == MAX_SEQ_PATTERN ? -5 : 1);
            // Avaliar tirar a desambiguação do ROYAL FLUSH daqui e tratar como um sequencia com carta alta A


            // zerar o bit 48 para poder comparar sequencias A2345  com as demais
            if (seq == SEQ_PATTERN) hand ^= SEQ_DISAMB;
            index = MAP[index];

        }

        //Tentativa de fazer 7 cartas como permutação de 5 cartas.... ficou muito lento.
        // Não funciona mais pois mudei o procedimento de desambiguação para poder comparar mãos diferentes.
        /* public void RankPokerHand(int[] cartas, out ulong hand, out int seq, out int flush, out int index)
         {
             //(cardNumber - 1) % 13
             // (cardNumber - valor) / 13

             int[] num = new int[5];
             int[] suit = new int[5];
             int[] i = new int[5];
             int[] cartasnum = new int[7];
             int[] cartassuit = new int[7];

             seq = 0;
             flush = 0;

             ulong besthand = 0;
             int bestindex = -1;

             for (int j = 0; j < 7; j++)
             {
                 cartasnum[j] = (cartas[j] - 1) % 13 - 1;
                 if (cartasnum[j] == -1) cartasnum[j] = 12;
                 cartassuit[j] = 1 << ((cartas[j] - 1) / 13);
             }

             for (i[0] = 0; i[0] < 3; i[0]++)
             {
                 num[0] = cartasnum[i[0]];
                 suit[0] = cartassuit[i[0]];
                 for (i[1] = i[0] + 1; i[1] < 4; i[1]++)
                 {
                     num[1] = cartasnum[i[1]];
                     suit[1] = cartassuit[i[1]];
                     for (i[2] = i[1] + 1; i[2] < 5; i[2]++)
                     {
                         num[2] = cartasnum[i[2]];
                         suit[2] = cartassuit[i[2]];
                         for (i[3] = i[2] + 1; i[3] < 6; i[3]++)
                         {
                             num[3] = cartasnum[i[3]];
                             suit[3] = cartassuit[i[3]];
                             for (i[4] = i[3] + 1; i[4] < 7; i[4]++)
                             {
                                 num[4] = cartasnum[i[4]];
                                 suit[4] = cartassuit[i[4]];
                                 RankPokerHand(ref num, ref suit, out hand, out seq, out flush, out index);
                                 if (index > bestindex)
                                 {
                                     bestindex = index;
                                     besthand = hand;
                                 }
                                 else if (index == bestindex)
                                 {
                                     if (DISAMB_NEED[index]) 
                                     {
                                         //besthand = HandDisambiguation(hand, out hand, out besthand, index);
                                     }
                                     else if (hand > besthand)
                                     {
                                         besthand = hand;
                                     }

                                 }
                             }
                         }
                     }
                 }
             }
             hand = besthand;
             index = bestindex;
         }

         */


        //----------- DEPRECATED ----------------//

        public int RankPokerHandSEVENCardsOLD(ref int[] cartas)
        {

            for (int j = 0; j < 7; j++)
            {
                _cards[j] = cartas[j];
                _cardsValues[j] = (cartas[j] - 1) % 13 - 1;
                if (_cardsValues[j] == -1) _cardsValues[j] = 12;
                _cardsSuits[j] = UM << 13 * ((cartas[j] - 1) / 13);
            }
            RankPokerHand7Cards();
            return _handPower;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)] 
        private void RankPokerHand7Cards()
        {
            int shift;

            _handNumber = 0;
            _sequenceNumber = 0;
            _flushNumber = INIT_FLUSH;

            for (int i = 0; i < 7; i++)
            {
                shift = (_cardsValues[i]) << 2;
                _handNumber |= (((_handNumber >> shift) & 15) << 1 | 1) << shift;
                _sequenceNumber |= 1 << (_cardsValues[i]);
                _flushNumber += _cardsSuits[i];
            }

            _flushNumber = (_flushNumber & FLUSHMASK) >> 3;

            int seqAux = _sequenceNumber;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;
            seqAux &= seqAux << 1;

            _sequenceFound = seqAux > 0;
            _handPower = 1; // Set to sequence to save inner loop set instructions    

            if (!_sequenceFound)
            {
                if (((_sequenceNumber & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                {
                    _sequenceNumber = 8; // clean the ACE in the sequence so that the most significant digit is the max card in straight
                    _sequenceFound = true;
                    //_handPower = 1;
                }
                else _handPower = (int)(_handNumber % 15);
            }
            else
            {

                seqAux |= seqAux >> 1;
                seqAux |= seqAux >> 2;
                seqAux |= seqAux >> 4;
                seqAux |= seqAux >> 8;
                seqAux += 1;
                seqAux >>= 1;

                _sequenceNumber = seqAux;

            }

            if (_flushNumber > 0)
            {
                _handPower = 2;
                if (_sequenceFound)
                {
                    int seqflushInt = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (_cardsSuits[i] == _flushNumber) seqflushInt |= 1 << (_cardsValues[i]);
                    }

                    if (((seqflushInt & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = 15; // clean the ACE in the sequence so that the most significant digit is the max card in strainght
                        _handPower = 8;
                        return;
                    }

                    if (((seqflushInt & MAX_SEQ_PATTERN) ^ MAX_SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = MAX_SEQ_PATTERN_HBIT;
                        _handPower = 9;
                        return;
                    }

                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;
                    seqflushInt &= seqflushInt << 1;

                    // Have to test if a flush sequence was found
                    if (seqflushInt > 0)
                    {
                        seqflushInt |= seqflushInt >> 1;
                        seqflushInt |= seqflushInt >> 2;
                        seqflushInt |= seqflushInt >> 4;
                        seqflushInt |= seqflushInt >> 8;
                        seqflushInt += 1;
                        seqflushInt >>= 1;

                        _sequenceNumber = seqflushInt;
                        _handPower = 8;
                        return;
                    }

                }
            }

            if (_handPower == 7)
                if ((_handNumber & FOUR_DISAMB) > 0)
                {
                    //_handPower = 7; Coincidently the HandPower is already equal to hand strength.
                    return;
                }

            //if the function is not returned yet the index must be translated to hand strength
            _handPower = MAP7CARDS[_handPower];

        }


        private void RankPokerHand7CardsOLD()
        {
            int shift;

            _handNumber = 0;
            _sequenceNumber = 0;
            _flushNumber = INIT_FLUSH;
            _handPower = 1; // Set to sequence to save inner loop set instructions         

            for (int i = 0; i < 7; i++)
            {
                shift = (_cardsValues[i]) << 2;
                _handNumber |= (((_handNumber >> shift) & 15) << 1 | 1) << shift;
                _sequenceNumber |= 1 << (_cardsValues[i]);
                _flushNumber += _cardsSuits[i];
            }

            _flushNumber = (_flushNumber & FLUSHMASK) >> 3;
            _sequenceFound = false;

            int seqAux = MAX_SEQ_PATTERN;
            for (int i = 0; i < 9; i++)
            {
                if (((_sequenceNumber & seqAux) ^ seqAux) == 0)
                {
                    _sequenceNumber = seqAux;
                    _sequenceFound = true;
                    //_handPower = 1;
                    break;
                }
                seqAux >>= 1;
            }

            if (!_sequenceFound)
            {
                if (((_sequenceNumber & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                {
                    _sequenceNumber = 15; // clean the ACE in the sequence so that the most significant digit is the max card in strainght
                    _sequenceFound = true;
                    //_handPower = 1;
                }
                else _handPower = (int)(_handNumber % 15);
            }

            if (_flushNumber > 0)
            {
                _handPower = 2;
                if (_sequenceFound)
                {
                    int seqflushInt = 0;
                    for (int i = 0; i < 7; i++)
                    {
                        if (_cardsSuits[i] == _flushNumber) seqflushInt |= 1 << (_cardsValues[i]);
                    }

                    if (((seqflushInt & MAX_SEQ_PATTERN) ^ MAX_SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = MAX_SEQ_PATTERN;
                        _handPower = 9;
                        return;
                    }
                    else
                    {
                        seqAux = HALF_MAX_SEQ_PATTERN;
                        for (int i = 1; i < 9; i++)
                        {
                            if (((seqflushInt & seqAux) ^ seqAux) == 0)
                            {
                                _sequenceNumber = seqAux;
                                _handPower = 8;
                                return;
                            }
                            seqAux >>= 1;
                        }
                    }

                    if (((seqflushInt & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                    {
                        _sequenceNumber = 15; // clean the ACE in the sequence so that the most significant digit is the max card in strainght
                        _handPower = 8;
                        return;
                    }


                }
            }

            if (_handPower == 7)
                if ((_handNumber & FOUR_DISAMB) > 0)
                {
                    //_handPower = 7; Coincidently the HandPower is already equal to hand strength.
                    return;
                }

            //if the function is not returned yet the index must be translated to hand strength
            _handPower = MAP7CARDS[_handPower];

        }

        //Must not be called if index are equal
        public int HandCompare7Cards(ulong h1, int i1, ulong h2, int i2)
        {

            HandDisambiguation7Cards(h1, out ulong h11, out ulong h12, i1);
            HandDisambiguation7Cards(h2, out ulong h21, out ulong h22, i2);

            if (h11 > h21) return 1;
            if (h11 < h21) return -1;

            if (h12 > h22) return 1;
            if (h12 < h22) return -1;

            return 0;
        }

        public void HandDisambiguation7Cards(ulong hand, out ulong hand1, out ulong hand2, int index)
        {

            hand1 = hand;
            hand2 = hand;

            if (!DISAMB_NEED[index]) return;

            hand1 = hand & DISAMB_ARRAY[index];
            hand1 >>= DISAMB_SHIFT[index];

            // clean lower games 
            int i = 0;
            while (hand1 % 15 > DISAMB_CARDS_GAME[index])
            {
                i += 4;
                hand1 >>= 4;
            }
            hand1 <<= i;

            //Tratamento especial para full house
            if (index != 8)
                hand2 = (hand1 ^ MAX_NUM) & hand & HIGH_CARD_DISAMB;
            else
                hand2 = (hand1 ^ MAX_NUM) & hand & PAIR_DISAMB >> 1;

            //clean up unused cards
            i = 0;
            while (hand2 % 15 > DISAMB_CARDS_LEFT[index])
            {
                i += 4;
                hand2 >>= 4;
            }
            hand2 <<= i;

        }

        // Não funciona pois mudei a desambiguação de mão.... tem que rever
        public void RankPokerFIVECardsHand(ref int[] num, ref int[] suit, out ulong hand, out ulong hand1, out int index)
        {

            hand = 0;
            int seq = 0;
            int flush = 15;

            int shift;

            for (int i = 0; i < 5; i++)
            {
                shift = 4 * (num[i]);
                hand |= (((hand >> shift) & 15) << 1 | 1) << shift; ;
                seq |= 1 << (num[i]);
                flush &= suit[i];
            }

            //((s / (s & -s) == 31) || (s == 0x403c) ? 3 : 1);
            //{ 7, 8, 4, 5, 0, 1, 2, 9, 3 }
            //hands=["4 of a Kind", "Straight Flush", "Straight", "Flush", "High Card", "1 Pair", "2 Pair", "Royal Flush", "3 of a Kind", "Full House" ];

            index = (int)(hand % 15);
            index -= (seq / (seq & -seq) == 31) | (seq == SEQ_PATTERN) ? 3 : 1;
            index -= (flush > 0 ? 1 : 0) * (seq == MAX_SEQ_PATTERN ? -5 : 1);
            // Avaliar tirar a desambiguação do ROYAL FLUSH daqui e tratar como um sequencia com carta alta A


            // zerar o bit 48 para poder comparar sequencias A2345  com as demais
            if (seq == SEQ_PATTERN) hand ^= SEQ_DISAMB;
            index = MAP[index];


            hand1 = 0;
            //HandDisambiguation5Cards(hand, out hand, out hand1, index);

        }

        public void RankPokerHand7CardsOld(ref int[] num, ref ulong[] suit, out ulong hand, out int seq, out ulong flush, out int index)
        {

            hand = 0;
            seq = 0;
            flush = INIT_FLUSH;
            index = 1; // Set to sequence to save inner loop set instructions

            ulong seqflush = 0;
            ulong seqflushAux;
            int seqflushInt;
            int shift;

            bool AchouSequencia = false;

            for (int i = 0; i < 7; i++)
            {
                shift = (num[i]) << 2;
                hand |= (((hand >> shift) & 15) << 1 | 1) << shift;
                seq |= 1 << (num[i]);
                flush += suit[i];
                seqflush |= suit[i] << num[i];
            }

            flush &= FLUSHMASK;
            flush >>= 3;   //ajusta para o suit do flush

            int seqAux = MAX_SEQ_PATTERN;
            for (int i = 0; i < 9; i++)
            {
                if (((seq & seqAux) ^ seqAux) == 0)
                {
                    seq = seqAux;
                    AchouSequencia = true;
                    //index = 1;
                    break;
                }
                seqAux >>= 1;
            }

            if (!AchouSequencia)
            {
                if (((seq & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                {
                    seq = SEQ_PATTERN;
                    AchouSequencia = true;
                    //index = 1;
                }
                else index = (int)(hand % 15);
            }

            if (flush > 0)
            {
                index = 2;
                if (AchouSequencia)
                {
                    seqflushAux = seqflush;
                    if (flush == HEARTS_MASK) seqflushAux = seqflush >> 13;
                    else if (flush == SPADES_MASK) seqflushAux = seqflush >> 26;
                    else if (flush == DIAMONDS_MASK) seqflushAux = seqflush >> 39;
                    //else seqflushAux = seqflush;

                    seqflushInt = (int)(seqflushAux &= 8191);

                    if (((seqflushInt & MAX_SEQ_PATTERN) ^ MAX_SEQ_PATTERN) == 0)
                    {
                        seq = seqflushInt;
                        index = 9;
                        return;
                    }
                    else
                    {
                        seqAux = HALF_MAX_SEQ_PATTERN;
                        for (int i = 1; i < 9; i++)
                        {
                            if (((seqflushInt & seqAux) ^ seqAux) == 0)
                            {
                                seq = seqflushInt;
                                index = 8;
                                return;
                            }
                            seqAux >>= 1;
                        }
                    }

                    if (((seqflushInt & SEQ_PATTERN) ^ SEQ_PATTERN) == 0)
                    {
                        seq = SEQ_PATTERN;
                        index = 8;
                        return;
                    }


                }
            }

            // zerar o bit 48 para poder comparar sequencias A2345  com as demais 
            //if (seq == SEQ_PATTERN) hand ^= SEQ_DISAMB;
            if (index == 7)
                if ((hand & FOUR_DISAMB) > 1)
                {
                    index = 7;
                    return;
                }
            index = MAP7CARDS[index];

        }

        public static void SetRandomHandOLD(PokerEval p1, PokerEval p2, Random R, int[] c)
        {
            int next = 0;

            bool ok;

            for (int i = 0; i < c.Length; i++)
            {
                ok = false;
                while (!ok)
                {
                    next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    ok = true;
                    for (int j = 0; j < i; j++)
                    {
                        if (c[j] == next)
                        {
                            ok = false;
                            break;
                        }

                    }
                }
                c[i] = next;
            }

            p1.RankPokerHandSEVENCards(ref c);
            c[5] = c[7];
            c[6] = c[8];
            p2.RankPokerHandSEVENCards(ref c);

        }

        public static void SetRandomHandOLD2(PokerEval p1, PokerEval p2, Random R, int[] c)
        {
            int next;
            int j;

            for (int i = 0; i < c.Length; i++)
            {
                j = 0;
                next = R.Next(1, DECK_CARDS_NUMBER + 1);
                while (j < i)
                {
                    if (c[j] != next)
                        j++;
                    else
                    {
                        j = 0;
                        next = R.Next(1, DECK_CARDS_NUMBER + 1);
                    }
                }
                c[i] = next;
            }

            p1.RankPokerHandSEVENCards(ref c);
            c[5] = c[7];
            c[6] = c[8];
            p2.RankPokerHandSEVENCards(ref c);

        }

    }
}
