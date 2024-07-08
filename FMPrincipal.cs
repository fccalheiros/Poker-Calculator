using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;


namespace PokerCalculator
{
    public partial class FMPrincipal : Form
    {

        private int[,] SimulaGanhou;
        private int[,] SimulaPerdeu;
        private int[,] SimulaEmpate;

        private ulong w, l, t;
        float et;

        /*
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
        */

        //hands=["4 of a Kind", "Straight Flush", "Straight", "Flush", "High Card", "1 Pair", "2 Pair", "Royal Flush", "3 of a Kind", "Full House" ];
        private readonly int[] forca = { 7, 8, 4, 5, 0, 1, 2, 9, 3, 6 };


        private void IniciaSimula()
        {
            SimulaGanhou = new int[52, 52];
            SimulaPerdeu = new int[52, 52];
            SimulaEmpate = new int[52, 52];
            for (int i = 0; i < 52; i++)
                for (int j = 0; j < 52; j++)
                {
                    SimulaGanhou[i, j] = 0;
                    SimulaPerdeu[i, j] = 0;
                    SimulaEmpate[i, j] = 0;
                }
        }

        private void SetResultadoGanhou(ref CARD[] p)
        {
            SimulaGanhou[p[0].CardNumber - 1, p[1].CardNumber - 1]++;
            SimulaGanhou[p[1].CardNumber - 1, p[0].CardNumber - 1]++;
        }

        private void SetResultadoGanhou(int c1, int c2)
        {
            SimulaGanhou[c1 - 1, c2 - 1]++;
            SimulaGanhou[c2 - 1, c1 - 1]++;
        }
        private void SetResultadoPerdeu(int c1, int c2)
        {
            SimulaPerdeu[c1 - 1, c2 - 1]++;
            SimulaPerdeu[c2 - 1, c1 - 1]++;
        }

        private void SetResultadoPerdeu(ref CARD[] p)
        {
            SimulaPerdeu[p[0].CardNumber - 1, p[1].CardNumber - 1]++;
            SimulaPerdeu[p[1].CardNumber - 1, p[0].CardNumber - 1]++;
        }
        private void SetResultadoEmpate(ref CARD[] p)
        {
            SimulaEmpate[p[0].CardNumber - 1, p[1].CardNumber - 1]++;
            SimulaEmpate[p[1].CardNumber - 1, p[0].CardNumber - 1]++;
        }

        private void SetResultadoEmpate(int c1, int c2)
        {
            SimulaEmpate[c1 - 1, c2 - 1]++;
            SimulaEmpate[c2 - 1, c1 - 1]++;
        }

        private void PrintResultado(TextBox tb, DateTime dt, ulong nSimul, int carta, int carta1)
        {
            carta--;
            carta1--;
            NumberFormatInfo nfi = new CultureInfo("en-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 2;
            CARD c = new CARD();
            CARD c1 = new CARD();
            string s = "\r\n Tempo Total (" + nSimul.ToString("D") + " ) :" + (DateTime.Now - dt).ToString();

            c.SET(carta + 1);
            c1.SET(carta1 + 1);
            s += "\r\n" + c.ToString() + c1.ToString() + " - " + SimulaGanhou[carta, carta1].ToString() + " : " + SimulaPerdeu[carta, carta1].ToString() + " : " + SimulaEmpate[carta, carta1].ToString();
            float g = ((float)(SimulaGanhou[carta, carta1]) / (float)Convert.ToDecimal(SimulaGanhou[carta, carta1] + SimulaPerdeu[carta, carta1] + SimulaEmpate[carta, carta1]));
            float p = ((float)(SimulaPerdeu[carta, carta1]) / (float)Convert.ToDecimal(SimulaGanhou[carta, carta1] + SimulaPerdeu[carta, carta1] + SimulaEmpate[carta, carta1]));
            float e = 1 - p - g;
            float equity = g + e / 2;
            s += " --- " + g.ToString("P", nfi) + " : " + p.ToString("P", nfi) + " : " + e.ToString("P", nfi) + " - Equity = " + equity.ToString("P", nfi);

            tb.Text = s;
        }

        private void PrintResultado(TextBox tb, DateTime dt, ulong nSimul, string cards, ulong win, ulong lost, ulong tie)
        {

            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 2;
            string s = "\r\n Tempo Total (" + nSimul.ToString("D") + " ) :" + (DateTime.Now - dt).ToString();
            s += "\r\n" + cards + " - " + win.ToString() + " : " + lost.ToString() + " : " + tie.ToString();
            float g = ((float)(win) / (float)Convert.ToDecimal(win + lost + tie));
            float p = ((float)(lost) / (float)Convert.ToDecimal(win + lost + tie));
            float e = 1 - p - g;
            float equity = g + e / 2;
            s += " --- " + g.ToString("P", nfi) + " : " + p.ToString("P", nfi) + " : " + e.ToString("P", nfi) + " - Equity = " + equity.ToString("P", nfi);

            tb.Text = s;
        }

        private void PrintResultado(TextBox tb, DateTime dt, ulong nSimul, string cards, ulong win, ulong lost, ulong tie, float eqTie)
        {

            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 2;
            string s = "\r\n Tempo Total (" + nSimul.ToString("D") + " ) :" + (DateTime.Now - dt).ToString();
            s += "\r\n" + cards + " - " + win.ToString() + " : " + lost.ToString() + " : " + tie.ToString();
            float g = ((float)(win) / (float)Convert.ToDecimal(nSimul));
            float p = ((float)(lost) / (float)Convert.ToDecimal(nSimul));
            float e = 1 - p - g;
            float equity = g + eqTie / (float)Convert.ToDecimal(nSimul);
            s += " --- " + g.ToString("P", nfi) + " : " + p.ToString("P", nfi) + " : " + e.ToString("P", nfi) + " - Equity = " + equity.ToString("P", nfi);

            tb.Text = s;
        }


        private void PrintResultado(TextBox tb, DateTime dt, ulong nSimul)
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 2;
            CARD c = new CARD();
            CARD c1 = new CARD();
            string s = "\r\n Tempo Total (" + nSimul.ToString("D") + " ) :" + (DateTime.Now - dt).ToString();
            for (int i = 0; i < 52; i++)
                for (int j = i + 1; j < 52; j++)
                {
                    c.SET(i + 1);
                    c1.SET(j + 1);
                    s += "\r\n" + c.ToString() + c1.ToString() + " - " + SimulaGanhou[i, j].ToString() + " : " + SimulaPerdeu[i, j].ToString() + " : " + SimulaEmpate[i, j].ToString();
                    float g = ((float)(SimulaGanhou[i, j]) / (float)Convert.ToDecimal(SimulaGanhou[i, j] + SimulaPerdeu[i, j] + SimulaEmpate[i, j]));
                    float p = ((float)(SimulaPerdeu[i, j]) / (float)Convert.ToDecimal(SimulaGanhou[i, j] + SimulaPerdeu[i, j] + SimulaEmpate[i, j]));
                    float e = 1 - p - g;
                    float equity = g + e / 2;
                    s += " --- " + g.ToString("P", nfi) + " : " + p.ToString("P", nfi) + " : " + e.ToString("P", nfi) + " - Equity = " + equity.ToString("P", nfi);
                }
            tb.Text = s;
        }

