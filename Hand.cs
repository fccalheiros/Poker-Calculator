using System;


// This is part of the first implementation that does not have good performance
// This structure represents a deck card
public struct CARD
{
    private byte cardNumber;
    public byte value;
    public byte suit;

    public byte CardNumber
    {
        get { return cardNumber; }
        set { cardNumber = value; }
    }

    public char ReturnSuitChar()
    {
        if (cardNumber == 0) return " "[0];
        return ReturnSuitChar(suit);
    }

    public void SetValue(string s)
    {
        value = ReturnValueByte(s[0]);
        suit = ReturnSuitByte(s[1]);
        cardNumber = ReturnSequenceNumber(value, suit);
    }

    public static int ToInt(string s)
    {
        s = s.ToUpper();
        byte v = ReturnValueByte(s[0]);
        byte n = ReturnSuitByte(s[1]);
        return ReturnSequenceNumber(v, n);
    }

    public char ReturnValueChar()
    {
        if (cardNumber == 0) return ' ';
        return ReturnValueChar(value);
    }

    public static char ReturnValueChar(int n)
    {
        if (n == 0) return 'A';
        if (n <= 8) return (n + 1).ToString()[0];
        if (n == 9) return 'T';
        if (n == 10) return 'J';
        if (n == 11) return 'Q';
        if (n == 12) return 'K';
        return new char();
    }

    public static byte ReturnValueByte(char s)
    {
        byte v;
        switch (s)
        {
            case 'A': v = 0; break;
            case 'T': v = 9; break;
            case 'J': v = 10; break;
            case 'Q': v = 11; break;
            case 'K': v = 12; break;
            default: v = Convert.ToByte(Convert.ToByte(s.ToString()) - 1); break;
        }
        return v;
    }

    public static char ReturnSuitChar(int n)
    {
        //'hearts'(copas), 'diamonds'(ouros), 'clubs'(paus) e 'spades'

        if (n == 0) return 'c';
        if (n == 1) return 'h';
        if (n == 2) return 's';
        if (n == 3) return 'd';
        return new char();
    }

    public static byte ReturnSuitByte(char s)
    {
        //'hearts'(copas), 'diamonds'(ouros), 'clubs'(paus) e 'spades'
        byte v = 0;
        switch (s)
        {
            case 'c': v = 0; break;
            case 'h': v = 1; break;
            case 's': v = 2; break;
            case 'd': v = 3; break;
            case 'C': v = 0; break;
            case 'H': v = 1; break;
            case 'S': v = 2; break;
            case 'D': v = 3; break;
        }
        return v;
    }

    public static byte ReturnSequenceNumber(byte v, byte n)
    {
        return Convert.ToByte(v + 1 + n * 13);
    }

    public bool IsEmpty()
    {
        return (cardNumber == 0);
    }

    public void SET(byte number)
    {
        if (number == 0)
        {
            cardNumber = 0;
            value = 50;
            suit = 50;
            return;
        }

        cardNumber = number;
        value = Convert.ToByte((cardNumber - 1) % 13);
        suit = Convert.ToByte((cardNumber - value) / 13);
    }

    public void SET(int number) { SET(Convert.ToByte(number)); }
    public void SET(CARD c)
    {
        cardNumber = c.CardNumber;
        value = c.value;
        suit = c.suit;
    }

    public CARD(byte number)
    {
        cardNumber = 0;
        value = 50;
        suit = 50;

        SET(number);
    }

    public override string ToString() => $"{ReturnValueChar()}{ReturnSuitChar()}";

}

public enum PokerHands : int
{
    RoyalStraightFlush = 9,
    StraightFlush = 8,
    Four = 7,
    FullHouse = 6,
    Flush = 5,
    Straight = 4,
    Trips = 3,
    TwoPairs = 2,
    Pair = 1,
    HighCard = 0
}

public class HoldemPokerHand
{

    //Cards are numbers from 1 .. 52 --- A até K e Clubs - Hearts - Spades - Diamonds
    // suit 0 - Clubs - 1 Hearts - 2 Spades - 3 Diamonds

    const int TotalCards = 52;
    const int NumCards = 14;
    const int Numsuits = 4;
    const int BoardSize = 5;
    const int pocketCardsSize = 2;

    private CARD[] pocketCards;
    private CARD[] board;

    private byte[] hand;
    private byte[] suits;

    public CARD[] GetBoard() { return board; }
    public CARD[] GetPocketCards() { return pocketCards; }

