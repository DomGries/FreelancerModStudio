using System;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmNewMod : Form
    {
        public frmNewMod()
        {
            InitializeComponent();
        }

        bool ValidPathRoot(string path)
        {
            return (path.Trim().Length >= 2 && System.IO.Directory.Exists(System.IO.Path.GetPathRoot(path.Trim().Substring(0, 2))));
        }

        bool ValidChars(string path)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidPathChars())
                if (path.Contains(invalidChar.ToString()))
                    return false;

            return true;
        }

        bool ValidName(string name)
        {
            return name.Trim().Length > 0;
        }

        void Path_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (ValidName(txtName.Text) && ValidPathRoot(txtSaveLocation.Text));
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidChars(txtName.Text))
            {
                if (ValidChars(txtSaveLocation.Text))
                    DialogResult = DialogResult.OK;
                else
                    MessageBox.Show(Properties.Strings.ModInvalidPathChars, Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show(Properties.Strings.ModInvalidNameChars, Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtSaveLocation.Text;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                txtSaveLocation.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}