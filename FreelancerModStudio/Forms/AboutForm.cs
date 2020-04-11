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

            this.Text = string.Format(Strings.AboutText, FLUtils.AssemblyUtils.Name);
            this.lblProductName.Text = FLUtils.AssemblyUtils.Name;
            this.lblVersion.Text = string.Format(Strings.AboutVersion, FLUtils.AssemblyUtils.Version);
            this.lblCopyright.Text = FLUtils.AssemblyUtils.Copyright;
            this.lblCompanyName.Text = FLUtils.AssemblyUtils.Company;
        }
    }
}
