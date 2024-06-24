using System;
using System.Drawing;
using System.Windows.Forms;

namespace PokerCalculator
{
    class PokerButton : Button
    {
        private int _i;
        private int _j;
        private bool _selected;
        private Color _maincolor;

        public Color MainColor
        {
            get { return _maincolor; }
            set { _maincolor = value; }
        }

        public int X
        {
            get { return _i; }
        }

        public int Y
        {
            get { return _j; }
        }

        public bool Selected
        {
            get { return _selected; }
            set { if (value) SelectButton(); else UnSelectButton(); _selected = value; }
        }

        private string Converte(int c)
        {
            switch (c)
            {
                case 0: return "A";
                case 1: return "K";
                case 2: return "Q";
                case 3: return "J";
                case 4: return "T";
                default: return (14 - c).ToString();
            }
        }

        private string Suit(int s)
        {

            switch (s)
            {
                case 0: return "c";
                case 1: return "h";
                case 2: return "s";
                case 3: return "d";
                default: return "";
            }
        }

        public PokerButton(int i, int j) : base()
        {
            _i = i;
            _j = j;
            _selected = false;

            Name = "BTR" + i.ToString() + j.ToString();
            Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            Margin = new System.Windows.Forms.Padding(0);
            Padding = new System.Windows.Forms.Padding(0);
            TabIndex = 13 * i + j;
            Font = new Font("Microsoft Sans Serif", 7);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;


            Text = Converte(j) + Converte(i);
            if (i > j)
            {
                Text += "s";
                _maincolor = Color.FromArgb(224, 127, 127);
            }
            else if (i < j)
            {
                Text = Converte(i) + Converte(j);
                Text += "o";
                _maincolor = Color.FromArgb(188, 206, 211);
            }
            else
            {
                _maincolor = Color.FromArgb(172, 255, 163);
            }
            BackColor = _maincolor;
            FlatAppearance.BorderSize = 2;

        }

        public PokerButton(int card, Color color) : base()
        {
            _i = card / 13;
            _j = card % 13;
            _selected = false;

            Name = "BTC" + _i.ToString() + _j.ToString();
            Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            Margin = new System.Windows.Forms.Padding(0);
            Padding = new System.Windows.Forms.Padding(0);
            TabIndex = 13 * _i + _j;
            Font = new Font("Microsoft Sans Serif", 7);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            FlatAppearance.MouseOverBackColor = SystemColors.ControlLight;

            Text = Converte(_j) + Suit(_i);
            _maincolor = color;
            BackColor = _maincolor;
            FlatAppearance.BorderSize = 2;

        }


        public void SelectButton()
        {
            if (!_selected) PerformClick();
        }

        public void UnSelectButton()
        {
            if (_selected) PerformClick();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!_selected)
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Outset);
            }
            else
            {
                ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset,
                    System.Drawing.SystemColors.ControlLightLight, 2, ButtonBorderStyle.Inset);
            }

        }

        protected override void OnClick(EventArgs e)
        {
            _selected = !_selected;
            if (_selected)
                BackColor = Color.FromArgb(200, 120, 200);
            else
                BackColor = _maincolor;


            base.OnClick(e);

        }
    }
}
