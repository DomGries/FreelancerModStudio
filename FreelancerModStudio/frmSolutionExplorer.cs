using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreelancerModStudio.Data;

namespace FreelancerModStudio
{
    public partial class frmSolutionExplorer : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmSolutionExplorer()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.SolutionExplorer;

            ImageList imageList = new ImageList();
            imageList.ColorDepth = ColorDepth.Depth24Bit;
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
            this.TabText = Properties.Strings.SolutionExplorerText;
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
            TreeNode treeNode = new TreeNode(mod.Data.About.Name);
            treeNode.Name = "mod";
            treeNode.ImageKey = "mod";
            treeNode.Nodes.Add("data", "data", "hiddenfolder");
            treeNode.Nodes.Add("exe", "exe", "hiddenfolder");
            treeNode.Nodes.Add("dlls", "dlls", "hiddenfolder");
            treeView1.Nodes.Add(treeNode);
            treeView1.ExpandAll();
        }
    }
}