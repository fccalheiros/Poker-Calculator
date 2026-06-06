
namespace PokerCalculator
{
    partial class FMTest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.TB01 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.TB02 = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.CBTeste = new System.Windows.Forms.ComboBox();
            this.TBSimul = new System.Windows.Forms.TextBox();
            this.TBPocket = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.TBBoard = new System.Windows.Forms.TextBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.FlatAppearance.BorderSize = 2;
            this.button1.Location = new System.Drawing.Point(1347, 763);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(159, 50);
            this.button1.TabIndex = 1;
            this.button1.Text = "Run Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TB01
            // 
            this.TB01.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TB01.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TB01.Location = new System.Drawing.Point(52, 45);
            this.TB01.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TB01.Multiline = true;
            this.TB01.Name = "TB01";
            this.TB01.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TB01.Size = new System.Drawing.Size(1454, 690);
            this.TB01.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(52, 827);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(159, 72);
            this.button2.TabIndex = 3;
            this.button2.Text = "Compare";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button3.Location = new System.Drawing.Point(52, 749);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(159, 72);
            this.button3.TabIndex = 4;
            this.button3.Text = "Full Count";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // TB02
            // 
            this.TB02.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TB02.Enabled = false;
            this.TB02.Location = new System.Drawing.Point(835, 842);
            this.TB02.Name = "TB02";
            this.TB02.Size = new System.Drawing.Size(485, 30);
            this.TB02.TabIndex = 5;
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(235, 749);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(159, 50);
            this.button4.TabIndex = 6;
            this.button4.Text = "Clear Output";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // CBTeste
            // 
            this.CBTeste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CBTeste.FormattingEnabled = true;
            this.CBTeste.Items.AddRange(new object[] {
            "TestaRandomGames",
            "TestaRandomGames2",
            "TestaTodasCombinações",
            "TestaTodasCombinaçõesNovo",
            "StressPokerEval",
            "TestaVariasHands",
            "Testa7Cartas",
            "TestaUmaHand5Cartas",
            "TestaRandomGames3",
            "TestaRandomGamesRange",
            "TestaPEVAL",
            "TestaTodasCombinaçõesNovissimo",
            "TestaTodasCombinaçõesNovissimoBIT",
            "TestaRandomGamesFast",
            "TestaRandomGamesFastCompare",
            "TestaThread",
            "TestaThreadRange",
            "TestaRandomGamesRangeFast",
            "TestaRandomGamesRangeFastN",
            "TestaThreadRangeN",
            "ORDENAMAOS",
            "RandomDistTest",
            "TestaOmaha",
            "TestaEnumerated",
            "TestaEnumeratedSpeed"});
            this.CBTeste.Location = new System.Drawing.Point(1087, 763);
            this.CBTeste.Name = "CBTeste";
            this.CBTeste.Size = new System.Drawing.Size(233, 33);
            this.CBTeste.TabIndex = 7;
            // 
            // TBSimul
            // 
            this.TBSimul.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TBSimul.Location = new System.Drawing.Point(835, 763);
            this.TBSimul.Name = "TBSimul";
            this.TBSimul.Size = new System.Drawing.Size(227, 30);
            this.TBSimul.TabIndex = 8;
            this.TBSimul.Text = "1000000";
            // 
            // TBPocket
            // 
            this.TBPocket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TBPocket.Location = new System.Drawing.Point(610, 763);
            this.TBPocket.Name = "TBPocket";
            this.TBPocket.Size = new System.Drawing.Size(179, 30);
            this.TBPocket.TabIndex = 9;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Location = new System.Drawing.Point(235, 827);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(159, 50);
            this.button5.TabIndex = 10;
            this.button5.Text = "Ranges";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // TBBoard
            // 
            this.TBBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TBBoard.Location = new System.Drawing.Point(610, 842);
            this.TBBoard.Name = "TBBoard";
            this.TBBoard.Size = new System.Drawing.Size(179, 30);
            this.TBBoard.TabIndex = 11;
            //
            // buttonStop
            //
            this.buttonStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(1347, 830);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(159, 50);
            this.buttonStop.TabIndex = 12;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // FMPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1588, 925);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.TBBoard);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.TBPocket);
            this.Controls.Add(this.TBSimul);
            this.Controls.Add(this.CBTeste);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.TB02);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.TB01);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "FMPrincipal";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.FMPrincipal_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox TB01;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox TB02;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ComboBox CBTeste;
        private System.Windows.Forms.TextBox TBSimul;
        private System.Windows.Forms.TextBox TBPocket;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox TBBoard;
        private System.Windows.Forms.Button buttonStop;
    }
}

