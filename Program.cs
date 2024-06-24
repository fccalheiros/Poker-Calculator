using System;
using System.Windows.Forms;

namespace Confidencial
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        /// 
        static public Distribution FMDistribution;
        static public FMPrincipal FMMain;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FMDistribution = new Distribution();
            FMDistribution.MainWindow = true;

            FMMain = new FMPrincipal();

            Application.Run(FMDistribution);
        }
    }
}
