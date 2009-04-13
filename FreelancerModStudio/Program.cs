using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //DevTest.CreateTemplate(@"D:\BrigitteDaten\Eigene Musik\1.5b15-sdk_20050627"); return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //initialize program
            Helper.Program.Start();
        }
    }
}