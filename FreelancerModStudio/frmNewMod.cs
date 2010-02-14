using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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

        bool ValidHomepage(string uri)
        {
            if (uri.Trim().Length == 0)
                return true;

            try
            {
                new Uri(uri.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        void Path_TextChanged(object sender, EventArgs e)
        {
            this.btnOK.Enabled = (this.ValidName(this.txtName.Text) && this.ValidPathRoot(this.txtSaveLocation.Text));
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (this.ValidChars(this.txtName.Text))
            {
                if (this.ValidChars(this.txtSaveLocation.Text))
                {
                    if (this.ValidHomepage(this.txtHomepage.Text))
                        this.DialogResult = DialogResult.OK;
                    else
                        MessageBox.Show(Properties.Strings.ModInvalidHomepage, Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                    MessageBox.Show(Properties.Strings.ModInvalidPathChars, Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
                MessageBox.Show(Properties.Strings.ModInvalidNameChars, Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        void btnBrowse_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.SelectedPath = this.txtSaveLocation.Text;

            if (this.folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                this.txtSaveLocation.Text = this.folderBrowserDialog1.SelectedPath;
        }
    }
}