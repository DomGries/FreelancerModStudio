namespace FreelancerModStudio
{
    using System.Windows.Forms;

    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = Helper.Settings.Data.Data.General;
            propertyGrid.ExpandAllGridItems();
        }
    }
}
