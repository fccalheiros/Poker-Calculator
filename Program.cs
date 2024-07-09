using System;
using System.Windows.Forms;

namespace PokerCalculator
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        /// 
        static public Distribution FMDistribution;
        static public FMTest FMMain;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FMDistribution = new Distribution();
            FMDistribution.MainWindow = true;

            FMMain = new FMTest();

            Application.Run(FMDistribution);
        }
    }
}
