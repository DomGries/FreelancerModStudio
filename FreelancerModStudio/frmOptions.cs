using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmOptions : Form
    {
        public frmOptions()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;
            this.propertyGrid.SelectedObject = Helper.Settings.Data.Data.General;
            this.propertyGrid.ExpandAllGridItems();
        }
    }
}
