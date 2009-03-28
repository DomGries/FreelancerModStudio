using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmSolutionExplorer : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmSolutionExplorer()
        {
			InitializeComponent();
			this.Icon = Properties.Resources.SolutionExplorer;

			this.BuildList();
        }

		private void AddNode(Settings.Template.File file)
		{
			//file.Path
		}

		private void BuildList ()
		{
            foreach (Settings.Template.File file in Helper.Template.Data.Data.Files)
            {
                this.AddNode(file);
            }
		}
    }
}