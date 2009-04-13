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
        public delegate void OptionChangedType(OptionChangedValue[] options);
        public OptionChangedType OptionChanged;

        private void OnOptionChanged(OptionChangedValue[] options)
        {
            if (this.OptionChanged != null)
                this.OptionChanged(options);
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

        public void ShowData(Settings.TemplateINIBlock[] blocks, int templateIndex)
        {
            //display options
            List<CustomPropertyCollection> propertyObjects = new List<CustomPropertyCollection>();
            foreach (Settings.TemplateINIBlock block in blocks)
            {
                CustomPropertyCollection properties = new CustomPropertyCollection();
                for (int i = 0; i < block.Options.Count; i++)
                {
                    string category = "";
                    string comment = "";
                    int categoryIndex = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Category;
                    if (Helper.Template.Data.Language != null && Helper.Template.Data.Language.Categories.Count > categoryIndex + 1)
                        category = Helper.Template.Data.Language.Categories[categoryIndex].Value;

                    properties.Add(new CustomPropertyItem(block.Options[i].Name, block.Options[i].Value, i, category, comment, false));
                }
                propertyObjects.Add(properties);
            }

            propertyGrid.SelectedObjects = propertyObjects.ToArray();
        }

        private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertyGrid.HelpVisible = descriptionToolStripMenuItem.Checked;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            List<OptionChangedValue> optionsChanged = new List<OptionChangedValue>();
            for (int i = 0; i < propertyGrid.SelectedObjects.Length; i++)
            {
                CustomPropertyCollection properties = (CustomPropertyCollection)propertyGrid.SelectedObjects[i];
                int propertyIndex = properties.IndexOf(e.ChangedItem.Label);
                if (propertyIndex != -1)
                    optionsChanged.Add(new OptionChangedValue(i, (int)properties[propertyIndex].Tag, properties[propertyIndex].Value));
            }

            OnOptionChanged(optionsChanged.ToArray());
        }
    }

    public class OptionChangedValue
    {
        public int PropertyIndex;
        public int OptionIndex;
        public object NewValue;

        public OptionChangedValue(int propertyIndex, int optionIndex, object newValue)
        {
            PropertyIndex = propertyIndex;
            OptionIndex = optionIndex;
            NewValue = newValue;
        }
    }
}
