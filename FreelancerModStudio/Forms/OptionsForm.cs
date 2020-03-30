namespace FreelancerModStudio
{
    using System.Windows.Forms;

    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            this.InitializeComponent();
            this.propertyGrid.SelectedObject = Helper.Settings.Data.Data.General;
            this.propertyGrid.ExpandAllGridItems();
        }
    }
}
