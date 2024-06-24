using System;


public struct CARD
{
	private byte seqnumber;
	public byte valor;
	public byte naipe;

	public byte SeqNumber   
	{
		get { return seqnumber; }  
		set { seqnumber = value; }  	
	}

	public char RetNaipeChar()
	{
		if (seqnumber == 0) return " "[0];
		return RetNaipeChar(naipe);
	}

	public void SetValue(string s)
	{
		valor = RetValorByte(s[0]);
		naipe = RetNaipeByte(s[1]);
		seqnumber = RetSeqNumber(valor,naipe);
	}

	public static int ToInt(string s)
	{
		s = s.ToUpper();
		byte v = RetValorByte(s[0]);
		byte n = RetNaipeByte(s[1]);
		return RetSeqNumber(v, n);
	}

	public char RetValorChar()
	{
		if (seqnumber == 0) return ' ';
		return RetValorChar(valor);
	}

	public static char RetValorChar(int n)
	{
		if (n == 0) return 'A';
		if (n <= 8) return (n+1).ToString()[0];
		if (n == 9) return 'T';
		if (n == 10) return 'J';
		if (n == 11) return 'Q';
		if (n == 12) return 'K';
		return new char();
	}

	public static byte RetValorByte(char s) 
	{
		byte v;
		switch (s)
		{
			case 'A': v = 0; break;
			case 'T': v = 9; break;
			case 'J': v = 10; break;
			case 'Q': v = 11; break;
			case 'K': v = 12; break;
			default : v = Convert.ToByte(Convert.ToByte(s.ToString()) - 1); break;
		}
		return v;
	}

	public static char RetNaipeChar(int n)
	{
		//'hearts'(copas), 'diamonds'(ouros), 'clubs'(paus) e 'spades'

		if (n == 0) return 'c';
		if (n == 1) return 'h';
		if (n == 2) return 's';
		if (n == 3) return 'd';
		return new char();
	}

	public static byte RetNaipeByte(char s)
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

	public static byte RetSeqNumber(byte v, byte n) 
	{
		return Convert.ToByte(v + 1 + n * 13);
	}

	public bool IsEmpty()
    {
		return (seqnumber == 0);
    }

	public void SET(byte seq)
	{
		if (seq == 0)
		{
			seqnumber = 0;
			valor = 50;
			naipe = 50;
			return;
		}

		seqnumber = seq;
		valor = Convert.ToByte((seqnumber - 1) % 13);
		naipe = Convert.ToByte((seqnumber - valor) / 13);
	}

	public void SET(int seq) { SET(Convert.ToByte(seq));  }
	public void SET(CARD c)
    {
		seqnumber = c.SeqNumber;
		valor = c.valor;
		naipe = c.naipe;
	}

	public CARD(byte seq)
	{
		seqnumber = 0;
		valor = 50;
		naipe = 50;

		SET(seq);
	}

	public override string ToString() => $"{RetValorChar()}{RetNaipeChar()}";

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

	//cartas são número de 1 .. 52 --- A até K e Paus - copas - espadas - ouro
	// naipe 0 - paus - 1 copas - 2 espadas - 3 ouros

	const int TotalCards = 52;
	const int NumCards = 14;
	const int NumNaipes = 4;
	const int BoardSize = 5;
	const int pocketSize = 2;

	private CARD [] pocketCards;
	private CARD [] board;

	private byte[] hand;
	private byte[] naipes;

	public CARD [] GetBoard () { return board;  }
	public CARD[] GetPocketCards() { return pocketCards; }

	public int [] GetCardsNumbers()
    {
		int[] cards = new int[7];
		for (int i = 0; i < 5; i++) cards[i] = board[i].SeqNumber;
		cards[5] = pocketCards[0].SeqNumber;
		cards[6] = pocketCards[1].SeqNumber;
		return cards;
	}

