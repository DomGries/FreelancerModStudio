using System;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();

            Text = String.Format(Properties.Strings.AboutText, Helper.Assembly.Title);
            lblProductName.Text = Helper.Assembly.Product;
            lblVersion.Text = String.Format(Properties.Strings.AboutVersion, Helper.Assembly.Version);
            lblCopyright.Text = Helper.Assembly.Copyright;
            lblCompanyName.Text = Helper.Assembly.Company;
            txtDescription.Text = Helper.Assembly.Description;
        }
    }
}
