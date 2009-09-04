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

#if !DEBUG
            //global error catch
            try
            {
#endif
                //initialize program
                Helper.Program.Start();
#if !DEBUG
            }
            catch (Exception ex)
            {
                string text = "A critical error occured!" + Environment.NewLine + Environment.NewLine + "Please post an error report on the homepage (link inside Help menu).";
                Helper.Exceptions.Show(new Exception(text, ex));
            }
#endif
        }
    }
}