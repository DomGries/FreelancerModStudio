using System;
using System.Windows.Forms;
using FreelancerModStudio.SystemPresenter;

namespace FreelancerModStudio
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            //DevTest.CreateTemplate(@""); return;
            UtfModel.LoadModel(@"D:\MyData\DWN\FLDD\DATA\SOLAR\GALLIA\ga_lane.cmp");
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
                    System.Diagnostics.Process.Start("http://code.google.com/p/freelancermodstudio/issues");
                }
            }
#endif
        }
    }
}
