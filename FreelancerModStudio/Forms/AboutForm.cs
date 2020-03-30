namespace FreelancerModStudio
{
    using System;
    using System.Windows.Forms;

    using FreelancerModStudio.Properties;

    internal partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            this.InitializeComponent();

            this.Text = string.Format(Strings.AboutText, Helper.Assembly.Name);
            this.lblProductName.Text = Helper.Assembly.Name;
            this.lblVersion.Text = string.Format(Strings.AboutVersion, Helper.Assembly.Version);
            this.lblCopyright.Text = Helper.Assembly.Copyright;
            this.lblCompanyName.Text = Helper.Assembly.Company;
        }
    }
}
