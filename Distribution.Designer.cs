
using System.Windows.Forms;

namespace PokerCalculator
{
    partial class Distribution
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TBOutput = new System.Windows.Forms.TextBox();
            this.BTAll = new System.Windows.Forms.Button();
            this.BTSuited = new System.Windows.Forms.Button();
            this.BTPair = new System.Windows.Forms.Button();
            this.BTBroadway = new System.Windows.Forms.Button();
            this.BTClear = new System.Windows.Forms.Button();
            this.TrackBAR = new System.Windows.Forms.TrackBar();
            this.TBPercentual = new System.Windows.Forms.TextBox();
            this.BTRUN = new System.Windows.Forms.Button();
            this.BTCancelar = new System.Windows.Forms.Button();
            this.BTP1 = new System.Windows.Forms.Button();
            this.BTP2 = new System.Windows.Forms.Button();
            this.BTP3 = new System.Windows.Forms.Button();
            this.BTP4 = new System.Windows.Forms.Button();
            this.BTP5 = new System.Windows.Forms.Button();
            this.BTP6 = new System.Windows.Forms.Button();
            this.BTP7 = new System.Windows.Forms.Button();
            this.BTClearAll = new System.Windows.Forms.Button();
            this.TBThreads = new System.Windows.Forms.TextBox();
            this.BTClearBoard = new System.Windows.Forms.Button();
            this.BTClearHero = new System.Windows.Forms.Button();
            this.TBHero = new System.Windows.Forms.TextBox();
            this.TBBoard = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBAR)).BeginInit();
            this.SuspendLayout();
            // 
            // TBOutput
            // 
            this.TBOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TBOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TBOutput.Location = new System.Drawing.Point(0, 634);
            this.TBOutput.Multiline = true;
            this.TBOutput.Name = "TBOutput";
            this.TBOutput.Size = new System.Drawing.Size(1564, 157);
            this.TBOutput.TabIndex = 0;
            // 
            // BTAll
            // 
            this.BTAll.Location = new System.Drawing.Point(607, 77);
            this.BTAll.Name = "BTAll";
            this.BTAll.Size = new System.Drawing.Size(123, 31);
            this.BTAll.TabIndex = 1;
            this.BTAll.Text = "All";
            this.BTAll.UseVisualStyleBackColor = true;
            this.BTAll.Click += new System.EventHandler(this.BTAll_Click);
            // 
            // BTSuited
            // 
            this.BTSuited.Location = new System.Drawing.Point(607, 114);
            this.BTSuited.Name = "BTSuited";
            this.BTSuited.Size = new System.Drawing.Size(123, 31);
            this.BTSuited.TabIndex = 2;
            this.BTSuited.Text = "Any Suited";
            this.BTSuited.UseVisualStyleBackColor = true;
            this.BTSuited.Click += new System.EventHandler(this.BTSuited_Click);
            // 
            // BTPair
            // 
            this.BTPair.Location = new System.Drawing.Point(607, 188);
            this.BTPair.Name = "BTPair";
            this.BTPair.Size = new System.Drawing.Size(123, 31);
            this.BTPair.TabIndex = 4;
            this.BTPair.Text = "Any Pair";
            this.BTPair.UseVisualStyleBackColor = true;
            this.BTPair.Click += new System.EventHandler(this.BTPair_Click);
            // 
            // BTBroadway
            // 
            this.BTBroadway.Location = new System.Drawing.Point(607, 151);
            this.BTBroadway.Name = "BTBroadway";
            this.BTBroadway.Size = new System.Drawing.Size(123, 31);
            this.BTBroadway.TabIndex = 3;
            this.BTBroadway.Text = "Any Broadway";
            this.BTBroadway.UseVisualStyleBackColor = true;
            this.BTBroadway.Click += new System.EventHandler(this.BTBroadway_Click);
            // 
            // BTClear
            // 
            this.BTClear.Location = new System.Drawing.Point(607, 225);
            this.BTClear.Name = "BTClear";
            this.BTClear.Size = new System.Drawing.Size(123, 31);
            this.BTClear.TabIndex = 5;
            this.BTClear.Text = "Clear";
            this.BTClear.UseVisualStyleBackColor = true;
            this.BTClear.Click += new System.EventHandler(this.BTClear_Click);
            // 
            // TrackBAR
            // 
            this.TrackBAR.BackColor = System.Drawing.SystemColors.ControlLight;
            this.TrackBAR.LargeChange = 1;
            this.TrackBAR.Location = new System.Drawing.Point(102, 549);
            this.TrackBAR.Maximum = 169;
            this.TrackBAR.Name = "TrackBAR";
            this.TrackBAR.Size = new System.Drawing.Size(574, 56);
            this.TrackBAR.TabIndex = 6;
            this.TrackBAR.Scroll += new System.EventHandler(this.TrackBAR1_Scroll);
            // 
            // TBPercentual
            // 
            this.TBPercentual.Location = new System.Drawing.Point(710, 549);
            this.TBPercentual.Name = "TBPercentual";
            this.TBPercentual.Size = new System.Drawing.Size(89, 22);
            this.TBPercentual.TabIndex = 7;
            // 
            // BTRUN
            // 
            this.BTRUN.Location = new System.Drawing.Point(607, 491);
            this.BTRUN.Name = "BTRUN";
            this.BTRUN.Size = new System.Drawing.Size(123, 31);
            this.BTRUN.TabIndex = 9;
            this.BTRUN.Text = "Run";
            this.BTRUN.UseVisualStyleBackColor = true;
            this.BTRUN.Click += new System.EventHandler(this.BTRUN_Click);
            // 
            // BTCancelar
            // 
            this.BTCancelar.Location = new System.Drawing.Point(506, 692);
            this.BTCancelar.Name = "BTCancelar";
            this.BTCancelar.Size = new System.Drawing.Size(123, 31);
            this.BTCancelar.TabIndex = 8;
            this.BTCancelar.Text = "Cancel";
            this.BTCancelar.UseVisualStyleBackColor = true;
            this.BTCancelar.Click += new System.EventHandler(this.BTCancelar_Click);
            // 
            // BTP1
            // 
            this.BTP1.Location = new System.Drawing.Point(60, 18);
            this.BTP1.Name = "BTP1";
            this.BTP1.Size = new System.Drawing.Size(74, 31);
            this.BTP1.TabIndex = 10;
            this.BTP1.Text = "Player1";
            this.BTP1.UseVisualStyleBackColor = true;
            this.BTP1.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP2
            // 
            this.BTP2.Location = new System.Drawing.Point(153, 18);
            this.BTP2.Name = "BTP2";
            this.BTP2.Size = new System.Drawing.Size(74, 31);
            this.BTP2.TabIndex = 11;
            this.BTP2.Text = "Player2";
            this.BTP2.UseVisualStyleBackColor = true;
            this.BTP2.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP3
            // 
            this.BTP3.Location = new System.Drawing.Point(246, 18);
            this.BTP3.Name = "BTP3";
            this.BTP3.Size = new System.Drawing.Size(74, 31);
            this.BTP3.TabIndex = 12;
            this.BTP3.Text = "Player3";
            this.BTP3.UseVisualStyleBackColor = true;
            this.BTP3.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP4
            // 
            this.BTP4.Location = new System.Drawing.Point(339, 18);
            this.BTP4.Name = "BTP4";
            this.BTP4.Size = new System.Drawing.Size(74, 31);
            this.BTP4.TabIndex = 13;
            this.BTP4.Text = "Player4";
            this.BTP4.UseVisualStyleBackColor = true;
            this.BTP4.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP5
            // 
            this.BTP5.Location = new System.Drawing.Point(432, 18);
            this.BTP5.Name = "BTP5";
            this.BTP5.Size = new System.Drawing.Size(74, 31);
            this.BTP5.TabIndex = 14;
            this.BTP5.Text = "Player5";
            this.BTP5.UseVisualStyleBackColor = true;
            this.BTP5.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP6
            // 
            this.BTP6.Location = new System.Drawing.Point(525, 18);
            this.BTP6.Name = "BTP6";
            this.BTP6.Size = new System.Drawing.Size(74, 31);
            this.BTP6.TabIndex = 15;
            this.BTP6.Text = "Player6";
            this.BTP6.UseVisualStyleBackColor = true;
            this.BTP6.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTP7
            // 
            this.BTP7.Location = new System.Drawing.Point(618, 18);
            this.BTP7.Name = "BTP7";
            this.BTP7.Size = new System.Drawing.Size(74, 31);
            this.BTP7.TabIndex = 16;
            this.BTP7.Text = "Player7";
            this.BTP7.UseVisualStyleBackColor = true;
            this.BTP7.Click += new System.EventHandler(this.BTPlayer_Click);
            // 
            // BTClearAll
            // 
            this.BTClearAll.Location = new System.Drawing.Point(607, 262);
            this.BTClearAll.Name = "BTClearAll";
            this.BTClearAll.Size = new System.Drawing.Size(123, 31);
            this.BTClearAll.TabIndex = 17;
            this.BTClearAll.Text = "Clear All";
            this.BTClearAll.UseVisualStyleBackColor = true;
            this.BTClearAll.Click += new System.EventHandler(this.BTClearAll_Click);
            // 
            // TBThreads
            // 
            this.TBThreads.Location = new System.Drawing.Point(710, 583);
            this.TBThreads.Name = "TBThreads";
            this.TBThreads.Size = new System.Drawing.Size(89, 22);
            this.TBThreads.TabIndex = 18;
            // 
            // BTClearBoard
            // 
            this.BTClearBoard.Location = new System.Drawing.Point(607, 411);
            this.BTClearBoard.Name = "BTClearBoard";
            this.BTClearBoard.Size = new System.Drawing.Size(123, 31);
            this.BTClearBoard.TabIndex = 19;
            this.BTClearBoard.Text = "Clear Board";
            this.BTClearBoard.UseVisualStyleBackColor = true;
            this.BTClearBoard.Click += new System.EventHandler(this.BTClearBoard_Click);
            // 
            // BTClearHero
            // 
            this.BTClearHero.Location = new System.Drawing.Point(607, 374);
            this.BTClearHero.Name = "BTClearHero";
            this.BTClearHero.Size = new System.Drawing.Size(123, 31);
            this.BTClearHero.TabIndex = 20;
            this.BTClearHero.Text = "Clear Hero";
            this.BTClearHero.UseVisualStyleBackColor = true;
            this.BTClearHero.Click += new System.EventHandler(this.BTClearHero_Click);
            // 
            // TBHero
            // 
            this.TBHero.BackColor = System.Drawing.SystemColors.Window;
            this.TBHero.Location = new System.Drawing.Point(1033, 583);
            this.TBHero.Name = "TBHero";
            this.TBHero.Size = new System.Drawing.Size(89, 22);
            this.TBHero.TabIndex = 21;
            this.TBHero.TextChanged += new System.EventHandler(this.TBHero_TextChanged);
            // 
            // TBBoard
            // 
            this.TBBoard.Location = new System.Drawing.Point(1282, 583);
            this.TBBoard.Name = "TBBoard";
            this.TBBoard.Size = new System.Drawing.Size(89, 22);
            this.TBBoard.TabIndex = 22;
            this.TBBoard.TextChanged += new System.EventHandler(this.TBBoard_TextChanged);
            // 
            // Distribution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1564, 791);
            this.Controls.Add(this.TBBoard);
            this.Controls.Add(this.TBHero);
            this.Controls.Add(this.BTClearHero);
            this.Controls.Add(this.BTClearBoard);
            this.Controls.Add(this.TBThreads);
            this.Controls.Add(this.BTClearAll);
            this.Controls.Add(this.BTP7);
            this.Controls.Add(this.BTP6);
            this.Controls.Add(this.BTP5);
            this.Controls.Add(this.BTP4);
            this.Controls.Add(this.BTP3);
            this.Controls.Add(this.BTP2);
            this.Controls.Add(this.BTP1);
            this.Controls.Add(this.BTRUN);
            this.Controls.Add(this.BTCancelar);
            this.Controls.Add(this.TBPercentual);
            this.Controls.Add(this.TrackBAR);
            this.Controls.Add(this.BTClear);
            this.Controls.Add(this.BTPair);
            this.Controls.Add(this.BTBroadway);
            this.Controls.Add(this.BTSuited);
            this.Controls.Add(this.BTAll);
            this.Controls.Add(this.TBOutput);
            this.Name = "Distribution";
            this.Text = "Distribution";
            this.Resize += new System.EventHandler(this.Distribution_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.TrackBAR)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TBOutput;
        private System.Windows.Forms.Button BTAll;
        private System.Windows.Forms.Button BTSuited;
        private System.Windows.Forms.Button BTPair;
        private System.Windows.Forms.Button BTBroadway;
        private System.Windows.Forms.Button BTClear;
        private System.Windows.Forms.TrackBar TrackBAR;
        private System.Windows.Forms.TextBox TBPercentual;
        private System.Windows.Forms.Button BTRUN;
        private System.Windows.Forms.Button BTCancelar;
        private System.Windows.Forms.Button BTP1;
        private System.Windows.Forms.Button BTP2;
        private System.Windows.Forms.Button BTP3;
        private System.Windows.Forms.Button BTP4;
        private System.Windows.Forms.Button BTP5;
        private System.Windows.Forms.Button BTP6;
        private System.Windows.Forms.Button BTP7;
        private System.Windows.Forms.Button BTClearAll;
        private System.Windows.Forms.TextBox TBThreads;
        private System.Windows.Forms.Button BTClearBoard;
        private System.Windows.Forms.Button BTClearHero;
        private System.Windows.Forms.TextBox TBHero;
        private System.Windows.Forms.TextBox TBBoard;
    }
}