    public int[] GetCardsNumbers()
    {
        int[] cards = new int[7];
        for (int i = 0; i < 5; i++) cards[i] = board[i].CardNumber;
        cards[5] = pocketCards[0].CardNumber;
        cards[6] = pocketCards[1].CardNumber;
        return cards;
    }

    private CARD GetHighCard()
    {
        CARD c;

        c = new CARD(0);

        for (byte i = NumCards; i >= 1; i--)
        {
            if (hand[i] > 0)
            {
                c.SET(i);
                return c;
            }

        }

        return c;
    }

    private CARD GetHighCard(int suit)
    {
        CARD c;

        int res = -1;
        c = new CARD(0);

        for (int i = 0; i < pocketCardsSize; i++)
        {
            if (pocketCards[i].suit == suit)
            {
                if (pocketCards[i].value == 0)
                {
                    c.SET(pocketCards[i].CardNumber);
                    return c;
                }
                if (pocketCards[i].value > res)
                {
                    res = pocketCards[i].value;
                    c.SET(pocketCards[i].CardNumber);
                }

            }
        }

        for (int i = 0; i < board.Length; i++)
        {
            if (board[i].suit == suit)
            {
                if (board[i].value == 0)
                {
                    c.SET(board[i].CardNumber);
                    return c;
                }
                if (board[i].value > res)
                {
                    res = board[i].value;
                    c.SET(board[i].CardNumber);
                }
            }
        }

        return c;
    }

    public bool FindCard(ref CARD c)
    {
        for (int i = 0; i < board.Length; i++)
            if (c.CardNumber == board[i].CardNumber) return true;

        if (c.CardNumber == pocketCards[0].CardNumber) return true;
        if (c.CardNumber == pocketCards[1].CardNumber) return true;

        return false;

    }

    public bool GetHighCards(ref CARD[] c, int suit)
    {
        int j = 0;
        string s = ToString();

        for (int i = NumCards - 1; i >= 1; i--)
        {
            if (hand[i] != 0)
            {
                c[j].SET(Convert.ToByte(i + 1));
                c[j].suit = Convert.ToByte(suit);
                if (s.IndexOf(c[j].ToString()) >= 0)
                {
                    j++;
                    if (j == BoardSize) return true;
                }
            }
        }
        return true;
    }

    public int Compare(HoldemPokerHand b)
    {
        PokerHands ha, hb;

        CARD[] c = new CARD[BoardSize];
        CARD[] c1 = new CARD[BoardSize];

        for (int i = 0; i < BoardSize; i++)
        {
            c[i] = new CARD(0);
            c1[i] = new CARD(0);
        }

        // c will bring the 5 cards used for the player´s hand which will enable the tiebreker
        ha = ReturnPokerHand(ref c);
        hb = b.ReturnPokerHand(ref c1);

        if (ha < hb) return -1;
        if (ha > hb) return 1;

        // Ace must be considered the higher card.
        for (int i = 0; i < BoardSize; i++)
        {
            if (c[i].value == 0 & !c[i].IsEmpty()) c[i].value = 13;
            if (c1[i].value == 0 & !c[i].IsEmpty()) c1[i].value = 13;
        }

        if (ha == PokerHands.Pair)
        {
            if (c[0].value < c1[0].value) return -1;
            if (c[0].value > c1[0].value) return 1;
            for (int i = 2; i < BoardSize; i++)
            {
                if (c[i].value < c1[i].value) return -1;
                if (c[i].value > c1[i].value) return 1;
            }
            return 0;
        }

        if (ha == PokerHands.Trips | ha == PokerHands.TwoPairs)
        {
            for (int i = 0; i < 3; i++)
            {
                if (c[i].value < c1[i].value) return -1;
                if (c[i].value > c1[i].value) return 1;
            }
            return 0;
        }

        if (ha == PokerHands.Flush | ha == PokerHands.HighCard)
        {
            for (int i = 0; i < BoardSize; i++)
            {
                if (c[i].value < c1[i].value) return -1;
                if (c[i].value > c1[i].value) return 1;
            }
            return 0;
        }

        if (ha == PokerHands.FullHouse | ha == PokerHands.Four)
        {
            for (int i = 0; i < 2; i++)
            {
                if (c[i].value < c1[i].value) return -1;
                if (c[i].value > c1[i].value) return 1;
            }
            return 0;
        }

        if (ha == PokerHands.StraightFlush | ha == PokerHands.Straight)
        {
            if (c[0].value < c1[0].value) return -1;
            if (c[0].value > c1[0].value) return 1;
            return 0;
        }

        return 0;
    }

