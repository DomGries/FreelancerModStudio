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
            //DevTest.CreateTemplate(@""); return;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            // catch real errors globally
            try
            {
#endif
                // initialize program
                Helper.Program.Start();
#if !DEBUG
            }
            catch (Exception ex)
            {
                string text = "A critical error occured!" + Environment.NewLine + Environment.NewLine + "Do you want to post an issue report?";
                string details = Helper.Exceptions.Get(ex) + Environment.NewLine + ex.StackTrace;
                if (MessageBox.Show(text + Environment.NewLine + Environment.NewLine + details, Helper.Assembly.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    Process.Start("http://code.google.com/p/freelancermodstudio/issues");
                }
            }
#endif
        }
    }
}
