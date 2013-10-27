using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreelancerModStudio.Controls;
using FreelancerModStudio.Data;
using FreelancerModStudio.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmProperties : DockContent, IContentForm
    {
        public delegate void OptionsChangedType(PropertyBlock[] blocks);

        public OptionsChangedType OptionsChanged;

        public delegate void ContentChangedType(IContentForm content);

        public ContentChangedType ContentChanged;

        void OnOptionsChanged(PropertyBlock[] blocks)
        {
            if (OptionsChanged != null)
            {
                OptionsChanged(blocks);
            }
        }

        void OnContentChanged(IContentForm content)
        {
            if (ContentChanged != null)
            {
                ContentChanged(content);
            }
        }

        public frmProperties()
        {
            InitializeComponent();
            Icon = Resources.Properties;

            RefreshSettings();
        }

        public void RefreshSettings()
        {
            TabText = Strings.PropertiesText;

            propertyGrid.PropertySort = Helper.Settings.Data.Data.General.PropertiesSortType;
            propertyGrid.HelpVisible = Helper.Settings.Data.Data.General.PropertiesShowHelp;
        }

        public void ClearData()
        {
            if (propertyGrid.SelectedObject != null)
            {
                propertyGrid.SelectedObject = null;
            }
        }

        public void ShowData(List<TableBlock> blocks, int templateIndex)
        {
            if (blocks == null)
            {
                propertyGrid.SelectedObjects = null;
                return;
            }

            PropertyBlock[] propertyBlocks = new PropertyBlock[blocks.Count];
            for (int i = 0; i < blocks.Count; i++)
            {
                propertyBlocks[i] = new PropertyBlock(blocks[i].Block, Helper.Template.Data.Files[templateIndex].Blocks.Values[blocks[i].Block.TemplateIndex]);
            }

            propertyGrid.SelectedObjects = propertyBlocks;
            propertyGrid.ExpandAllGridItems();

            //ensure visibility of selected grid item
            propertyGrid.SelectedGridItem.Select();
        }

        void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.HelpVisible = descriptionToolStripMenuItem.Checked;
        }

        void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Value != e.OldValue)
            {
                OnOptionsChanged((PropertyBlock[])propertyGrid.SelectedObjects);
            }
        }

        void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            if (DockHandler.IsActivated)
            {
                OnContentChanged(this);
            }
        }

        public bool CanAdd()
        {
            if (propertyGrid.SelectedGridItem != null)
            {
                return propertyGrid.SelectedGridItem.Value is PropertySubOptions || (propertyGrid.SelectedGridItem.Parent != null && propertyGrid.SelectedGridItem.Parent.Value is PropertySubOptions);
            }

            return false;
        }

        public bool CanCopy()
        {
            return false;
        }

        public bool CanCut()
        {
            return false;
        }

        public bool CanPaste()
        {
            return false;
        }

        public bool CanDelete()
        {
            return false;
        }

        public bool CanSelectAll()
        {
            return false;
        }

        public void Delete()
        {
            PropertyOptionDescriptor propertyOptionDescriptor = propertyGrid.SelectedGridItem.PropertyDescriptor as PropertyOptionDescriptor;
            if (propertyOptionDescriptor != null)
            {
                PropertyOption option = propertyOptionDescriptor.PropertyOption;
                object oldValue = option.Value;
                option.Value = string.Empty;

                propertyGrid_PropertyValueChanged(propertyGrid, new PropertyValueChangedEventArgs(propertyGrid.SelectedGridItem, oldValue));
            }
        }

        public void SelectAll()
        {
        }

        public ToolStripDropDown MultipleAddDropDown()
        {
            return null;
        }

        public void Copy()
        {
        }

        public void Paste()
        {
        }

        public void Cut()
        {
        }

        public bool UseDocument()
        {
            return false;
        }
    }
}