        public FMPrincipal()
        {
            InitializeComponent();

        }

        private void testehandtwopairs()
        {
            HoldemPokerHand hand = new HoldemPokerHand();
            hand.SetHand(" 2h 2d 2c 7h Kh 4d  Qd");
            CARD[] c = new CARD[5];

            TB01.Text = "";
            TB01.Text += "Hand: " + hand.ToString();
            TB01.Text += "\r\nJogo: " + HoldemPokerHand.GetHandName(hand.ReturnPokerHand(ref c)) + " : ";
            for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
            //  5c 9e 5e 4c tc Qo 4o
        }

        private bool TestaRandom()
        {
            int[] cont = new int[52];
            int soma = 0;
            int next;
            Random R = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 52; i++) cont[i] = 0;

            for (int i = 0; i < 10000; i++)
            {
                next = R.Next(1, 53);
                cont[next - 1]++;
            }

            for (int i = 0; i < 52; i++)
            {
                soma += cont[i];
                TB01.Text += "\r\n" + (i + 1).ToString() + " - " + cont[i].ToString();
            }
            TB01.Text += "\r\nSoma:" + soma.ToString();
            return true;
        }

        private bool TestaCartas()
        {
            CARD c1 = new CARD();
            CARD c2 = new CARD();

            for (byte i = 1; i <= 52; i++)
            {
                c1.SET(i);
                c2.SetValue(c1.ToString());
                if (c1.CardNumber != c2.CardNumber)
                    return false;
            }
            return true;
        }

        private bool TestaTodosStraightFlush()
        {

            CARD[] c = new CARD[5];
            CARD carta = new CARD();
            string s;
            int cont = 0;
            HoldemPokerHand hand = new HoldemPokerHand();
            s = "Ac Kc Qc Jc 5s Tc 8s";
            hand.SetHand(s);
            if (hand.ReturnPokerHand(ref c) != PokerHands.RoyalStraightFlush) return false;
            s = "Ad Kd Qd 8s 5s Jd Td";
            hand.SetHand(s);
            if (hand.ReturnPokerHand(ref c) != PokerHands.RoyalStraightFlush) return false;
            s = "As Ks Qs Js 5d Ts 8s";
            hand.SetHand(s);
            if (hand.ReturnPokerHand(ref c) != PokerHands.RoyalStraightFlush) return false;
            s = "Ad Kd Qd Jd 5s Td 8s";
            hand.SetHand(s);
            if (hand.ReturnPokerHand(ref c) != PokerHands.RoyalStraightFlush) return false;

            s = "4c 5c 6c 7c 8c 5d 9d";
            hand.SetHand(s);
            if (hand.ReturnPokerHand(ref c) != PokerHands.StraightFlush) return false;

            for (byte k = 0; k < 4; k++)
            {
                for (byte i = 1; i <= 9; i++)
                {
                    s = "";
                    for (byte j = i; j < i + 5; j++)
                    {
                        carta.SET(Convert.ToByte(j + 13 * k));
                        s += carta.ToString();
                    }
                    carta.SET(Convert.ToByte(i + 13 * ((k + 1) % 4)));
                    s += carta.ToString();
                    carta.SET(Convert.ToByte(i + 1 + 13 * ((k + 1) % 4)));
                    s += carta.ToString();
                    hand.SetHand(s);
                    if (hand.ReturnPokerHand(ref c) != PokerHands.StraightFlush)
                        return false;
                }
            }
            TB01.Text += "\r\nNúmero Straight Flush Testados: " + cont.ToString();
            return true;
        }