    public static string GetHandName(PokerHands h)
    {
        switch (h)
        {
            case PokerHands.RoyalStraightFlush: return "Royal Straight Flush";
            case PokerHands.StraightFlush: return "Straight Flush";
            case PokerHands.Four: return "Four";
            case PokerHands.FullHouse: return "FullHouse";
            case PokerHands.Flush: return "Flush";
            case PokerHands.Straight: return "Straight";
            case PokerHands.Trips: return "Trips";
            case PokerHands.TwoPairs: return "Two Pairs";
            case PokerHands.Pair: return "Pair";
            case PokerHands.HighCard: return "High Card";
        }
        return "";

    }

    public bool CheckStraightFlush(ref CARD[] c)
    {

        int n = -1;
        int cont = 0;

        for (int i = 0; i < Numsuits; i++)
            if (suits[i] >= BoardSize)
            {
                n = i;
                break;
            }

        if (n < 0) return false;

        for (int i = 0; i < BoardSize; i++) c[i].SET(0);

        string s = ToString();

        for (int i = NumCards - 1; i >= 0; i--)
        {
            if (hand[i] != 0)
            {
                c[cont].SET(Convert.ToByte(i + 1));
                c[cont].suit = Convert.ToByte(n);
                if (s.IndexOf(c[cont].ToString()) < 0)
                    cont = 0;
                else
                    cont++;
                if (cont == BoardSize) break;
            }
            else cont = 0;
        }

        if (cont != BoardSize) return false;

        return true;
    }

    public bool CheckFour(ref CARD[] c)
    {

        c[0].SET(0);
        for (int i = NumCards - 1; i >= 1; i--)
        {
            if (hand[i] == 4)
            {
                c[0].SET(Convert.ToByte(i + 1));
                break;
            }
        }

        if (!c[0].IsEmpty())
        {
            for (int i = NumCards - 1; i >= 1; i--)
            {
                if (hand[i] != 4 & hand[i] != 0)
                {
                    c[1].SET(Convert.ToByte(i + 1));
                    //avaliar se precisa limpar isso
                    //for (int j = 2; j < BoardSize; j++) c[j].SET(0);
                    return true;
                }
            }

        }

        return false;
    }

    public bool CheckFullHouse(ref CARD[] c)
    {
        int carta = -1;

        for (int i = NumCards - 1; i > 0; i--)
        {
            if (hand[i] == 3)
            {
                carta = i;
                c[0].SET(Convert.ToByte(i + 1));
                break;
            }
        }

        if (carta < 0) return false;

        for (int i = NumCards - 1; i > 0; i--)
        {
            if (i != carta & (hand[i] == 3 | hand[i] == 2))
            {
                c[1].SET(Convert.ToByte(i + 1));
                //avaliar se precisa limpar isso
                //for (int j = 2; j < BoardSize; j++) c[j].SET(0);
                return true;
            }
        }

        return false;
    }

    public bool CheckFlush(ref CARD[] c)
    {

        for (int i = 0; i < Numsuits; i++)
            if (suits[i] >= BoardSize)
            {
                GetHighCards(ref c, i);
                return true;
            }
        return false;
    }

    public bool CheckStraight(ref CARD[] c)
    {
        int cont = 0;

        for (int i = NumCards - 1; i >= 0; i--)
        {
            if (hand[i] != 0)
            {
                c[cont].SET(Convert.ToByte(i + 1));
                cont++;
                if (cont == BoardSize) return true;
            }
            else cont = 0;
        }
        return false;
    }

    public bool CheckTrips(ref CARD[] c)
    {
        c[0].SET(0);
        for (int i = NumCards - 1; i > 0; i--)
        {
            if (hand[i] == 3)
            {
                for (int j = 3; j < BoardSize; j++) c[j].SET(0);
                c[0].SET(Convert.ToByte(i + 1));
                break;
            }
        }

        if (!c[0].IsEmpty())
        {
            int j = 1;
            for (int i = NumCards - 1; i > 0; i--)
            {
                if (hand[i] != 3 & hand[i] != 0)
                {
                    c[j].SET(Convert.ToByte(i + 1));
                    j++;
                    if (j == 3) return true;
                }
            }
        }

        return false;
    }

