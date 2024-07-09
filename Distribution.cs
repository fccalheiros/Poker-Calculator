using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace PokerCalculator
{

    public partial class Distribution : Form
    {

        const int nPlayers = 7;

        private readonly Button[] PlayerButton;
        private int SelectedPlayer;
        private PokerButton SavedBT;

        private readonly bool[,,] SelectedButton;
        private readonly PokerButton[,] ArrayButton;

        private readonly PokerButton[,] HeroButton;
        private int HeroCardsSelected;
        private string HeroHandSTR;
        private string LastValidHeroHandSTR;

        private readonly PokerButton[,] BoardButton;
        private int BoardCardsSelected;
        private string BoardCardsSTR;
        private string LastValidBoardCardsSTR;

        private readonly int[] TrackBarCache;

        public int[,] CardsSelection;
        public ulong[] RangeSelection;
        private int selectionSize;

        private readonly string[] StrengthOrder = { "AA", "KK", "QQ", "JJ", "TT", "AKs", "99", "AQs", "AKo", "AJs", "KQs", "88", "ATs", "AQo", "KJs", "KTs", "QJs", "AJo", "KQo", "QTs",
                                   "A9s", "77", "ATo", "JTs", "KJo", "A8s", "K9s", "QJo", "A7s", "KTo", "Q9s", "A5s", "66", "A6s", "QTo", "J9s", "A9o", "T9s",
                                   "A4s", "K8s", "JTo", "K7s", "A8o", "A3s", "Q8s", "K9o", "A2s", "K6s", "J8s", "T8s", "A7o", "55", "Q9o", "98s", "K5s", "Q7s",
                                   "J9o","A5o", "T9o", "A6o", "K4s", "K8o", "Q6s", "J7s", "T7s", "A4o", "97s", "K3s", "87s", "Q5s", "K7o", "44", "Q8o", "A3o", "K2s",
                                   "J8o", "Q4s", "T8o", "J6s", "K6o", "A2o", "T6s", "98o", "76s", "86s", "96s", "Q3s", "J5s", "K5o", "Q7o", "Q2s", "J4s", "33", "65s",
                                   "J7o", "T7o", "K4o", "75s", "T5s", "Q6o", "J3s", "95s", "87o", "85s", "97o", "T4s", "K3o", "J2s", "54s", "Q5o", "64s", "T3s", "22",
                                   "K2o", "74s", "76o", "T2s", "Q4o", "J6o", "84s", "94s", "86o", "T6o", "96o", "53s", "93s", "Q3o", "J5o", "63s", "43s", "92s", "73s",
                                   "65o", "Q2o", "J4o", "83s", "75o", "52s", "85o", "82s", "T5o", "95o", "J3o", "62s", "54o", "42s", "T4o", "J2o", "72s", "64o", "T3o",
                                   "32s", "74o", "84o", "T2o", "94o", "53o", "93o", "63o", "43o", "92o", "73o", "83o", "52o", "82o", "42o", "62o", "72o", "32o" };

        private ulong win, loss, tie;
        private float tieEquity;

        private static readonly Mutex mut = new Mutex();

        private bool _mainwindow;

        public bool MainWindow
        {
            get { return _mainwindow; }
            set { _mainwindow = value; }
        }

        public int SelectionSize
        {
            get { return selectionSize; }
            set { selectionSize = value; }
        }

        public Distribution()
        {

            InitializeComponent();
            int size = 40;
            int space = 0;
            int start = 60;

            HeroButton = new PokerButton[4, 13];
            BoardButton = new PokerButton[4, 13];
            ArrayButton = new PokerButton[13, 13];
            CardsSelection = new int[51 * 52, 2];
            RangeSelection = new ulong[51 * 52];
            TrackBarCache = new int[nPlayers];
            HeroCardsSelected = 0;
            BoardCardsSelected = 0;
            HeroHandSTR = "";
            BoardCardsSTR = "";
            TBThreads.Text = Environment.ProcessorCount.ToString();
            tbNSimul.Text = "5000000";

            SelectedButton = new bool[nPlayers, 13, 13];
            for (int i = 0; i < nPlayers; i++)
                for (int b1 = 0; b1 < 13; b1++)
                    for (int b2 = 0; b2 < 13; b2++)
                        SelectedButton[i, b1, b2] = false;


            PlayerButton = new Button[nPlayers];
            PlayerButton[0] = BTP1;
            PlayerButton[1] = BTP2;
            PlayerButton[2] = BTP3;
            PlayerButton[3] = BTP4;
            PlayerButton[4] = BTP5;
            PlayerButton[5] = BTP6;
            PlayerButton[6] = BTP7;
            SelectedPlayer = 0;

            int delta = start - BTP1.Location.X;
            System.Drawing.Point L;
            for (int i = 0; i < nPlayers; i++)
            {
                L = new Point(PlayerButton[i].Location.X + delta, PlayerButton[i].Location.Y);
                PlayerButton[i].Location = L;
                TrackBarCache[i] = 0;

            }

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    ArrayButton[i, j] = new PokerButton(i, j)
                    {
                        Location = new System.Drawing.Point(start + i * (size + space), start + j * (size + space)),
                        Size = new System.Drawing.Size(size, size)
                    };

                    ArrayButton[i, j].Click += new System.EventHandler(this.BT_click);
                    ArrayButton[i, j].MouseMove += new System.Windows.Forms.MouseEventHandler(this.BT_MouseMove);
                    ArrayButton[i, j].MouseDown += new System.Windows.Forms.MouseEventHandler(this.BT_MouseDown);
                    ArrayButton[i, j].MouseDown += new System.Windows.Forms.MouseEventHandler(this.BT_MouseUP);

                    Controls.Add(ArrayButton[i, j]);
                }
            }

            SavedBT = ArrayButton[12, 12];

            TrackBAR.Location = new System.Drawing.Point(start, start + 14 * (size + space));
            TBPercentual.Location = new System.Drawing.Point(start + TrackBAR.Size.Width + 10, TrackBAR.Location.Y);
            TBThreads.Location = new System.Drawing.Point(TBPercentual.Location.X, TBPercentual.Location.Y + TBPercentual.Height + 10);
            tbNSimul.Location = new System.Drawing.Point(TBPercentual.Location.X +TBPercentual.Width + 10, TBPercentual.Location.Y);
            TBOutput.Location = new System.Drawing.Point(start, TrackBAR.Location.Y + TrackBAR.Size.Height + 20);

            Size = new Size(1250, 850);
            BTAll.Location = new System.Drawing.Point(start + 14 * (size + space), BTAll.Location.Y);
            BTSuited.Location = new System.Drawing.Point(start + 14 * (size + space), BTSuited.Location.Y);
            BTBroadway.Location = new System.Drawing.Point(start + 14 * (size + space), BTBroadway.Location.Y);
            BTPair.Location = new System.Drawing.Point(start + 14 * (size + space), BTPair.Location.Y);
            BTClear.Location = new System.Drawing.Point(start + 14 * (size + space), BTClear.Location.Y);
            BTClearAll.Location = new System.Drawing.Point(start + 14 * (size + space), BTClearAll.Location.Y);
            BTClearHero.Location = new System.Drawing.Point(start + 14 * (size + space), BTClearHero.Location.Y);
            BTClearBoard.Location = new System.Drawing.Point(start + 14 * (size + space), BTClearBoard.Location.Y);

            BTRUN.Location = new System.Drawing.Point(start + 14 * (size + space), BTRUN.Location.Y);

            BTCancelar.Location = new System.Drawing.Point(BTRUN.Location.X, BTRUN.Location.Y + BTRUN.Height + 20);
            BTFMPrincipal.Location = new System.Drawing.Point(BTCancelar.Location.X, BTCancelar.Location.Y + BTCancelar.Height + 40);

            TBHero.Location = new System.Drawing.Point(BTClearAll.Location.X + BTClearAll.Width + size, start + 13 * (size + space) + 20);
            TBHero.Width = 4 * (size + space);
            TBBoard.Location = new System.Drawing.Point(BTClearAll.Location.X + BTClearAll.Width + size + 5 * (size + space), start + 13 * (size + space) + 20);
            TBBoard.Width = TBHero.Width;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    HeroButton[i, j] = new PokerButton(13 * i + j, Color.FromArgb(224, 127, 127))
                    {
                        Location = new System.Drawing.Point(BTClearAll.Location.X + BTClearAll.Width + size + i * (size + space), start + j * (size + space)),
                        Size = new System.Drawing.Size(size, size)
                    };

                    BoardButton[i, j] = new PokerButton(13 * i + j, Color.FromArgb(188, 206, 211))
                    {
                        Location = new System.Drawing.Point(BTClearAll.Location.X + BTClearAll.Width + size + (i + 5) * (size + space), start + j * (size + space)),
                        Size = new System.Drawing.Size(size, size)
                    };

                    HeroButton[i, j].Refresh();
                    BoardButton[i, j].Refresh();

                    HeroButton[i, j].Click += new System.EventHandler(this.BTHEROCard_click);
                    BoardButton[i, j].Click += new System.EventHandler(this.BTBOARDCard_click);

                    Controls.Add(HeroButton[i, j]);
                    Controls.Add(BoardButton[i, j]);

                }
            }


        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (_mainwindow)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }
            else
            {
                Hide();
                if (Owner != null)
                {
                    Owner.Visible = false;
                    Owner.Show();
                }
            }
        }

        private void BTHEROCard_click(object sender, EventArgs e)
        {
            PokerButton bt = (PokerButton)sender;
            HeroCardsSelected += bt.Selected ? 1 : -1;

            if (bt.Selected) HeroHandSTR += bt.Text + " ";
            else HeroHandSTR = HeroHandSTR.Replace(bt.Text + " ", "");

            if (HeroCardsSelected > 2)
                bt.PerformClick();

            if (bt.Selected && BoardButton[bt.X, bt.Y].Selected)
                bt.PerformClick();

            TBHero.TextChanged -= new System.EventHandler(this.TBHero_TextChanged);
            TBHero.Text = PEval.OrderStringCardSet(HeroHandSTR);
            LastValidHeroHandSTR = TBHero.Text;
            TBHero.TextChanged += new System.EventHandler(this.TBHero_TextChanged);
        }

        private void BTBOARDCard_click(object sender, EventArgs e)
        {
            PokerButton bt = (PokerButton)sender;
            BoardCardsSelected += (bt.Selected == true) ? 1 : -1;

            if (bt.Selected) BoardCardsSTR += bt.Text + " ";
            else BoardCardsSTR = BoardCardsSTR.Replace(bt.Text + " ", "");

            if (BoardCardsSelected > 5)
                bt.PerformClick();

            if (bt.Selected && HeroButton[bt.X, bt.Y].Selected)
                bt.PerformClick();

            TBBoard.TextChanged -= new System.EventHandler(this.TBBoard_TextChanged);
            TBBoard.Text = PEval.OrderStringCardSet(BoardCardsSTR);
            LastValidBoardCardsSTR = TBBoard.Text;
            TBBoard.TextChanged += new System.EventHandler(this.TBBoard_TextChanged);
        }

        private void BT_click(object sender, EventArgs e)
        {
            //TBSelecao.Text += ((Button)sender).Text;

            PokerButton bt = (PokerButton)sender;
            SelectedButton[SelectedPlayer, bt.X, bt.Y] = bt.Selected;
        }

        private void BT_MouseUP(object sender, MouseEventArgs e)
        {
            this.Capture = false;
        }

        private void BT_MouseDown(object sender, MouseEventArgs e)
        {
            PokerButton bt = (PokerButton)sender;
            bt.PerformClick();
            this.Capture = true;

        }

        private void BT_MouseMove(object sender, MouseEventArgs e)
        {
            PokerButton bt = (PokerButton)sender;
            if (bt.X != SavedBT.X || bt.Y != SavedBT.Y)
            {
                SavedBT = bt;
                if (e.Button == MouseButtons.Left)
                {
                    bt.PerformClick();
                }
            }
        }

        private void BTAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    ArrayButton[i, j].SelectButton();
                }
            }
        }

        private void BTClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    ArrayButton[i, j].UnSelectButton();
                    SelectedButton[SelectedPlayer, i, j] = false;
                }
            }
            TrackBAR.Value = 0;
            TrackBarCache[SelectedPlayer] = 0;
            TBPercentual.Text = "";
        }

        private void BTClearHero_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    HeroButton[i, j].UnSelectButton();
                }
            }

            HeroCardsSelected = 0;
            HeroHandSTR = "";

        }

        private void BTClearBoard_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    BoardButton[i, j].UnSelectButton();
                }
            }
            BoardCardsSTR = "";
            BoardCardsSelected = 0;
        }

        private void BTClearAll_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    ArrayButton[i, j].UnSelectButton();
                    for (int v = 0; v < nPlayers; v++)
                        SelectedButton[v, i, j] = false;

                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    HeroButton[i, j].UnSelectButton();
                    BoardButton[i, j].UnSelectButton();
                }
            }

            BoardCardsSTR = "";
            BoardCardsSelected = 0;
            HeroHandSTR = "";
            HeroCardsSelected = 0;
            TrackBAR.Value = 0;
            TBPercentual.Text = "";
            for (int v = 0; v < nPlayers; v++)
                TrackBarCache[v] = 0;
        }

        private void BTSuited_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = i + 1; j < 13; j++)
                {
                    ArrayButton[j, i].SelectButton();
                }
            }
        }

        private void BTBroadway_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    ArrayButton[i, j].SelectButton();
                }
            }

        }

        private void BTPair_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 13; i++)
            {
                ArrayButton[i, i].SelectButton();
            }
        }

        private void BTFMPrincipal_Click(object sender, EventArgs e)
        {
            Program.FMMain.Show();
        }
        private void TBHero_TextChanged(object sender, EventArgs e)
        {

            TextBox tb = (TextBox)sender;
            string s = tb.Text.Replace(" ", "");
            int x, y;

            bool valid = PEval.IsValidStringCardSet(s); ;

            if (s.Length > 4 || !valid)
            {
                s = LastValidHeroHandSTR.Replace(" ", "");
                tb.Text = LastValidHeroHandSTR;
            }


            if (s.Length % 2 == 0)
            {
                tb.BackColor = System.Drawing.SystemColors.Window;

                for (x = 0; x < 4; x++)
                    for (y = 0; y < 13; y++)
                        if (HeroButton[x, y].Selected)
                            HeroButton[x, y].PerformClick();


                for (int i = 0; i < s.Length; i += 2)
                {
                    x = ToSuitNumber(s.Substring(i + 1, 1));
                    y = ToCardNumber(s.Substring(i, 1));
                    if (!HeroButton[x, y].Selected)
                        HeroButton[x, y].PerformClick();
                }

            }


        }

        private void TBBoard_TextChanged(object sender, EventArgs e)
        {

            TextBox tb = (TextBox)sender;
            string s = tb.Text.Replace(" ", "");
            int x, y;

            bool valid = PEval.IsValidStringCardSet(s); ;

            if (s.Length > 10 || !valid)
            {
                s = LastValidBoardCardsSTR.Replace(" ", "");
                tb.Text = LastValidHeroHandSTR;
            }


            if (s.Length % 2 == 0)
            {
                tb.BackColor = System.Drawing.SystemColors.Window;

                for (x = 0; x < 4; x++)
                    for (y = 0; y < 13; y++)
                        if (BoardButton[x, y].Selected)
                            BoardButton[x, y].PerformClick();


                for (int i = 0; i < s.Length; i += 2)
                {
                    x = ToSuitNumber(s.Substring(i + 1, 1));
                    y = ToCardNumber(s.Substring(i, 1));
                    if (!BoardButton[x, y].Selected)
                        BoardButton[x, y].PerformClick();
                }

            }

        }

        private void TrackBAR1_Scroll(object sender, EventArgs e)
        {
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 1;
            float p = 0;

            for (int i = 0; i < TrackBAR.Value; i++)
            {
                string s = StrengthOrder[i];
                int x = ToCardNumber(s.Substring(0, 1));
                int y = ToCardNumber(s.Substring(1, 1));
                if (s.Length == 2) { ArrayButton[x, y].SelectButton(); p += 6; }
                else if (StrengthOrder[i].Substring(2, 1).Equals("s")) { ArrayButton[y, x].SelectButton(); p += 4; }
                else { ArrayButton[x, y].SelectButton(); p += 12; }
            }
            for (int i = TrackBAR.Value; i < 169; i++)
            {
                string s = StrengthOrder[i];
                int x = ToCardNumber(s.Substring(0, 1));
                int y = ToCardNumber(s.Substring(1, 1));
                if (s.Length == 2) { ArrayButton[x, y].UnSelectButton(); }
                else if (StrengthOrder[i].Substring(2, 1).Equals("s")) { ArrayButton[y, x].UnSelectButton(); }
                else { ArrayButton[x, y].UnSelectButton(); }
            }

            p /= (52 * 51 / 2);
            TBPercentual.Text = p.ToString("P", nfi);
            TrackBarCache[SelectedPlayer] = TrackBAR.Value;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            /*
                        Graphics g = e.Graphics;
                        using (Pen selPen = new Pen(Color.Blue))
                        {
                            g.DrawRectangle(selPen, 10, 10, 50, 50);
                        }
              */
            Point L;
            Rectangle R1;

            Rectangle R = new Rectangle(TrackBAR.Location.X - 2, TrackBAR.Location.Y - 2,
                 TrackBAR.ClientRectangle.Width + 4, TrackBAR.ClientRectangle.Height + 4);

            ControlPaint.DrawBorder(e.Graphics, R,
                 System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                 System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                 System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                 System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset);


            for (int i = 0; i < nPlayers; i++)
            {
                L = PlayerButton[i].Location;
                R1 = PlayerButton[i].ClientRectangle;
                R = new Rectangle(L.X, L.Y, R1.Width, R1.Height);
                ControlPaint.DrawBorder(e.Graphics, R,
                        System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                        System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                        System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                        System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset);
            }
            L = PlayerButton[SelectedPlayer].Location;
            R1 = PlayerButton[SelectedPlayer].ClientRectangle;
            R = new Rectangle(L.X, L.Y, R1.Width, R1.Height);

            ControlPaint.DrawBorder(e.Graphics, R,
                System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset);


        }

        private void Distribution_Resize(object sender, EventArgs e)
        {
            // TBPercentual.Text = Size.Width.ToString() + " " + Size.Height.ToString();
        }

        private void BTCancelar_Click(object sender, EventArgs e)
        {
            TBOutput.Text = "";

        }

        private int ToCardNumber(string s)
        {
            s = s.ToUpper();
            switch (s[0])
            {
                case 'A': return 0;
                case 'K': return 1;
                case 'Q': return 2;
                case 'J': return 3;
                case 'T': return 4;
                default: return 14 - Convert.ToInt32(s);
            }

        }
        private int ToCardNumber(int c)
        {
            switch (c)
            {
                case 0: return 1; //Ace
                default: return (14 - c);
            }
        }
        private int ToCardNumberNew(int c)
        {
            return (12 - c);
        }

        private int ToSuitNumber(string s)
        {
            int suit;
            s = s.ToUpper();
            switch (s[0])
            {
                case 'C': suit = 0; break;
                case 'H': suit = 1; break;
                case 'S': suit = 2; break;
                case 'D': suit = 3; break;
                default: suit = -1; break;
            }
            return suit;
        }


        private void BTRUN_Click(object sender, EventArgs e)
        {

            if (TBHero.Text.Replace(" ", "").Length != 4)
            {
                MessageBox.Show("Hero Cards incomplete.", "Error", MessageBoxButtons.OK);
                return;
            }

            int c = TBBoard.Text.Replace(" ", "").Length / 2;

            if (c == 1 || c == 2)
            {
                MessageBox.Show("Invalid Board", "Error", MessageBoxButtons.OK);
                return;
            }
            TBOutput.Text = "";
            RunMonteCarloSimulator();
        }

        private void BTPlayer_Click(object sender, EventArgs e)
        {

            Button bt = (Button)sender;
            int p = Convert.ToInt32(bt.Text.Replace("Player", "")) - 1;
            EventArgs e1 = new EventArgs();

            if (SelectedPlayer != p)
            {
                SelectedPlayer = p;

                TrackBAR.Value = TrackBarCache[SelectedPlayer];
                this.TrackBAR1_Scroll(TrackBAR, e1);

                for (int b1 = 0; b1 < 13; b1++)
                    for (int b2 = 0; b2 < 13; b2++)
                    {
                        ArrayButton[b1, b2].Selected = SelectedButton[SelectedPlayer, b1, b2];
                    }
            }
            this.Refresh();

        }

        public void GetRange(out int nVillains, out ulong[,] range, out int[] rangesize)
        {
            range = new ulong[nPlayers, 52 * 51 / 2];
            rangesize = new int[nPlayers];
            nVillains = 0;

            int c1N, c2N;

            for (int nv = 0; nv < nPlayers; nv++)
            {
                int index = 0;
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        if (SelectedButton[nv, i, j])
                        {
                            c1N = ToCardNumberNew(j);
                            c2N = ToCardNumberNew(i);

                            if (i > j) //suited          
                            {
                                for (int k1 = 0; k1 <= 39; k1 += 13)
                                {
                                    range[nVillains, index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << (c2N + k1));
                                    index++;
                                }
                            }
                            else if (i < j) //unsuited  - use of modulus to make code cleaner
                            {
                                c1N = ToCardNumberNew(i);
                                c2N = ToCardNumberNew(j);
                                for (int k1 = 0; k1 <= 39; k1 += 13)
                                {
                                    for (int k2 = 13; k2 <= 39; k2 += 13)
                                    {
                                        range[nVillains, index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << ((c2N + k1 + k2) % 52));
                                        index++;
                                    }
                                }
                            }
                            else  //pair
                            {
                                for (int k1 = 0; k1 <= 39; k1 += 13)
                                {
                                    for (int k2 = k1 + 13; k2 <= 39; k2 += 13)
                                    {
                                        range[nVillains, index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << (c2N + k2));
                                        index++;
                                    }
                                }
                            }

                        }
                    }
                }
                if (index > 0)
                {
                    rangesize[nVillains] = index;
                    nVillains++;
                }
            }
        }

        public void SETOLDCardSelection()
        {
            int index = 0;
            int c1, c2;
            int c1N, c2N;

            // old representation - A=1 .. K=13
            // new representation - 2=0 .. A =12
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    if (ArrayButton[i, j].Selected)
                    {
                        c1 = ToCardNumber(j);
                        c2 = ToCardNumber(i);
                        c1N = ToCardNumberNew(j);
                        c2N = ToCardNumberNew(i);

                        if (i > j) //suited          
                        {
                            for (int k1 = 0; k1 <= 39; k1 += 13)
                            {
                                CardsSelection[index, 0] = c1 + k1;
                                CardsSelection[index, 1] = c2 + k1;
                                RangeSelection[index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << (c2N + k1));
                                index++;
                            }
                        }
                        else if (i < j) //unsuited  - use of modulus to make code cleaner
                        {
                            c1 = ToCardNumber(i);
                            c2 = ToCardNumber(j);
                            c1N = ToCardNumberNew(i);
                            c2N = ToCardNumberNew(j);
                            for (int k1 = 0; k1 <= 39; k1 += 13)
                            {
                                for (int k2 = 13; k2 <= 39; k2 += 13)
                                {
                                    CardsSelection[index, 0] = c1 + k1;
                                    CardsSelection[index, 1] = ((c2 + k1 + k2 - 1) % 52) + 1;
                                    RangeSelection[index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << ((c2N + k1 + k2) % 52));
                                    index++;
                                }
                            }
                        }
                        else  //pair
                        {
                            for (int k1 = 0; k1 <= 39; k1 += 13)
                            {
                                for (int k2 = k1 + 13; k2 <= 39; k2 += 13)
                                {
                                    CardsSelection[index, 0] = c1 + k1;
                                    CardsSelection[index, 1] = c2 + k2;
                                    RangeSelection[index] = (CONSTANTS.ONE << (c1N + k1)) | (CONSTANTS.ONE << (c2N + k2));
                                    index++;
                                }
                            }
                        }

                    }
                }
            }

            selectionSize = index;
        }

        private void RunMonteCarloSimulator()
        {
            win = 0; tie = 0; loss = 0; tieEquity = 0;
            DateTime InitialDateTime = DateTime.Now;

            


            ulong herohand = PEval.ConvertStringToCardSet(HeroHandSTR);
            ulong currentBoard = PEval.ConvertStringToCardSet(BoardCardsSTR);
            int boardCardsLeft = 5 - BoardCardsSelected;
            int nThreads = Convert.ToInt32(TBThreads.Text);
            ulong nSimul = Convert.ToUInt64(tbNSimul.Text);
            ulong n = nSimul / (ulong)nThreads;

            int nVillains;
            ulong[,] range;
            int[] rangesize;

            GetRange(out nVillains, out range, out rangesize);

            PokerMonteCarloServer[] server = new PokerMonteCarloServer[nThreads];
            Thread[] PokerServer = new Thread[nThreads];

            for (int i = 0; i < nThreads-1; i++)
            {
                server[i] = new PokerMonteCarloServer(herohand, n, currentBoard, boardCardsLeft, range, rangesize, nVillains, new simulCallBack(ResultCallback));
                PokerServer[i] = new Thread(new ThreadStart(server[i].SimulateRangeN));
                PokerServer[i].Start();
            }
            server[nThreads - 1] = new PokerMonteCarloServer(herohand, nSimul-n*((ulong)nThreads-1), currentBoard, boardCardsLeft, range, rangesize, nVillains, new simulCallBack(ResultCallback));
            PokerServer[nThreads - 1] = new Thread(new ThreadStart(server[nThreads - 1].SimulateRangeN));
            PokerServer[nThreads - 1].Start();

            for (int i = 0; i < nThreads; i++)
            {
                PokerServer[i].Join();
            }

            ShowResults(InitialDateTime, DateTime.Now);
        }



        private void ShowResults(DateTime dtIni, DateTime dtFim)
        {
            ulong nSimul = win + loss + tie;
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.PercentDecimalDigits = 2;
            string s = "";
            s += "HERO: " + HeroHandSTR + " - " + win.ToString("N0", nfi) + " : " + loss.ToString("N0", nfi) + " : " + tie.ToString("N0", nfi);

            float w = ((float)(win) / (float)Convert.ToDecimal(nSimul));
            float l = ((float)(loss) / (float)Convert.ToDecimal(nSimul));
            float t = 1 - l - w;
            float equity = w + tieEquity / (float)Convert.ToDecimal(nSimul);
            s += " --- " + w.ToString("P", nfi) + " : " + l.ToString("P", nfi) + " : " + t.ToString("P", nfi) + " - Equity = " + equity.ToString("P", nfi);
            s += "\r\nBOARD: " + BoardCardsSTR;
            s += "\r\nProcessing Time (" + nSimul.ToString("N0", nfi) + ") :" + (dtFim - dtIni).ToString();

            TBOutput.Text = s;
        }

        public void ResultCallback(ulong _win, ulong _loss, ulong _tie, float _tieEquity)
        {
            // When there is more than one opponent, multiple ties may occur. Therefore, tie equity should be considered as an outcome of the process.
            mut.WaitOne();
            win += _win;
            loss += _loss;
            tie += _tie;
            tieEquity += _tieEquity;
            mut.ReleaseMutex();
        }

    }
}
