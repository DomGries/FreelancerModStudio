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
        public int FileTypeIndex { get; set; }

        public frmFileType(string filePath)
        {
            InitializeComponent();

            if (filePath != null)
            {
                this.pathTextBox.Text = filePath;
                this.pathTextBox.SelectionStart = this.pathTextBox.Text.Length - 1;
                this.pathTextBox.ScrollToCaret();
            }
            else
            {
                this.pathLabel.Visible = false;
                this.pathTextBox.Visible = false;
                this.Height -= 26;
            }

            //set aviable file types
            for (int i = 0; i < Helper.Template.Data.Files.Count; i++)
                fileTypeComboBox.Items.Add(new FileTypeItem(Helper.Template.Data.Files[i].Name, i));

            fileTypeComboBox.Sorted = true;

            if (fileTypeComboBox.Items.Count > Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType)
                fileTypeComboBox.SelectedIndex = Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType;
            else
                fileTypeComboBox.SelectedIndex = 0;
        }

        void frmFileType_FormClosed(object sender, FormClosedEventArgs e)
        {
            FileTypeIndex = ((FileTypeItem)fileTypeComboBox.SelectedItem).Index;
            Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType = fileTypeComboBox.SelectedIndex;
        }
    }

    public class FileTypeItem
    {
        public string Name;
        public int Index;

        public FileTypeItem(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
