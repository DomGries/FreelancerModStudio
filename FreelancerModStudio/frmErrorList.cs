namespace FreelancerModStudio
{
    public partial class frmErrorList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmErrorList()
        {
            InitializeComponent();
            Icon = Properties.Resources.Error;
        }

        public void RefreshSettings()
        {
            TabText = Properties.Strings.ErrorListText;
        }
    }
}