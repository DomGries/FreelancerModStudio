using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            Helper.Program.Start();
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            if (!Debugger.IsAttached)
            {
                string text = "A critical error occured!" + Environment.NewLine + Environment.NewLine + "Do you want to post an issue report?";
                string details = Helper.Exceptions.Get((Exception)ex.ExceptionObject) + Environment.NewLine + ex.ExceptionObject;
                if (MessageBox.Show(text + Environment.NewLine + Environment.NewLine + details, Helper.Assembly.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start("https://github.com/AftermathFreelancer/FLModStudio/issues");
                }
                Environment.Exit(1);
            }

            else
            {
                Debugger.Break();
            }
        }
    }
}