        private void TestaTodasCombinaçõesNovo()
        {
            int[] contador = new int[Convert.ToInt32(PokerHands.RoyalStraightFlush) + 1];
            for (int j = 0; j <= Convert.ToInt32(PokerHands.RoyalStraightFlush); j++) contador[j] = 0;
            int[] resultados = { 23294460, 58627800, 31433400, 6461620, 6180020, 4047644, 3473184, 224848, 37260, 4324 };

            PokerEval pe = new PokerEval();

            int cont = 0;
            decimal per = 0;
            int[] i = new int[7];
            HoldemPokerHand hand = new HoldemPokerHand();
            CARD[] c = new CARD[5];
            NumberFormatInfo nfi = new CultureInfo("en-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 4;
            DateTime dt = DateTime.Now;
            DateTime dtf = DateTime.Now;
            TB01.Text += "\r\n Início Testa Combinaçoes: " + dt.ToString();
            for (i[0] = 1; i[0] <= 52 - 6; i[0]++)
                for (i[1] = i[0] + 1; i[1] <= 52 - 5; i[1]++)
                    for (i[2] = i[1] + 1; i[2] <= 52 - 4; i[2]++)
                        for (i[3] = i[2] + 1; i[3] <= 52 - 3; i[3]++)
                            for (i[4] = i[3] + 1; i[4] <= 52 - 2; i[4]++)
                                for (i[5] = i[4] + 1; i[5] <= 52 - 1; i[5]++)
                                    for (i[6] = i[5] + 1; i[6] <= 52 - 0; i[6]++)
                                    {

                                        cont++;
                                        pe.RankPokerHandSEVENCards(ref i);
                                        //hand.SetHand(i);
                                        //PokerHands h = hand.ReturnPokerHand(ref c);
                                        contador[pe.HandPower]++;

                                        //if ( pe.HandPower != (int) h) {
                                        //  pe.RankPokerHandSEVENCards(ref i);
                                        //}

                                        if (pe.HandPower == 9)
                                        {
                                            //TB01.Text += "\r\n" + hand.ToString() + " : " ;
                                            //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
                                            per = (Convert.ToDecimal(cont) / (decimal)133784560);
                                            dtf = DateTime.Now;
                                            TB02.Text = cont.ToString("N", nfi) + " - " + per.ToString("P", nfi) + " - " + (dtf - dt).ToString();

                                            //TB01.SelectionStart = TB01.TextLength;
                                            //TB01.ScrollToCaret();
                                            Application.DoEvents();
                                        }
                                    }
            TB01.Text = "\r\n" + cont.ToString();
            TB01.Text += "\r\n";
            decimal acum = 0;
            bool correto = true;
            for (PokerHands j = PokerHands.RoyalStraightFlush; j >= 0; j--)
            {
                int vezes = contador[Convert.ToInt32(j)];
                per = Convert.ToDecimal(vezes) / Convert.ToDecimal(cont);
                acum += per;
                correto &= (vezes == resultados[Convert.ToInt32(j)]);
                TB01.Text += HoldemPokerHand.GetHandName(j) + " - " + vezes.ToString() + " - " + per.ToString("P", nfi) + " - " + acum.ToString("P", nfi) + "\r\n";
            }
            dtf = DateTime.Now;
            TB01.Text += "Funcionou? : " + correto.ToString() + "\r\n";
            TB01.Text += "Tempo Total: " + (dtf - dt).ToString();
            TB02.Text = cont.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (dtf - dt).ToString();
        }

        private void TestaTodasCombinaçõesNovissimo()
        {
            int[] contador = new int[Convert.ToInt32(PokerHands.RoyalStraightFlush) + 1];
            for (int j = 0; j <= Convert.ToInt32(PokerHands.RoyalStraightFlush); j++) contador[j] = 0;
            int[] resultados = { 23294460, 58627800, 31433400, 6461620, 6180020, 4047644, 3473184, 224848, 37260, 4324 };

            PokerEval pe = new PokerEval();

            int cont = 0;
            decimal per;
            int[] i = new int[7];
            HoldemPokerHand hand = new HoldemPokerHand();
            CARD[] c = new CARD[5];
            NumberFormatInfo nfi = new CultureInfo("en-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 4;
            DateTime dt = DateTime.Now;
            DateTime dtf = DateTime.Now;
            TB01.Text += "\r\n Início Testa Combinaçoes: " + dt.ToString();
            for (i[0] = 0; i[0] <= 51 - 6; i[0]++)
                for (i[1] = i[0] + 1; i[1] <= 51 - 5; i[1]++)
                    for (i[2] = i[1] + 1; i[2] <= 51 - 4; i[2]++)
                        for (i[3] = i[2] + 1; i[3] <= 51 - 3; i[3]++)
                            for (i[4] = i[3] + 1; i[4] <= 51 - 2; i[4]++)
                                for (i[5] = i[4] + 1; i[5] <= 51 - 1; i[5]++)
                                    for (i[6] = i[5] + 1; i[6] <= 51 - 0; i[6]++)
                                    {
                                        cont++;
                                        //pe.RankPokerHandSEVENCards(ref i);

                                        //ulong cardset = PEval.ConvertArrayToCardSet(ref i);
                                        ulong cardset = CONSTANTS.ONE << i[0] | CONSTANTS.ONE << i[1] | CONSTANTS.ONE << i[2] |
                                            CONSTANTS.ONE << i[3] | CONSTANTS.ONE << i[4] | CONSTANTS.ONE << i[5] | CONSTANTS.ONE << i[6];

                                        int strength = HoldemEval.ProcessCardSet(cardset);
                                        int game = PEval.ReturnHandPower(strength);
                                        contador[game]++;

                                        //if (pe.HandPower != game)
                                        // {
                                        //     pe.RankPokerHandSEVENCards(ref i);
                                        //    strength = HoldemEval.ProcessCardSet(cardset);
                                        //   game = PEval.ReturnHandPower(strength);
                                        // }
                                        /*
                                                                                if (game == 9)
                                                                                {
                                                                                    //TB01.Text += "\r\n" + hand.ToString() + " : " ;
                                                                                    //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
                                                                                    per = (Convert.ToDecimal(cont) / (decimal)133784560);
                                                                                    dtf = DateTime.Now;
                                                                                    TB02.Text = cont.ToString("N", nfi) + " - " + per.ToString("P", nfi) + " - " + (dtf - dt).ToString();

                                                                                    //TB01.SelectionStart = TB01.TextLength;
                                                                                    //TB01.ScrollToCaret();
                                                                                    Application.DoEvents();
                                                                                }
                                        */
                                    }
            TB01.Text = "\r\n" + cont.ToString();
            TB01.Text += "\r\n";
            decimal acum = 0;
            bool correto = true;
            for (PokerHands j = PokerHands.RoyalStraightFlush; j >= 0; j--)
            {
                int vezes = contador[Convert.ToInt32(j)];
                per = Convert.ToDecimal(vezes) / Convert.ToDecimal(cont);
                acum += per;
                correto &= (vezes == resultados[Convert.ToInt32(j)]);
                TB01.Text += HoldemPokerHand.GetHandName(j) + " - " + vezes.ToString() + " - " + per.ToString("P", nfi) + " - " + acum.ToString("P", nfi) + "\r\n";
            }
            dtf = DateTime.Now;
            TB01.Text += "Funcionou? : " + correto.ToString() + "\r\n";
            TB01.Text += "Tempo Total: " + (dtf - dt).ToString();
            TB02.Text = cont.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (dtf - dt).ToString();
        }

        private void TestaTodasCombinaçõesNovissimoBIT()
        {
            int[] contador;
            int[] resultados = { 23294460, 58627800, 31433400, 6461620, 6180020, 4047644, 3473184, 224848, 37260, 4324 };

            int cont = 133784560;
            decimal per;
            int[] i = new int[7];
            HoldemPokerHand hand = new HoldemPokerHand();
            CARD[] c = new CARD[5];
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 4;
            DateTime dt = DateTime.Now;
            DateTime dtf = DateTime.Now;
            TB01.Text += "\r\n Início Testa Combinaçoes: " + dt.ToString();

            contador = PEval.EvaluateAllCombinations();

            TB01.Text = "\r\n" + cont.ToString();
            TB01.Text += "\r\n";
            decimal acum = 0;
            bool correto = true;

            for (PokerHands j = PokerHands.RoyalStraightFlush; j >= 0; j--)
            {
                int vezes = contador[Convert.ToInt32(j)];
                per = Convert.ToDecimal(vezes) / Convert.ToDecimal(cont);
                acum += per;
                correto &= (vezes == resultados[Convert.ToInt32(j)]);
                TB01.Text += HoldemPokerHand.GetHandName(j) + " - " + vezes.ToString() + " - " + per.ToString("P", nfi) + " - " + acum.ToString("P", nfi) + "\r\n";
            }

            dtf = DateTime.Now;
            TB01.Text += "Funcionou? : " + correto.ToString() + "\r\n";
            TB01.Text += "Tempo Total: " + (dtf - dt).ToString();
            TB02.Text = cont.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (dtf - dt).ToString();
        }


        private void StressPokerEval()
        {
            ulong hand;
            int seq;
            int flush;
            int index;
            DateTime dtIni = DateTime.Now;
            HoldemPokerHand hhhh = new HoldemPokerHand();
            int[] num = { 0, 0, 5, 5, 10 };
            int[] suit = { 1, 2, 2, 4, 8 };
            int[] board = { 1, 14, 15, 28, 39, 50, 51 };
            ulong totSimul = Convert.ToUInt64(TBSimul.Text);

            hhhh.SetHand(board);
            TB01.Text = hhhh.ToString();

            for (ulong i = 0; i < totSimul; i++)
                RankPokerHand(num, suit, out hand, out seq, out flush, out index);
            TB02.Text = (DateTime.Now - dtIni).ToString();
        }
        private void TestaTodasCombinações()
        {
            int[] contador = new int[Convert.ToInt32(PokerHands.RoyalStraightFlush) + 1];
            for (int j = 0; j <= Convert.ToInt32(PokerHands.RoyalStraightFlush); j++) contador[j] = 0;
            int[] resultados = { 23294460, 58627800, 31433400, 6461620, 6180020, 4047644, 3473184, 224848, 37260, 4324 };

            int cont = 0;
            decimal per;
            int[] i = new int[7];
            HoldemPokerHand hand = new HoldemPokerHand();
            CARD[] c = new CARD[5];
            NumberFormatInfo nfi = new CultureInfo("en-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 4;
            DateTime dt = DateTime.Now;
            DateTime dtf = DateTime.Now;
            TB01.Text += "\r\n Início Testa Combinaçoes: " + dt.ToString();
            for (i[0] = 1; i[0] <= 52 - 6; i[0]++)
                for (i[1] = i[0] + 1; i[1] <= 52 - 5; i[1]++)
                    for (i[2] = i[1] + 1; i[2] <= 52 - 4; i[2]++)
                        for (i[3] = i[2] + 1; i[3] <= 52 - 3; i[3]++)
                            for (i[4] = i[3] + 1; i[4] <= 52 - 2; i[4]++)
                                for (i[5] = i[4] + 1; i[5] <= 52 - 1; i[5]++)
                                    for (i[6] = i[5] + 1; i[6] <= 52 - 0; i[6]++)
                                    {
                                        cont++;
                                        hand.SetHand(i);
                                        PokerHands h = hand.ReturnPokerHand(ref c);
                                        contador[Convert.ToInt32(h)]++;

                                        if (h == PokerHands.StraightFlush)
                                        {
                                            //TB01.Text += "\r\n" + hand.ToString() + " : " ;
                                            //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
                                            per = (Convert.ToDecimal(cont) / (decimal)133784560);
                                            dtf = DateTime.Now;
                                            TB02.Text = cont.ToString("N", nfi) + " - " + per.ToString("P", nfi) + " - " + (dtf - dt).ToString();

                                            //TB01.SelectionStart = TB01.TextLength;
                                            //TB01.ScrollToCaret();
                                            Application.DoEvents();
                                        }
                                    }
            TB01.Text = "\r\n" + cont.ToString();
            TB01.Text += "\r\n";
            decimal acum = 0;
            bool correto = true;
            for (PokerHands j = PokerHands.RoyalStraightFlush; j >= 0; j--)
            {
                int vezes = contador[Convert.ToInt32(j)];
                per = Convert.ToDecimal(vezes) / Convert.ToDecimal(cont);
                acum += per;
                correto &= (vezes == resultados[Convert.ToInt32(j)]);
                TB01.Text += HoldemPokerHand.GetHandName(j) + " - " + vezes.ToString() + " - " + per.ToString("P", nfi) + " - " + acum.ToString("P", nfi) + "\r\n";
            }
            dtf = DateTime.Now;
            TB01.Text += "Funcionou? : " + correto.ToString() + "\r\n";
            TB01.Text += "Tempo Total: " + (dtf - dt).ToString();
        }


        private void TestaRandomGames()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);
            PokerEval pe = new PokerEval();
            PokerEval pe1 = new PokerEval();
            HoldemPokerHand hand = new HoldemPokerHand();
            HoldemPokerHand hand1 = new HoldemPokerHand();
            CARD[] c = new CARD[5];
            CARD[] c1 = new CARD[5];
            CARD[] p;
            CARD[] p1;
            int[] cards;
            int[] cards1;
            PokerHands h1, h;

            DateTime dtini = DateTime.Now;
            IniciaSimula();

            int res;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            for (ulong i = 0; i < nSimul; i++)
            {
                hand.SetRandomHand(1, 5, R);
                hand1.SetRandomHand(hand, R);
                res = hand.Compare(hand1);

                p = hand.GetPocketCards();
                p1 = hand1.GetPocketCards();

                cards = hand.GetCardsNumbers();
                cards1 = hand1.GetCardsNumbers();

                pe.RankPokerHandSEVENCards(ref cards);
                pe1.RankPokerHandSEVENCards(ref cards1);

                h = hand.ReturnPokerHand(ref c);
                h1 = hand1.ReturnPokerHand(ref c1);

                int res1 = pe.HandCompare7Cards(pe1);

                if (res != res1)
                {
                    res = hand.Compare(hand1);
                    res1 = pe.HandCompare7Cards(pe1);
                }

                //TB01.Text += "\r\nJogo 1: " + HoldemPokerHand.GetHandName(h) + " : " + hand.ToString() + " : ";
                //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
                //TB01.Text += "\r\nJogo 2: " + HoldemPokerHand.GetHandName(h1) + " : " + hand1.ToString() + " : ";
                //for (int j = 0; j < 5; j++) TB01.Text += c1[j].ToString() + " - ";

                if (res == 0)
                {
                    SetResultadoEmpate(ref p);
                    SetResultadoEmpate(ref p1);
                }
                else if (res == 1)
                {
                    SetResultadoGanhou(ref p);
                    SetResultadoPerdeu(ref p1);
                }

                else
                {
                    SetResultadoGanhou(ref p1);
                    SetResultadoPerdeu(ref p);
                }
                if ((i & 16383) == 16383)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            PrintResultado(TB01, dtini, nSimul);
            nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();

        }

        private void TestaRandomGames2()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);
            PokerEval pe = new PokerEval();
            PokerEval pe1 = new PokerEval();
            int[] c = new int[9];
            DateTime dtini = DateTime.Now;
            IniciaSimula();

            const ulong mask = 65535;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            for (ulong i = 0; i < nSimul; i++)
            {
                PokerEval.SetRandomHand(pe, pe1, R, c);
                //pe.RankPokerHandSEVENCards(ref cards);
                //pe1.RankPokerHandSEVENCards(ref cards1);

                int res = pe.HandCompare7Cards(pe1);

                if (res == 0)
                {
                    SetResultadoEmpate(pe.Cards[5], pe.Cards[6]);
                    SetResultadoEmpate(pe1.Cards[5], pe1.Cards[6]);
                }
                else if (res == 1)
                {
                    SetResultadoGanhou(pe.Cards[5], pe.Cards[6]);
                    SetResultadoPerdeu(pe1.Cards[5], pe1.Cards[6]);
                }

                else
                {
                    SetResultadoGanhou(pe1.Cards[5], pe1.Cards[6]);
                    SetResultadoPerdeu(pe.Cards[5], pe.Cards[6]);
                }

                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul);

        }