	private CARD GetHighCard()
	{
		CARD c;

		c = new CARD(0);

		for (byte i = NumCards; i >= 1 ; i--)
		{
			if (hand[i] > 0)
			{
				c.SET(i);
				return c;
			}

		}

		return c;
	}

	private CARD GetHighCard(int naipe)
    {
		CARD c;

		int res = -1;
		c = new CARD(0);

		for (int i = 0; i < pocketSize; i++)
        {
			if (pocketCards[i].naipe == naipe)
			{
				if (pocketCards[i].valor == 0)
				{
					c.SET(pocketCards[i].SeqNumber);
					return c;
				}
				if (pocketCards[i].valor > res)
				{
					res = pocketCards[i].valor;
					c.SET(pocketCards[i].SeqNumber);
				}

			}
        }

		for (int i = 0; i < board.Length; i++)
		{
			if (board[i].naipe == naipe)
			{
				if (board[i].valor == 0)
				{
					c.SET(board[i].SeqNumber);
					return c;
				}
				if (board[i].valor > res)
				{
					res = board[i].valor;
					c.SET(board[i].SeqNumber);
				}
			}
		}

		return c;
    }

	public bool FindCard(ref CARD c)
    {
		for (int i = 0; i < board.Length; i++)
			if (c.SeqNumber == board[i].SeqNumber) return true;

		if (c.SeqNumber == pocketCards[0].SeqNumber) return true;
		if (c.SeqNumber == pocketCards[1].SeqNumber) return true;

		return false;

	}

