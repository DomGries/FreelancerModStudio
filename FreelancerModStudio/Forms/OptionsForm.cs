namespace FreelancerModStudio
{
    using System.Windows.Forms;

    using FreelancerModStudio.SystemDesigner;

    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            this.InitializeComponent();
            this.propertyGrid.SelectedObject = Helper.Settings.Data.Data.General;
            this.propertyGrid.ExpandAllGridItems();
        }

        private void OptionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Helper.Settings.Save();
            SharedGeometries.LoadColors(Helper.Settings.Data.Data.General.ColorBox);
        }
    }
}
