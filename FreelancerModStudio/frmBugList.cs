using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace FreelancerModStudio
{
    public partial class frmBugList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmBugList()
        {
			InitializeComponent();
			this.Icon = Properties.Resources.Error;
        }
    }
}