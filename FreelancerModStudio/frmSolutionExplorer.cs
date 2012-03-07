using System.Windows.Forms;
using FreelancerModStudio.Data;

namespace FreelancerModStudio
{
    public partial class frmSolutionExplorer : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmSolutionExplorer()
        {
            InitializeComponent();
            Icon = Properties.Resources.SolutionExplorer;

            var imageList = new ImageList { ColorDepth = ColorDepth.Depth24Bit };
            imageList.Images.Add("mod", Properties.Resources.Mod);
            imageList.Images.Add("openfolder", Properties.Resources.OpenFolder);
            imageList.Images.Add("folder", Properties.Resources.Folder);
            imageList.Images.Add("hiddenfolder", Properties.Resources.HiddenFolder);
            imageList.Images.Add("hiddenfolderopen", Properties.Resources.HiddenFolderOpen);
            imageList.Images.Add("ini", Properties.Resources.Fileini);
            imageList.Images.Add("txt", Properties.Resources.Filetxt);
            imageList.Images.Add("dll", Properties.Resources.Filedll);
            imageList.Images.Add("wav", Properties.Resources.Filewav);
            imageList.Images.Add("file", Properties.Resources.File);
            imageList.Images.Add("hiddenfile", Properties.Resources.HiddenFile);

            treeView1.ImageList = imageList;
            //this.BuildList();

            RefreshSettings();
        }

        public void RefreshSettings()
        {
            TabText = Properties.Strings.SolutionExplorerText;
        }

        //void AddNode(Settings.Template.File file)
        //{
        //    //file.Path
        //}

        //void BuildList ()
        //{
        //    foreach (Settings.Template.File file in Helper.Template.Data.Files)
        //    {
        //        this.AddNode(file);
        //    }
        //}

        public void ShowProject(Mod mod)
        {
            var treeNode = new TreeNode(mod.Data.About.Name) { Name = "mod", ImageKey = "mod" };
            treeNode.Nodes.Add("data", "data", "hiddenfolder");
            treeNode.Nodes.Add("exe", "exe", "hiddenfolder");
            treeNode.Nodes.Add("dlls", "dlls", "hiddenfolder");
            treeView1.Nodes.Add(treeNode);
            treeView1.ExpandAll();
        }
    }
}