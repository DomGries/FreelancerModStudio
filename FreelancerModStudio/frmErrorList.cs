using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace FreelancerModStudio
{
    public partial class frmErrorList : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmErrorList()
        {
			InitializeComponent();
			this.Icon = Properties.Resources.Error;
        }

        public void RefreshSettings()
        {
            this.TabText = Properties.Strings.ErrorListText;
        }
    }
}