    public bool CheckPair(ref CARD[] c)
    {

        c[0].SET(0);
        c[1].SET(0);

        for (int i = NumCards - 1; i > 0; i--)
        {
            if (hand[i] == 2)
            {
                if (c[0].IsEmpty())
                    c[0].SET(Convert.ToByte(i + 1));
                else
                {
                    c[1].SET(Convert.ToByte(i + 1));
                    //  avaliar for (int j = 2; j < BoardSize; j++) c[j].SET(0);
                    break;
                }
            }
        }

        if (!c[0].IsEmpty())
        {
            int j = 2;
            if (hand[NumCards - 1] != 2 & hand[NumCards - 1] != 0)
            {
                c[j].SET(1);
                j++;
            }

            for (int i = NumCards - 2; i > 0; i--)
                if (i != c[0].value & i != c[1].value & hand[i] != 0)
                {
                    c[j].SET(Convert.ToByte(i + 1));
                    j++;
                    if (j == BoardSize) return true;
                }

            return true;
        }

        return false;
    }

    public bool GetHighCards(ref CARD[] c)
    {
        int j = 0;

        for (int i = NumCards - 1; i >= 1; i--)
        {
            if (hand[i] != 0)
            {
                c[j].SET(Convert.ToByte(i + 1));
                j++;
                if (j == BoardSize) return true;
            }
        }

        return true;
    }

    public PokerHands ReturnPokerHand(ref CARD[] c)
    {

        if (CheckStraightFlush(ref c))
        {
            if (c[0].value == 0) return PokerHands.RoyalStraightFlush;
            return PokerHands.StraightFlush;
        }

        if (CheckFour(ref c)) return PokerHands.Four;

        if (CheckFullHouse(ref c)) return PokerHands.FullHouse;

        if (CheckFlush(ref c)) return PokerHands.Flush;

        if (CheckStraight(ref c)) return PokerHands.Straight;

        if (CheckTrips(ref c)) return PokerHands.Trips;

        if (CheckPair(ref c))
        {
            if (c[1].IsEmpty()) return PokerHands.Pair;
            return PokerHands.TwoPairs;
        }

        GetHighCards(ref c);

        return PokerHands.HighCard;
    }

    private bool AddBoard(int c, int ind)
    {
        bool res = board[ind].CardNumber == 0;
        if (!res) return res;

        CARD newBoardCard = new CARD(Convert.ToByte(c));
        board[ind] = newBoardCard;

        hand[newBoardCard.value]++;
        suits[newBoardCard.suit]++;

        // The Ace is also placed at the end to facilitate the verification of sequences.
        if (newBoardCard.value == 0) hand[NumCards - 1]++;

        return res;
    }

    public bool AddRiver(int c)
    {
        return (AddBoard(c, 4));

    }

    public bool AddTurn(int c)
    {
        return (AddBoard(c, 3));
    }

    public bool SetHand(int[] cards)
    {
        int[] p = new int[2];
        int[] b = new int[cards.Length - 2];

        for (int i = 0; i < BoardSize; i++) b[i] = cards[i];
        for (int i = 0; i < pocketCardsSize; i++) p[i] = cards[i + BoardSize];
        return SetHand(p, b);

    }

    public bool SetHand(int[] pocket, int[] boardCards)
    {
        bool res = (pocketCards.Length == pocketCardsSize) & ((boardCards.Length == 0) | ((boardCards.Length >= 3) & boardCards.Length <= BoardSize));
        CARD carta;

        if (!res) return res;
        this.ResetHand();

        pocketCards[0] = new CARD(Convert.ToByte(pocket[0]));
        pocketCards[1] = new CARD(Convert.ToByte(pocket[1]));

        for (int i = 0; i < boardCards.Length; i++) board[i] = new CARD(Convert.ToByte(boardCards[i]));

        for (int i = 0; i < pocketCards.Length; i++)
        {
            carta = pocketCards[i];
            hand[carta.value]++;
            suits[carta.suit]++;
            if (carta.value == 0) hand[NumCards - 1]++;
        }

        for (int i = 0; i < boardCards.Length; i++)
        {
            carta = board[i];
            hand[carta.value]++;
            suits[carta.suit]++;
            if (carta.value == 0) hand[NumCards - 1]++;
        }

        return res;
    }

    public bool SetHand(string s)
    {
        int c;
        s = s.Replace(" ", "");
        s = s.ToUpper();
        c = s.Length / 2;

        string[] p = new string[pocketCardsSize];
        string[] b = new string[BoardSize];

        if (c < BoardSize) return false;


        for (int i = 0; i < BoardSize; i++)
        {
            b[i] = s.Substring(2 * i, 2);
        }

        p[0] = s.Substring(2 * BoardSize, 2);
        p[1] = s.Substring(2 * (BoardSize + 1), 2);

        return SetHand(p, b);

    }