        private void TestaRandomGames3()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);
            PokerEval pe = new PokerEval();
            PokerEval pe1 = new PokerEval();
            int[] c = new int[9];
            DateTime dtini = DateTime.Now;
            IniciaSimula();

            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            int carta1 = CARD.ToInt(TBPocket.Text.Substring(0, 2));
            int carta2 = CARD.ToInt(TBPocket.Text.Substring(3, 2));
            c[0] = carta1;
            c[1] = carta2;

            for (ulong i = 0; i < nSimul; i++)
            {
                PokerEval.RandomHand(c, R);
                pe.RankPokerHandSEVENCards(ref c);
                c[0] = c[7];
                c[1] = c[8];
                pe1.RankPokerHandSEVENCards(ref c);
                c[0] = carta1;
                c[1] = carta2;


                int res = pe.HandCompare7Cards(pe1);

                if (res == 0)
                {
                    SetResultadoEmpate(pe.Cards[0], pe.Cards[1]);
                    SetResultadoEmpate(pe1.Cards[0], pe1.Cards[1]);
                }
                else if (res == 1)
                {
                    SetResultadoGanhou(pe.Cards[0], pe.Cards[1]);
                    SetResultadoPerdeu(pe1.Cards[0], pe1.Cards[1]);
                }

                else
                {
                    SetResultadoGanhou(pe1.Cards[0], pe1.Cards[1]);
                    SetResultadoPerdeu(pe.Cards[0], pe.Cards[1]);
                }

                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, carta1, carta2);

        }

        private void TestaRandomGamesRange()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);
            PokerEval pe = new PokerEval();
            PokerEval pe1 = new PokerEval();
            int[] c = new int[9];
            DateTime dtini = DateTime.Now;
            IniciaSimula();

            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            int carta1 = CARD.ToInt(TBPocket.Text.Substring(0, 2));
            int carta2 = CARD.ToInt(TBPocket.Text.Substring(3, 2));
            c[0] = carta1;
            c[1] = carta2;

            for (ulong i = 0; i < nSimul; i++)
            {
                //public void GetRange(out int nVillains, out ulong[,] range, out int[] rangesize)
                PokerEval.RandomHand(c, Program.FMDistribution.CardsSelection, Program.FMDistribution.SelectionSize, R);
                pe.RankPokerHandSEVENCards(ref c);
                c[0] = c[7];
                c[1] = c[8];
                pe1.RankPokerHandSEVENCards(ref c);
                c[0] = carta1;
                c[1] = carta2;


                int res = pe.HandCompare7Cards(pe1);

                if (res == 0)
                {
                    SetResultadoEmpate(pe.Cards[0], pe.Cards[1]);
                    SetResultadoEmpate(pe1.Cards[0], pe1.Cards[1]);
                }
                else if (res == 1)
                {
                    SetResultadoGanhou(pe.Cards[0], pe.Cards[1]);
                    SetResultadoPerdeu(pe1.Cards[0], pe1.Cards[1]);
                }

                else
                {
                    SetResultadoGanhou(pe1.Cards[0], pe1.Cards[1]);
                    SetResultadoPerdeu(pe.Cards[0], pe.Cards[1]);
                }

                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, carta1, carta2);

        }


        private void TestaRandomGamesFast()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);

            DateTime dtini = DateTime.Now;


            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);
            ulong win = 0, tie = 0, lost = 0;

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong villainhand;
            ulong board;

            int heroResult;
            int villainResult;

            int[] c = new int[9];
            int carta1 = CARD.ToInt(TBPocket.Text.Substring(0, 2));
            int carta2 = CARD.ToInt(TBPocket.Text.Substring(3, 2));
            c[0] = carta1;
            c[1] = carta2;

            for (ulong i = 0; i < nSimul; i++)
            {

                //PokerEval.RandomHand(c, R);
                //heroResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                //c[0] = c[7];
                //c[1] = c[8];

                //villainResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                //c[0] = carta1;
                //c[1] = carta2;

                HoldemEval.RandomHand(herohand, 0, 5, out board, out villainhand, R);
                heroResult = HoldemEval.ProcessCardSet(herohand | board);
                villainResult = HoldemEval.ProcessCardSet(villainhand | board);

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;


                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, win, lost, tie);

        }


        private void TestaRandomGamesRangeFast()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);

            DateTime dtini = DateTime.Now;


            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);
            ulong win = 0, tie = 0, lost = 0;

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong villainhand;
            ulong board;

            int heroResult;
            int villainResult;

            int[] c = new int[9];
            int carta1 = CARD.ToInt(TBPocket.Text.Substring(0, 2));
            int carta2 = CARD.ToInt(TBPocket.Text.Substring(3, 2));
            c[0] = carta1;
            c[1] = carta2;

            for (ulong i = 0; i < nSimul; i++)
            {

                //PokerEval.RandomHand(c, R);
                //heroResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                //c[0] = c[7];
                //c[1] = c[8];

                //villainResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                //c[0] = carta1;
                //c[1] = carta2;


                //PokerEval.RandomHand(c, Program.FMDistribution.CardsSelection, Program.FMDistribution.SelectionSize, R);


                HoldemEval.RandomHandRange(herohand, 0, 5, Program.FMDistribution.RangeSelection, Program.FMDistribution.SelectionSize, out board, out villainhand, R);
                heroResult = HoldemEval.ProcessCardSet(herohand | board);
                villainResult = HoldemEval.ProcessCardSet(villainhand | board);

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;


                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, win, lost, tie);

        }

        private void TestaRandomGamesRangeFastN()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;


            DateTime dtini = DateTime.Now;

            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);
            ulong win = 0, tie = 0, lost = 0;
            float eqTie = 0;

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong currentBoard = PEval.ConvertStringToCardSet(TBBoard.Text);
            int boardCardsLeft = 5 - (TBBoard.Text.Replace(" ", "").Length / 2);
            ulong[] villainhand;
            ulong board;

            int heroResult;



            int nVillains = 2;
            int bestvillainResult;

            ulong[,] range = new ulong[nVillains, Program.FMDistribution.SelectionSize];
            int[] rangesize = new int[nVillains];

            /*
            for (int i = 0; i < nVillains; i++) 
            {
                rangesize[i] = Program.FMDistribution.SelectionSize;
                for (int j = 0; j < Program.FMDistribution.SelectionSize; j++) 
                {
                    range[i, j] = Program.FMDistribution.RangeSelection[j];
                }
            }
            */

            Program.FMDistribution.GetRange(out nVillains, out range, out rangesize);
            int[] villainResult = new int[nVillains];

            Random R = new Random(DateTime.Now.Millisecond);

            for (ulong i = 0; i < nSimul; i++)
            {

                HoldemEval.RandomHandRange(herohand, currentBoard, boardCardsLeft, nVillains, range, rangesize, out board, out villainhand, R);
                heroResult = HoldemEval.ProcessCardSet(herohand | board);
                bestvillainResult = 0;

                for (int v = 0; v < nVillains; v++)
                {
                    villainResult[v] = HoldemEval.ProcessCardSet(villainhand[v] | board);
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

                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, win, lost, tie, eqTie);

        }

        private void TestaThread()
        {
            w = 0; t = 0; l = 0; et = 0;
            DateTime dtini = DateTime.Now;

            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong currentBoard = PEval.ConvertStringToCardSet(TBBoard.Text);
            int boardCardsLeft = 5 - (TBBoard.Text.Replace(" ", "").Length / 2);
            int nThreads = 8;
            ulong n = nSimul / (ulong)nThreads;

            PokerMonteCarloServer[] server = new PokerMonteCarloServer[nThreads];
            Thread[] PokerServer = new Thread[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                server[i] = new PokerMonteCarloServer(herohand, n, currentBoard, boardCardsLeft, new simulCallBack(ResultCallback));
                PokerServer[i] = new Thread(new ThreadStart(server[i].Simulate));
                PokerServer[i].Start();
            }

            for (int i = 0; i < nThreads; i++)
            {
                PokerServer[i].Join();
            }

            PrintResultado(TB01, dtini, (w + l + t), TBPocket.Text, w, l, t, et);

        }

        public void ResultCallback(ulong _w, ulong _l, ulong _t, float _et)
        {
            w += _w;
            l += _l;
            t += _t;
            et += _et;
            //Console.WriteLine("Acionou a callback: " + w.ToString() + " " + l.ToString() + " " + t.ToString() + " " + et.ToString() + "  " + DateTime.Now.ToString()+ "." + DateTime.Now.Millisecond.ToString());
        }

        private void TestaThreadRange()
        {
            w = 0; t = 0; l = 0; et = 0;
            DateTime dtini = DateTime.Now;

            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong currentBoard = PEval.ConvertStringToCardSet(TBBoard.Text);
            int boardCardsLeft = 5 - (TBBoard.Text.Replace(" ", "").Length / 2);
            int nThreads = 8;
            ulong n = nSimul / (ulong)nThreads;

            PokerMonteCarloServer[] server = new PokerMonteCarloServer[nThreads];
            Thread[] PokerServer = new Thread[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                server[i] = new PokerMonteCarloServer(herohand, n, currentBoard, boardCardsLeft, Program.FMDistribution.RangeSelection, Program.FMDistribution.SelectionSize, new simulCallBack(ResultCallback));
                PokerServer[i] = new Thread(new ThreadStart(server[i].SimulateRange));
                PokerServer[i].Start();
            }

            for (int i = 0; i < nThreads; i++)
            {
                PokerServer[i].Join();
            }

            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, w, l, t, et);

        }

        private void TestaThreadRangeN()
        {
            w = 0; t = 0; l = 0; et = 0;
            DateTime dtini = DateTime.Now;

            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);
            ulong currentBoard = PEval.ConvertStringToCardSet(TBBoard.Text);
            int boardCardsLeft = 5 - (TBBoard.Text.Replace(" ", "").Length / 2);
            int nThreads = 8;
            ulong n = nSimul / (ulong)nThreads;

            int nVillains = 2;

            ulong[,] range = new ulong[nVillains, Program.FMDistribution.SelectionSize];
            int[] rangesize = new int[nVillains];

            /*
            for (int i = 0; i < nVillains; i++)
            {
                rangesize[i] = Program.FMDistribution.SelectionSize;
                for (int j = 0; j < Program.FMDistribution.SelectionSize; j++)
                {
                    range[i, j] = Program.FMDistribution.RangeSelection[j];
                }
            }
            */

            Program.FMDistribution.GetRange(out nVillains, out range, out rangesize);

            PokerMonteCarloServer[] server = new PokerMonteCarloServer[nThreads];
            Thread[] PokerServer = new Thread[nThreads];

            for (int i = 0; i < nThreads; i++)
            {
                server[i] = new PokerMonteCarloServer(herohand, n, currentBoard, boardCardsLeft, range, rangesize, nVillains, new simulCallBack(ResultCallback));
                PokerServer[i] = new Thread(new ThreadStart(server[i].SimulateRangeN));
                PokerServer[i].Start();
            }

            for (int i = 0; i < nThreads; i++)
            {
                PokerServer[i].Join();
            }

            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, w, l, t, et);

        }

        private void ORDENAMAOS()
        {
            w = 0; t = 0; l = 0; et = 0;
            DateTime dtini = DateTime.Now;

            ulong nSimul = Convert.ToUInt64(TBSimul.Text);

            ulong herohand;
            ulong currentBoard = 0;
            int boardCardsLeft = 5;
            int nThreads = 8;
            ulong n = nSimul / (ulong)nThreads;

            ulong c0, c1;
            for (c0 = 1; c0 < CONSTANTS.ONE << 13; c0 <<= 1)
                for (c1 = c0 << 1; c1 < CONSTANTS.ONE << 26; c1 <<= 1)
                {
                    {
                        w = 0; t = 0; l = 0; et = 0;
                        herohand = c0 | c1;
                        PokerMonteCarloServer[] server = new PokerMonteCarloServer[nThreads];
                        Thread[] PokerServer = new Thread[nThreads];

                        for (int i = 0; i < nThreads; i++)
                        {
                            server[i] = new PokerMonteCarloServer(herohand, n, currentBoard, boardCardsLeft, new simulCallBack(ResultCallback));
                            PokerServer[i] = new Thread(new ThreadStart(server[i].Simulate));
                            PokerServer[i].Start();
                        }

                        for (int i = 0; i < nThreads; i++)
                        {
                            PokerServer[i].Join();
                        }
                        Console.WriteLine(PEval.ToString(herohand) + " " + (((float)w + (float)et) / (float)nSimul).ToString());
                    }
                }

            // PrintResultado(TB01, dtini, (w + l + t), TBPocket.Text, w, l, t, et);

        }


        private void TestaRandomGamesFastCompare()
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.NumberDecimalDigits = 0;
            Random R = new Random(DateTime.Now.Millisecond);

            DateTime dtini = DateTime.Now;
            IniciaSimula();

            const ulong mask = (1 << 20) - 1;
            ulong nSimul = Convert.ToUInt64(TBSimul.Text);
            ulong win = 0, tie = 0, lost = 0;

            ulong herohand = PEval.ConvertStringToCardSet(TBPocket.Text);

            int heroResult;
            int villainResult;

            PokerEval pe = new PokerEval();
            PokerEval pe1 = new PokerEval();
            int[] c = new int[9];
            int carta1 = CARD.ToInt(TBPocket.Text.Substring(0, 2));
            int carta2 = CARD.ToInt(TBPocket.Text.Substring(3, 2));
            c[0] = carta1;
            c[1] = carta2;

            for (ulong i = 0; i < nSimul; i++)
            {

                PokerEval.RandomHand(c, R);
                pe.RankPokerHandSEVENCards(ref c);
                heroResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                c[0] = c[7];
                c[1] = c[8];


                pe1.RankPokerHandSEVENCards(ref c);
                villainResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                c[0] = carta1;
                c[1] = carta2;

                //PEval.RandomHand(herohand, out board, out villainhand, R);
                //heroResult = HoldemEval.ProcessCardSet(herohand | board);
                //villainResult = HoldemEval.ProcessCardSet(villainhand | board);

                int res = pe.HandCompare7Cards(pe1);
                int res2 = (heroResult > villainResult ? 1 : heroResult < villainResult ? -1 : 0);

                if (res != res2)
                {
                    heroResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                    c[0] = c[7];
                    c[1] = c[8];
                    villainResult = HoldemEval.ProcessCardSet(PEval.ConvertArrayToCardSet(ref c, 7));
                    c[0] = carta1;
                    c[1] = carta2;

                    res = pe.HandCompare7Cards(pe1);

                    pe1.RankPokerHandSEVENCards(ref c);
                    res = pe.HandCompare7Cards(pe1);

                }

                if (heroResult > villainResult)
                    win++;
                else if (heroResult < villainResult)
                    lost++;
                else
                    tie++;


                if ((i & mask) == mask)
                {
                    TB02.Text = i.ToString("N", nfi) + " - " + (Convert.ToDecimal(i) / Convert.ToDecimal(nSimul)).ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
                    Application.DoEvents();
                }
            }
            TB02.Text = nSimul.ToString("N", nfi) + " - " + 1.ToString("P", nfi) + " - " + (DateTime.Now - dtini).ToString();
            PrintResultado(TB01, dtini, nSimul, TBPocket.Text, win, lost, tie);

        }

        private void PoeTextoLabel(HoldemPokerHand hand)
        {

            CARD[] c = new CARD[5];
            CARD[] c2 = new CARD[5];
            PokerHands h;

            TB01.Text += "\r\n" + hand.ToString();
            TB01.Text += "\r\n" + hand.GetHandString();
            TB01.Text += "\r\n" + hand.GetSuitsString();
            TB01.Text += "\r\nSequência? : " + hand.CheckStraight(ref c).ToString() + " - " + c[0].ToString();
            TB01.Text += "\r\nFlush? : " + hand.CheckFlush(ref c).ToString() + " - " + c[0].ToString();
            TB01.Text += "\r\nFour? : " + hand.CheckFour(ref c).ToString() + " - " + c[0].ToString() + " - " + c[1].ToString();
            TB01.Text += "\r\nTrips? : " + hand.CheckTrips(ref c).ToString() + " - " + c[0].ToString();
            TB01.Text += "\r\nPair? : " + hand.CheckPair(ref c).ToString() + " - " + c[0].ToString() + " - " + c[1].ToString();

            h = hand.ReturnPokerHand(ref c);
            TB01.Text += "\r\nJogo : " + HoldemPokerHand.GetHandName(h);
            for (int i = 0; i < 5; i++) TB01.Text += " - " + c[i].ToString();

            TB01.Text += "\r\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            switch (CBTeste.SelectedIndex)
            {

                case 0: TestaRandomGames(); break;
                case 1: TestaRandomGames2(); break;
                case 2: TestaTodasCombinações(); break;
                case 3: TestaTodasCombinaçõesNovo(); break;
                case 4: StressPokerEval(); break;
                case 5: TestaVariasHands(); break;
                case 6: Testa7Cartas(); break;
                case 7: TestaUmaHand5Cartas(); break;
                case 8: TestaRandomGames3(); break;
                case 9: TestaRandomGamesRange(); break;
                case 10: TestaPEVAL(); break;
                case 11: TestaTodasCombinaçõesNovissimo(); break;
                case 12: TestaTodasCombinaçõesNovissimoBIT(); break;
                case 13: TestaRandomGamesFast(); break;
                case 14: TestaRandomGamesFastCompare(); break;
                case 15: TestaThread(); break;
                case 16: TestaThreadRange(); break;
                case 17: TestaRandomGamesRangeFast(); break;
                case 18: TestaRandomGamesRangeFastN(); break;
                case 19: TestaThreadRangeN(); break;
                case 20: ORDENAMAOS(); break;
                case 21: RandomDistTest(); break;
                case 22: TestaOmaha(); break;
                case 23: TestaEnumerated(); break;
                    // default: s += (_cardsValues[i] + 2).ToString(); break;
            }

        }

        private void TestaEnumerated()
        {
            string sVillainHand = "5s 3s";
            string sHeroHand = "Ad Ah";
            string sBoardCards = "As 7h 5c 2d";
            ulong heroHand = PEval.ConvertStringToCardSet(sHeroHand);
            ulong villainHand = PEval.ConvertStringToCardSet(sVillainHand);
            ulong boardCards = PEval.ConvertStringToCardSet(sBoardCards);

            HoldemEval.Enumerate(heroHand, villainHand, boardCards, out int win, out int loss, out int tie);
            TB01.Text = "Hero Hand: " + sHeroHand + "\r\n";
            TB01.Text += "Villain Hand: " + sVillainHand + "\r\n";
            TB01.Text += "Board: " + sBoardCards + "\r\n";
            TB01.Text += "win: " + win.ToString() + " loss: " + loss.ToString() + " tie: " + tie.ToString() + "\r\n";

        }

        private void TestaOmaha()
        {
            ulong pocket = 0b1100000000000000000000000000000000010000000000010000;
            ulong board  = 0b0011100000000000000000100000100000000000000000000000;
            ulong[] cards;
            OmahaEval.StripCardSet(pocket, out cards);
            TB01.Text =  Convert.ToString((long)pocket,2).PadLeft(52,'0') + "\r\n";
            for (int i = 0; i < 4;i++)
            {
                TB01.Text += Convert.ToString((long)cards[i], 2).PadLeft(52,'0') + "\r\n";
            }
            TB01.Text += PEval.ToString(pocket) + "\r\n";
            TB01.Text += PEval.ToString(board) + "\r\n";
            int hand = OmahaEval.ProcessCardSet(cards, board);
            TB01.Text += Convert.ToString(hand,16) + "\r\n";
            TB01.Text += PEval.GetHandName(hand);
        }

        private void TestaPEVAL()
        {
            int n = Convert.ToInt32(TBPocket.Text);
            TBPocket.Text = PEval.lastbit(n).ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            CARD[] c = new CARD[5];

            HoldemPokerHand hand = new HoldemPokerHand();
            HoldemPokerHand hand1 = new HoldemPokerHand();
            TB01.Text = "";


            int[] p = { 01, 13 };
            int[] b = { 12, 11, 10, 50, 48 };
            int[] p1 = { 8, 9 };
            hand.SetHand(p, b);
            hand1.SetHand(p1, b);

            // Royal x straigth flush
            if (hand.Compare(hand1) != 1)
                TB01.Text = "Erro teste Royal x Straight";
            else
                TB01.Text += "Royal x Straight: " + hand.Compare(hand1).ToString() + "\r\n";
            TB01.Text += hand.ToString() + "\r\n";
            TB01.Text += HoldemPokerHand.GetHandName(hand.ReturnPokerHand(ref c)) + "\r\n";
            TB01.Text += hand1.ToString() + "\r\n";
            TB01.Text += HoldemPokerHand.GetHandName(hand1.ReturnPokerHand(ref c)) + "\r\n";


            // Four x Four
            int[] p2 = { 13, 14 };
            int[] b1 = { 12, 25, 38, 51, 01 };
            int[] p3 = { 15, 16 };
            hand.SetHand(p2, b1);
            hand1.SetHand(p3, b1);
            if (hand.Compare(hand1) != 0)
                TB01.Text += "Erro teste Four  x Four";
            else
                TB01.Text += "Four x Four: " + hand.Compare(hand1).ToString() + "\r\n";
            TB01.Text += hand.ToString() + "\r\n";
            TB01.Text += hand1.ToString() + "\r\n";

            // Four x full
            int[] p4 = { 13, 51 };
            int[] b2 = { 12, 25, 38, 14, 01 };
            int[] p5 = { 15, 27 };
            hand.SetHand(p4, b2);
            hand1.SetHand(p5, b2);
            if (hand.Compare(hand1) != 1)
                TB01.Text += "Erro teste Four  x Full";
            else
                TB01.Text += "Four x Full: " + hand.Compare(hand1).ToString() + "\r\n";
            TB01.Text += hand.ToString() + "\r\n";
            TB01.Text += hand1.ToString() + "\r\n";

            //TestaRandomGames();
            TestaRandomGames2();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int[] contador = new int[Convert.ToInt32(PokerHands.RoyalStraightFlush) + 1];
            for (int i = 0; i <= Convert.ToInt32(PokerHands.RoyalStraightFlush); i++) contador[i] = 0;

            Random R = new Random(DateTime.Now.Millisecond);
            TB01.Text = "";
            HoldemPokerHand hand = new HoldemPokerHand();
            NumberFormatInfo nfi = new CultureInfo("en-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 4;
            CARD[] c = new CARD[5];

            int totHands = 133784560;
            //int totHands = 1000000;
            for (int i = 0; i < totHands; i++)
            {
                hand.SetRandomHand(1, 5, R);
                //hand.SetHand("9E QE 8E JC 7O TE AE");

                PokerHands h = hand.ReturnPokerHand(ref c);
                contador[Convert.ToInt32(h)]++;

                if (h == PokerHands.RoyalStraightFlush)
                {
                    //TB01.Text += "\r\n" + hand.ToString() + " : " ;
                    //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
                    TB01.Text = i.ToString();
                    TB01.SelectionStart = TB01.TextLength;
                    TB01.ScrollToCaret();
                    Application.DoEvents();
                }


                // TB01.Text += "\r\nRandom Hand: " + hand.ToString();
                // TB01.Text += "\r\nJogo: " + HoldemPokerHand.GetHandName(hand.ReturnPokerHand(ref c)) + " : ";
                //for (int j = 0; j < 5; j++) TB01.Text += c[j].ToString() + " - ";
            }

            TB01.Text += "\r\n";
            decimal acum = 0;
            for (PokerHands i = PokerHands.RoyalStraightFlush; i >= 0; i--)
            {
                int vezes = contador[Convert.ToInt32(i)];
                decimal per = Convert.ToDecimal(vezes) / Convert.ToDecimal(totHands);
                acum += per;
                TB01.Text += HoldemPokerHand.GetHandName(i) + " - " + vezes.ToString() + " - " + per.ToString("P", nfi) + " - " + acum.ToString("P", nfi) + "\r\n";
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            TB01.Text = "";
            //TestaVariasHands();
            //Testa7Cartas();
            //TestaUmaHand5Cartas();
        }


        private void TestaVariasHands()
        {
            // { 7, 8, 4, 5, 6 };
            int[] num = { 0, 1, 1, 1, 1 };
            int[] suit = { 1, 2, 4, 8, 1 };

            TestaRankPokerHand(num, suit);

            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    num[j] = i + j;
                }
                TestaRankPokerHand(num, suit);
            }

        }

        private void TestaUmaHand5Cartas()
        {
            // { 7, 8, 4, 5, 6 };
            int[] num = { 10, 10, 7, 8, 9 };
            int[] suit = { 4, 2, 4, 4, 4 };

            TestaRankPokerHand(num, suit);


        }

        private void Testa7Cartas()
        {


            int[] p = { 14, 15, 16, 17, 18, 13, 12 };

            //int[] p2 = { 13, 14, 12, 25, 38, 51, 01 };

            int[] p2 = { 01, 12, 25, 38, 14, 13, 51 };

            int[] p3 = { 12, 25, 38, 14, 01, 15, 27 };

            RankPokerHand(p, out ulong hand, out int seq, out int flush, out int index);
            PrintHand(hand, seq, flush, index);

            RankPokerHand(p2, out hand, out seq, out flush, out index);
            PrintHand(hand, seq, flush, index);

            RankPokerHand(p3, out hand, out seq, out flush, out index);
            PrintHand(hand, seq, flush, index);
        }

        private void TestaRankPokerHand(int[] num, int[] suit)
        {

            RankPokerHand(num, suit, out ulong hand, out int seq, out int flush, out int index);
            PrintHand(hand, seq, flush, index);
        }

        private void PrintHand(ulong hand, int seq, int flush, int index)
        {
            int cont = 0;
            //TB01.Text += "\r\n HAND: " + hand.ToString();
            TB01.Text += "\r\n Mão: " + HoldemPokerHand.GetHandName((PokerHands)index);

            string s = "";
            ulong handAux = hand;
            while (handAux > 0)
            {
                s = (handAux & 1).ToString() + s;
                handAux >>= 1;
                cont++;
                if (cont % 4 == 0) s = "." + s;
            }
            if (s.Length % 5 == 0) s = s.Substring(1);

            TB01.Text += "\r\n" + hand.ToString() + " = " + s;
            s = "";
            int seqAux = seq;
            while (seqAux > 0)
            {
                s = (seqAux & 1).ToString() + s;
                seqAux >>= 1;
            }
            TB01.Text += "\r\nSequência: " + s + " = " + seq.ToString();
            TB01.Text += "\r\nFlush: " + flush.ToString();

            TB01.Text += "\r\n Divisão sequencia: " + (seq / (seq & -seq)).ToString();
            //TB01.Text += "\r\n Divisão minha: " + (seq & 15).ToString();
            TB01.Text += "\r\n ---------------------------------------------------";

        }

        private void RankPokerHand(int[] cartas, out ulong hand, out int seq, out int flush, out int index)
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
                                RankPokerHand(num, suit, out hand, out seq, out flush, out index);
                                if (index > bestindex)
                                {
                                    bestindex = index;
                                    besthand = hand;
                                }
                                else if ((index == bestindex) & (hand > besthand))
                                    besthand = hand;
                            }
                        }
                    }
                }
            }
            hand = besthand;
            index = bestindex;

        }

        private void RankPokerHand(int[] num, int[] suit, out ulong hand, out int seq, out int flush, out int index)
        {

            // falta resolver a comparação do A2345 x outras sequencias

            hand = 0;
            seq = 0;
            flush = 15;

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
            index -= (seq / (seq & -seq) == 31) | (seq == 4111) ? 3 : 1;
            index -= (flush > 0 ? 1 : 0) * (seq == 7936 ? -5 : 1);

            // zerar o bit 48 para poder comparar sequencias A2345  com as demais
            if (seq == 4111) hand ^= 281474976710656;
            index = forca[index];

        }

        private void FMPrincipal_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!Program.FMDistribution.Visible)
            {
                Program.FMDistribution.Show(this);
            }
            else
            {
                Program.FMDistribution.Focus();
            }

        }

        private void RandomDistTest()
        {
            Random R = new Random(DateTime.Now.Millisecond);
            int[] c = new int[4];

            for (int i = 0; i < 4; i++) c[i] = 0;


            for (int i = 0; i < Convert.ToInt32(TBSimul.Text); i++)
            {
                int next = R.Next(0, 4);
                //while (next == 2 || next == 3)
                //   next = R.Next(0, 4);
                c[next]++;
            }

            for (int i = 0; i < 4; i++) TB01.Text += i.ToString() + " - " + c[i].ToString() + "\r\n";
        }
    }
}
