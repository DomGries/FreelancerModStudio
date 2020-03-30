namespace FreelancerModStudio
{
    using System.Windows.Forms;

    public partial class FrmFileType : Form
    {
        public int FileTypeIndex { get; set; }

        public FrmFileType(string filePath)
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(filePath))
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

            // set available file types
            for (int i = 0; i < Helper.Template.Data.Files.Count; ++i)
            {
                this.fileTypeComboBox.Items.Add(new FileTypeItem
                    {
                        Name = Helper.Template.Data.Files[i].Name,
                        Index = i
                    });
            }

            this.fileTypeComboBox.Sorted = true;

            if (this.fileTypeComboBox.Items.Count > Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType)
            {
                this.fileTypeComboBox.SelectedIndex = Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType;
            }
            else
            {
                this.fileTypeComboBox.SelectedIndex = 0;
            }
        }

        private void FrmFileTypeFormClosed(object sender, FormClosedEventArgs e)
        {
            this.FileTypeIndex = ((FileTypeItem)this.fileTypeComboBox.SelectedItem).Index;
            Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType = this.fileTypeComboBox.SelectedIndex;
        }
    }

    public class FileTypeItem
    {
        public string Name;
        public int Index;

        public override string ToString()
        {
            return this.Name;
        }
    }
}
