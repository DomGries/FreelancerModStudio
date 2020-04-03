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

            this.Text = string.Format(Strings.AboutText, FLUtils.AssemblyUtils.Name(true));
            this.lblProductName.Text = FLUtils.AssemblyUtils.Name(true);
            this.lblVersion.Text = string.Format(Strings.AboutVersion, FLUtils.AssemblyUtils.Version(true));
            this.lblCopyright.Text = FLUtils.AssemblyUtils.Copyright(true);
            this.lblCompanyName.Text = FLUtils.AssemblyUtils.Company(true);
        }
    }
}
