using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmFileType : Form
    {
        public int FileTypeIndex { get; set; }

        public frmFileType(string filePath)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(filePath))
            {
                pathTextBox.Text = filePath;
                pathTextBox.SelectionStart = pathTextBox.Text.Length - 1;
                pathTextBox.ScrollToCaret();
            }
            else
            {
                pathLabel.Visible = false;
                pathTextBox.Visible = false;
                Height -= 26;
            }

            //set available file types
            for (int i = 0; i < Helper.Template.Data.Files.Count; ++i)
            {
                fileTypeComboBox.Items.Add(new FileTypeItem
                    {
                        Name = Helper.Template.Data.Files[i].Name,
                        Index = i
                    });
            }

            fileTypeComboBox.Sorted = true;

            if (fileTypeComboBox.Items.Count > Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType)
            {
                fileTypeComboBox.SelectedIndex = Helper.Settings.Data.Data.Forms.ChooseFileType.SelectedFileType;
            }
            else
            {
                fileTypeComboBox.SelectedIndex = 0;
            }
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

        public override string ToString()
        {
            return Name;
        }
    }
}
