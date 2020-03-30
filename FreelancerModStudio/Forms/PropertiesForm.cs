namespace FreelancerModStudio
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using FreelancerModStudio.Controls;
    using FreelancerModStudio.Data;
    using FreelancerModStudio.Properties;

    using WeifenLuo.WinFormsUI.Docking;

    public partial class PropertiesForm : DockContent, IContentForm
    {
        public delegate void OptionsChangedType(PropertyBlock[] blocks);

        public OptionsChangedType OptionsChanged;

        private void OnOptionsChanged(PropertyBlock[] blocks) => this.OptionsChanged?.Invoke(blocks);

        public PropertiesForm()
        {
            this.InitializeComponent();
            this.Icon = Resources.Properties;

            this.RefreshSettings();
        }

        public void RefreshSettings()
        {
            this.TabText = Strings.PropertiesText;

            this.propertyGrid.PropertySort = Helper.Settings.Data.Data.General.PropertiesSortType;
            this.propertyGrid.HelpVisible = Helper.Settings.Data.Data.General.PropertiesShowHelp;
        }

        public void ClearData()
        {
            if (this.propertyGrid.SelectedObject != null)
            {
                this.propertyGrid.SelectedObject = null;
            }
        }

        public void ShowData(List<TableBlock> blocks, int templateIndex)
        {
            if (blocks == null)
            {
                this.propertyGrid.SelectedObjects = null;
                return;
            }

            PropertyBlock[] propertyBlocks = new PropertyBlock[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                propertyBlocks[i] = new PropertyBlock(blocks[i].Block, Helper.Template.Data.Files[templateIndex].Blocks.Values[blocks[i].Block.TemplateIndex]);
            }

            this.propertyGrid.SelectedObjects = propertyBlocks;
            this.propertyGrid.ExpandAllGridItems();

            // ensure visibility of selected grid item
            this.propertyGrid.SelectedGridItem.Select();
        }

        private void DescriptionToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.propertyGrid.HelpVisible = this.descriptionToolStripMenuItem.Checked;
        }

        private void PropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Value != e.OldValue)
            {
                this.OnOptionsChanged((PropertyBlock[])this.propertyGrid.SelectedObjects);
            }
        }

        #region IContentForm Member

        public bool CanDelete()
        {
            return false;
        }

        #endregion
    }
}