	public bool GetHighCards(ref CARD[] c, int naipe)
	{
		int j = 0;
		string s = ToString();

		for (int i = NumCards - 1; i >= 1; i--)
		{
			if (hand[i] != 0)
			{
				c[j].SET(Convert.ToByte(i + 1));
				c[j].naipe = Convert.ToByte(naipe);
				if (s.IndexOf(c[j].ToString()) >= 0) {
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

		ha = RetornaJogo(ref c);
		hb = b.RetornaJogo(ref c1);

		if (ha < hb) return -1;
		if (ha > hb) return 1;

		// Tratamento do As. Deve ser considerado a carta mais alta.
		for (int i = 0; i < BoardSize; i++)
        {
			if (c[i].valor == 0 & ! c[i].IsEmpty() ) c[i].valor = 13;
			if (c1[i].valor == 0 & !c[i].IsEmpty() ) c1[i].valor = 13;
        }

		if (ha == PokerHands.Pair)
		{
			if (c[0].valor < c1[0].valor) return -1;
			if (c[0].valor > c1[0].valor) return 1;
			for (int i = 2; i < BoardSize; i++)
			{
				if (c[i].valor < c1[i].valor) return -1;
				if (c[i].valor > c1[i].valor) return 1;
			}
			return 0;
		}

		if (ha == PokerHands.Trips | ha == PokerHands.TwoPairs)
		{
			for (int i = 0; i < 3; i++)
			{
				if (c[i].valor < c1[i].valor) return -1;
				if (c[i].valor > c1[i].valor) return 1;
			}
			return 0;
		}

		if (ha == PokerHands.Flush | ha == PokerHands.HighCard)
		{
			for (int i = 0; i < BoardSize; i++)
			{
				if (c[i].valor < c1[i].valor) return -1;
				if (c[i].valor > c1[i].valor) return 1;
			}
			return 0;
		}

		if (ha == PokerHands.FullHouse | ha == PokerHands.Four)
		{
			for (int i = 0; i < 2; i++)
			{
				if (c[i].valor < c1[i].valor) return -1;
				if (c[i].valor > c1[i].valor) return 1;
			}
			return 0;
		}

		if (ha == PokerHands.StraightFlush | ha == PokerHands.Straight) 
		{
			if (c[0].valor < c1[0].valor) return -1;
			if (c[0].valor > c1[0].valor) return 1;
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

		for (int i = 0; i < NumNaipes; i++)
			if (naipes[i] >= BoardSize)
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
				c[cont].naipe = Convert.ToByte(n);
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

		if (! c[0].IsEmpty())
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
			if (i != carta &  (hand[i] == 3 | hand[i] == 2) )
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
		
		for (int i = 0; i < NumNaipes; i++)
			if (naipes[i] >= BoardSize)
			{
				GetHighCards(ref c , i);
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
			if (hand[i] == 2) {
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
		
		if ( ! c[0].IsEmpty()  )
        {
			int j = 2;
			if ( hand[NumCards -1] != 2 & hand[NumCards - 1] != 0)
            {
				c[j].SET(1);
				j++;
			}

			for (int i = NumCards - 2; i > 0; i--)
				if ( i != c[0].valor & i != c[1].valor  & hand[i] !=0 )
				{
					c[j].SET(Convert.ToByte(i + 1));
					j++;
					if (j == BoardSize) return true;
				}

			return true;
		}

		return false;
	}

	public bool GetHighCards(ref CARD [] c)
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

	public PokerHands RetornaJogo(ref CARD [] c)
    {
		
		if (CheckStraightFlush(ref c))
            {
				if (c[0].valor == 0) return PokerHands.RoyalStraightFlush;
				return PokerHands.StraightFlush;
			}

		if ( CheckFour(ref c) ) return PokerHands.Four;
 
		if ( CheckFullHouse(ref c) ) return PokerHands.FullHouse;

		if ( CheckFlush(ref c) ) return PokerHands.Flush;

		if ( CheckStraight(ref c) ) return PokerHands.Straight;

		if (CheckTrips(ref c)) return PokerHands.Trips;

		if (CheckPair(ref c))
        {
			if (c[1].IsEmpty()) return PokerHands.Pair;
			return PokerHands.TwoPairs;
        }

		GetHighCards(ref c);
		
		return PokerHands.HighCard;
    }

	private bool  AddBoard(int c, int ind)
    {
		bool res = board[ind].SeqNumber == 0;
		if (!res) return res;

		CARD carta = new CARD(Convert.ToByte(c)) ;
		board[ind] = carta;

		hand[carta.valor]++;
		naipes[carta.naipe]++;

		// o Ás também é colocado no final para facilitar a verificação de sequências.
		if (carta.valor == 0) hand[NumCards-1]++;

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

	public bool SetHand(int [] cards)
    {
		int[] p = new int[2];
		int[] b = new int[cards.Length - 2];

		for (int i = 0; i < BoardSize; i++) b[i] = cards[i];
		for (int i = 0; i < pocketSize; i++) p[i] = cards[i+BoardSize];
		return SetHand(p, b);

	}

	public bool SetHand(int [] pocket, int [] boardCards)
    {
		bool res = (pocketCards.Length == pocketSize) & ((boardCards.Length == 0) | ((boardCards.Length >= 3) & boardCards.Length <= BoardSize));
		CARD carta;

		if (!res) return res;
		this.ResetHand();

		pocketCards[0] = new CARD(Convert.ToByte(pocket[0]));
		pocketCards[1] = new CARD(Convert.ToByte(pocket[1]));

		for (int i = 0; i < boardCards.Length; i++) board[i] = new CARD(Convert.ToByte(boardCards[i]));

		for (int i = 0; i < pocketCards.Length; i++)
		{
			carta = pocketCards[i];
			hand[carta.valor]++;
			naipes[carta.naipe]++;
			if (carta.valor == 0) hand[NumCards-1]++;
		}

		for (int i = 0; i < boardCards.Length; i++)
		{
			carta = board[i];
			hand[carta.valor]++;
			naipes[carta.naipe]++;
			if (carta.valor == 0) hand[NumCards-1]++;
		}

		return res;
    }

	public bool SetHand(string s) 
	{
		int c;
		s = s.Replace(" ", "");
		s = s.ToUpper();
		c = s.Length / 2;

		string[] p = new string[pocketSize];
		string[] b = new string[BoardSize];

		if (c < BoardSize) return false;


		for (int i = 0; i < BoardSize; i++)
        {
			b[i] = s.Substring(2 * i, 2);
        }

		p[0] = s.Substring(2 * BoardSize, 2);
		p[1] = s.Substring(2 * (BoardSize + 1), 2);

		return SetHand(p,b);

	}

	public bool SetHand(string [] pocket, string [] boardCards)
    {

		bool res = (pocketCards.Length == pocketSize) & ((boardCards.Length == 0) | ((boardCards.Length >= 3) & boardCards.Length <= BoardSize));
		if (!res) return res;

		int[] p = new int[pocketSize];
		int[] b = new int[boardCards.Length];

		for (int i = 0; i < p.Length; i++) p[i] = CARD.ToInt(pocket[i]) ;
		for (int i = 0; i < b.Length; i++) b[i] = CARD.ToInt(boardCards[i]);

		return SetHand(p,b);
	}

	private void ClassInitiator (int pocketCard1, int pocketCard2)
	{
	
		hand = new byte[NumCards];
		naipes = new byte[NumNaipes];
		pocketCards = new CARD[pocketSize];
		board = new CARD[BoardSize];
		
		this.ResetHand();
		
		pocketCards[0] = new CARD(Convert.ToByte(pocketCard1));
		pocketCards[1] = new CARD(Convert.ToByte(pocketCard1));
	}

	public void ResetHand()
    {

		for (int i = 0; i < NumNaipes; i++) naipes[i] = 0;
		for (int i = 0; i < NumCards; i++) hand[i] = 0;
		for (int i = 0; i < BoardSize; i++) board[i] = new CARD(0);
		pocketCards[0] = new CARD(0);
		pocketCards[1] = new CARD(0);
	}

	public bool SetRandomHand(int players, int boardcards, Random R) {

		bool res = (boardcards == 0 | (boardcards >= 3 & boardcards <= BoardSize));
		res &= (players >= 1 & players <= 10);
		if (!res) return res;

		int next = 0;
		int[] c = new int[boardcards+2*players];
		bool ok;
		
		for ( int i = 0; i < c.Length; i++)
        {
			ok = false;
			while (!ok)
			{
				next = R.Next(1, TotalCards+1);
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
		int[] p = new int[pocketSize];
		for (int i = 0; i < boardcards; i++) { b[i] = c[i]; }
		for (int i = 0; i < pocketSize; i++) { p[i] = c[i+boardcards]; }

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
		int[] p = new int[pocketSize];
		bool ok;

		for (int i = 0; i < boardcards.Length; i++)
        {
			b[i] = boardcards[i].SeqNumber;
			c[i] = boardcards[i].SeqNumber;

		}
		c[boardcards.Length] = pocket[0].SeqNumber;
		c[boardcards.Length+1] = pocket[1].SeqNumber;

		for (int i = boardcards.Length +  2 ; i < c.Length; i++)
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

		for (int i = 0; i < pocketSize; i++) { p[i] = c[i + boardcards.Length + 2]; }

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
		if (pocket.Length == pocketSize)
		{
			ClassInitiator(pocket[0], pocket[1]);
		}
		else throw new IndexOutOfRangeException("Vetor deve conter apenas duas cartas.");
	}

	public override string ToString()
    {
		string res = "";
		if (pocketCards[0].SeqNumber == 0) return res;

		for (int i = 0; i < board.Length; i++)
			res +=  board[i].ToString() + " ";

		res += pocketCards[0].ToString()+ " ";
		res += pocketCards[1].ToString();

		return res;
	}

	public string GetHandString()
    {
		string res = "";

		for ( int i = 0; i < NumCards - 1 ; i++)
        {

			res += CARD.RetValorChar(i) + "-" + hand[i].ToString() + " | ";

        }
		return res;
    }

	public string GetNaipesString()
	{
		string res = "";
		for (int i = 0; i < NumNaipes; i++)
		{

			res += CARD.RetNaipeChar(i) + "-" + naipes[i].ToString() + "|";

		}
		return res;
	}

}