    public bool SetHand(string[] pocket, string[] boardCards)
    {

        bool res = (pocketCards.Length == pocketCardsSize) & ((boardCards.Length == 0) | ((boardCards.Length >= 3) & boardCards.Length <= BoardSize));
        if (!res) return res;

        int[] p = new int[pocketCardsSize];
        int[] b = new int[boardCards.Length];

        for (int i = 0; i < p.Length; i++) p[i] = CARD.ToInt(pocket[i]);
        for (int i = 0; i < b.Length; i++) b[i] = CARD.ToInt(boardCards[i]);

        return SetHand(p, b);
    }

    private void ClassInitiator(int pocketCard1, int pocketCard2)
    {

        hand = new byte[NumCards];
        suits = new byte[Numsuits];
        pocketCards = new CARD[pocketCardsSize];
        board = new CARD[BoardSize];

        this.ResetHand();

        pocketCards[0] = new CARD(Convert.ToByte(pocketCard1));
        pocketCards[1] = new CARD(Convert.ToByte(pocketCard1));
    }

    public void ResetHand()
    {

        for (int i = 0; i < Numsuits; i++) suits[i] = 0;
        for (int i = 0; i < NumCards; i++) hand[i] = 0;
        for (int i = 0; i < BoardSize; i++) board[i] = new CARD(0);
        pocketCards[0] = new CARD(0);
        pocketCards[1] = new CARD(0);
    }

    public bool SetRandomHand(int players, int boardcards, Random R)
    {

        bool res = (boardcards == 0 | (boardcards >= 3 & boardcards <= BoardSize));
        res &= (players >= 1 & players <= 10);
        if (!res) return res;

        int next = 0;
        int[] c = new int[boardcards + 2 * players];
        bool ok;

        for (int i = 0; i < c.Length; i++)
        {
            ok = false;
            while (!ok)
            {
                next = R.Next(1, TotalCards + 1);
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

        int[] b = new int[boardcards];
        int[] p = new int[pocketCardsSize];
        for (int i = 0; i < boardcards; i++) { b[i] = c[i]; }
        for (int i = 0; i < pocketCardsSize; i++) { p[i] = c[i + boardcards]; }

        SetHand(p, b);

        return res;
    }

    public bool SetRandomHand(HoldemPokerHand h, Random R)
    {
        CARD[] boardcards = h.GetBoard();
        CARD[] pocket = h.GetPocketCards();

        bool res = (boardcards.Length == 0 | (boardcards.Length >= 3 & boardcards.Length <= BoardSize));
        if (!res) return res;

        int next = 0;
        int[] c = new int[boardcards.Length + 4];
        int[] b = new int[boardcards.Length];
        int[] p = new int[pocketCardsSize];
        bool ok;

        for (int i = 0; i < boardcards.Length; i++)
        {
            b[i] = boardcards[i].CardNumber;
            c[i] = boardcards[i].CardNumber;

        }
        c[boardcards.Length] = pocket[0].CardNumber;
        c[boardcards.Length + 1] = pocket[1].CardNumber;

        for (int i = boardcards.Length + 2; i < c.Length; i++)
        {
            ok = false;
            while (!ok)
            {
                next = R.Next(1, TotalCards + 1);
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

        for (int i = 0; i < pocketCardsSize; i++) { p[i] = c[i + boardcards.Length + 2]; }

        SetHand(p, b);

        return res;
    }


    public HoldemPokerHand(int pocketCard1, int pocketCard2)
    {
        ClassInitiator(pocketCard1, pocketCard2);
    }

    public HoldemPokerHand()
    {
        ClassInitiator(0, 0);
    }

    public HoldemPokerHand(int[] pocket)
    {
        if (pocket.Length == pocketCardsSize)
        {
            ClassInitiator(pocket[0], pocket[1]);
        }
        else throw new IndexOutOfRangeException("Vetor deve conter apenas duas cartas.");
    }

    public override string ToString()
    {
        string res = "";
        if (pocketCards[0].CardNumber == 0) return res;

        for (int i = 0; i < board.Length; i++)
            res += board[i].ToString() + " ";

        res += pocketCards[0].ToString() + " ";
        res += pocketCards[1].ToString();

        return res;
    }

    public string GetHandString()
    {
        string res = "";

        for (int i = 0; i < NumCards - 1; i++)
        {

            res += CARD.ReturnValueChar(i) + "-" + hand[i].ToString() + " | ";

        }
        return res;
    }

    public string GetSuitsString()
    {
        string res = "";
        for (int i = 0; i < Numsuits; i++)
        {

            res += CARD.ReturnSuitChar(i) + "-" + suits[i].ToString() + "|";

        }
        return res;
    }

}
