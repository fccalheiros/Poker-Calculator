using System;
using System.Windows.Forms;

namespace PokerCalculator
{
    static class Program
    {
        /// <summary>
        /// Main application entry point.
        /// </summary>
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
