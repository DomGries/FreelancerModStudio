using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmOptions : Form
    {
        public frmOptions()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = Helper.Settings.Data.Data.General;
            propertyGrid.ExpandAllGridItems();
        }
    }
}
