using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmProperties : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public frmProperties()
        {
			InitializeComponent();
			this.Icon = Properties.Resources.Properties;
        }
    }
}