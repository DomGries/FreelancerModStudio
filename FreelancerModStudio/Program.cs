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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Helper.Settings.Load();
            //Helper.Template.Load();

            Application.Run(new frmMain());

            Helper.Settings.Save();
        }
    }
}