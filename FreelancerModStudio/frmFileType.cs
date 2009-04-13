using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmFileType : Form
    {
        public int FileTypeIndex { get; private set; }

        public frmFileType(string filePath)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;

            this.textBox1.Text = filePath;
            this.textBox1.SelectionStart = this.textBox1.Text.Length - 1;
            this.textBox1.ScrollToCaret();

            //set aviable file types
            foreach (Settings.Template.File file in Helper.Template.Data.Files)
                fileTypeComboBox.Items.Add(file.Name);

            if (fileTypeComboBox.Items.Count > Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType)
                fileTypeComboBox.SelectedIndex = Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType;
        }

        private void frmFileType_FormClosed(object sender, FormClosedEventArgs e)
        {
            FileTypeIndex = fileTypeComboBox.SelectedIndex;
            Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType = FileTypeIndex;
        }
    }
}
