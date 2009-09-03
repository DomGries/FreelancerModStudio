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
        public delegate void OptionsChangedType(PropertyBlock[] blocks);
        public OptionsChangedType OptionsChanged;

        private void OnOptionsChanged(PropertyBlock[] blocks)
        {
            if (this.OptionsChanged != null)
                this.OptionsChanged(blocks);
        }

        public frmProperties()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.Properties;
        }

        public void RefreshSettings()
        {
            this.TabText = Properties.Strings.PropertiesText;
        }

        public void ClearData()
        {
            if (propertyGrid.SelectedObject != null)
                propertyGrid.SelectedObject = null;
        }

        public void ShowData(Settings.EditorINIBlock[] blocks, int templateIndex)
        {
            List<PropertyBlock> propertyBlocks = new List<PropertyBlock>();

            //loop each selected block
            foreach (Settings.EditorINIBlock block in blocks)
                propertyBlocks.Add(new PropertyBlock(block, Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex]));

            propertyGrid.SelectedObjects = propertyBlocks.ToArray();
            propertyGrid.ExpandAllGridItems();
        }

        private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.HelpVisible = descriptionToolStripMenuItem.Checked;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Value != e.OldValue)
                OnOptionsChanged((PropertyBlock[])propertyGrid.SelectedObjects);
        }
    }
}
