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
        public delegate void OptionsChangedType(OptionChangedValue[] options);
        public OptionsChangedType OptionsChanged;

        private void OnOptionsChanged(OptionChangedValue[] options)
        {
            if (this.OptionsChanged != null)
                this.OptionsChanged(options);
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
                    if (Helper.Template.Data.Language != null && Helper.Template.Data.Language.Categories.Count > categoryIndex)
                        category = Helper.Template.Data.Language.Categories[categoryIndex].Value;

                    bool multiple = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Multiple;
                    string parent = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Parent;
                    int parentIndex = -1;
                    if (parent != null)
                        parentIndex = properties.IndexOf(parent);

                    string name = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Name;

                    CustomPropertyCollection subProperties = new CustomPropertyCollection();
                    for (int j = 0; j < block.Options[i].Values.Count; j++)
                        subProperties.Add(new CustomPropertyItem(block.Options[i].Name, block.Options[i].Values[j].Value, block.Options[i].Values[j].Value, new PropertyTag(i, j, block.Options[i].Values[j].ID), category, comment, false));

                    if (parentIndex != -1)
                    {
                        //add into parent
                        CustomPropertyCollection parentProperties = (CustomPropertyCollection)properties[parentIndex].Value;
                        for (int j = 0; j < parentProperties.Count; j++)
                        {
                            int parentPropertyID = ((PropertyTag)parentProperties[j].Tag).OptionID;
                            int nextParentPropertyID = Int32.MaxValue;
                            if (j < parentProperties.Count - 1)
                                nextParentPropertyID = ((PropertyTag)parentProperties[j + 1].Tag).OptionID;

                            CustomPropertyCollection newParentPropertySubValue = new CustomPropertyCollection();
                            for (int k = 0; k < subProperties.Count; k++)
                            {
                                int propertyID = ((PropertyTag)subProperties[k].Tag).OptionID;
                                if (propertyID > parentPropertyID && propertyID < nextParentPropertyID)
                                {
                                    if (j < parentProperties.Count - 1)
                                        newParentPropertySubValue.Add(subProperties[k]);
                                }
                            }

                            //add empty line
                            newParentPropertySubValue.Add(new CustomPropertyItem(name, "", "", new PropertyTag(i, 0, -1), category, comment, false));

                            CustomPropertyCollection newParentPropertyValue = new CustomPropertyCollection();
                            newParentPropertyValue.Add(parentProperties[j]);

                            if (multiple)
                                newParentPropertyValue.Add(new CustomPropertyItem(newParentPropertySubValue[0].Name, newParentPropertySubValue, newParentPropertySubValue, null, category, comment, false,
                                    new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                                    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));
                            else
                                newParentPropertyValue.Add(newParentPropertySubValue[0]);

                            CustomPropertyItem newParentProperty = new CustomPropertyItem(parentProperties[j].Name, newParentPropertyValue, newParentPropertyValue, null, parentProperties[j].Category, parentProperties[j].Description, parentProperties[j].ReadOnly,
                                new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                                new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor)));

                            parentProperties[j] = newParentProperty;
                        }
                    }
                    else
                    {
                        CustomPropertyItem property;
                        if (multiple)
                        {
                            //add empty line
                            subProperties.Add(new CustomPropertyItem(block.Options[i].Name, "", "", new PropertyTag(i, subProperties.Count, -1), category, comment, false));

                            property = new CustomPropertyItem(block.Options[i].Name, subProperties, subProperties, new PropertyTag(i, 0, -1), category, comment, false,
                                new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                                new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor)));
                        }
                        else
                        {
                            if (subProperties.Count > 0)
                                property = new CustomPropertyItem(block.Options[i].Name, subProperties[0].Value, subProperties[0].Value, new PropertyTag(i, 0, -1), category, comment, false);
                            else
                                property = new CustomPropertyItem(block.Options[i].Name, "", "", new PropertyTag(i, 0, -1), category, comment, false);
                        }
                        properties.Add(property);
                    }
                }
                propertyObjects.Add(properties);
            }

            propertyGrid.SelectedObjects = propertyObjects.ToArray();
            propertyGrid.ExpandAllGridItems();
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
                PropertyTag propertyTag = (PropertyTag)((CustomPropertyDescriptor)e.ChangedItem.PropertyDescriptor).PropertyItem.Tag;

                CustomPropertyItem property = ((CustomPropertyCollection)propertyGrid.SelectedObjects[i])[propertyTag.OptionIndex];

                //add new empty line if needed
                if (property.Value is CustomPropertyCollection)
                {
                    CustomPropertyCollection subProperties = (CustomPropertyCollection)property.Value;

                    if (e.ChangedItem.Value.ToString().Trim() != "" && propertyTag.OptionEntryIndex == subProperties.Count - 1)
                        subProperties.Add(new CustomPropertyItem(e.ChangedItem.Label, "", "", new PropertyTag(i, subProperties.Count, -1), subProperties[propertyTag.OptionEntryIndex].Category, subProperties[propertyTag.OptionEntryIndex].Description, false));
                    else if (e.ChangedItem.Value.ToString().Trim() == "" && propertyTag.OptionEntryIndex == subProperties.Count - 2)
                        subProperties.RemoveAt(subProperties.Count - 1);

                    propertyGrid.Refresh();
                }

                optionsChanged.Add(new OptionChangedValue(i, propertyTag.OptionIndex, propertyTag.OptionEntryIndex, e.ChangedItem.Value));
            }

            OnOptionsChanged(optionsChanged.ToArray());
        }
    }

    public class PropertyTag
    {
        public int OptionIndex;
        public int OptionEntryIndex;
        public int OptionID;

        public PropertyTag(int optionIndex, int optionEntryIndex, int optionId)
        {
            OptionIndex = optionIndex;
            OptionEntryIndex = optionEntryIndex;
            OptionID = optionId;
        }
    }

    public class OptionChangedValue
    {
        public int PropertyIndex;
        public int OptionIndex;
        public int OptionEntryIndex;
        public object NewValue;

        public OptionChangedValue(int propertyIndex, int optionIndex, int optionEntryIndex, object newValue)
        {
            PropertyIndex = propertyIndex;
            OptionIndex = optionIndex;
            OptionEntryIndex = optionEntryIndex;
            NewValue = newValue;
        }
    }
}
