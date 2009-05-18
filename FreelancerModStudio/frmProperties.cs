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

        public void ShowData(Settings.EditorINIBlock[] blocks, int templateIndex)
        {
            List<CustomPropertyCollection> propertyObjects = new List<CustomPropertyCollection>();

            //loop each selected block
            foreach (Settings.EditorINIBlock block in blocks)
            {
                CustomPropertyCollection properties = new CustomPropertyCollection();

                //loop each options
                for (int i = 0; i < block.Options.Count; i++)
                {
                    string category = "";
                    string comment = "";
                    int categoryIndex = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Category;
                    if (Helper.Template.Data.Language != null && Helper.Template.Data.Language.Categories.Count > categoryIndex)
                        category = Helper.Template.Data.Language.Categories[categoryIndex].Value;

                    bool multiple = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Multiple;
                    string name = Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].TemplateIndex].Name;
                    string childComment = "";

                    CustomPropertyCollection subProperties = new CustomPropertyCollection();

                    //loop each option
                    for (int j = 0; j < block.Options[i].Values.Count; j++)
                    {
                        CustomPropertyItem subProperty = new CustomPropertyItem(block.Options[i].Name, block.Options[i].Values[j].Value, block.Options[i].Values[j].Value, new PropertyTag(i, j, -1), category, comment, false);
                        if (block.Options[i].Values[j].SubOptions != null)
                        {
                            //add children
                            CustomPropertyCollection childSubProperties = new CustomPropertyCollection();

                            //loop each child
                            for (int k = 0; k < block.Options[i].Values[j].SubOptions.Values.Count; k++)
                                childSubProperties.Add(new CustomPropertyItem(block.Options[i].Values[j].SubOptions.Name, block.Options[i].Values[j].SubOptions.Values[k], block.Options[i].Values[j].SubOptions.Values[k], new PropertyTag(i, j, k), category, childComment, false));

                            //add empty line
                            childSubProperties.Add(new CustomPropertyItem(block.Options[i].Values[j].SubOptions.Name, "", "", new PropertyTag(i, j, block.Options[i].Values[j].SubOptions.Values.Count), category, childComment, false));

                            //add current option
                            CustomPropertyCollection childProperties = new CustomPropertyCollection();
                            childProperties.Add(subProperty);

                            if (Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Options[block.Options[i].Values[j].SubOptions.TemplateIndex].Multiple)
                            {
                                //add multiple children to current option
                                childProperties.Add(new CustomPropertyItem(childSubProperties[0].Name, childSubProperties, childSubProperties, null, category, childComment, false,
                                    new TypeConverterAttribute(typeof(PropertyListObjectConverter)),
                                    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));
                            }
                            else
                                //add single child to current option
                                childProperties.Add(childSubProperties[0]);

                            //add new sub property
                            subProperties.Add(new CustomPropertyItem(name, childProperties, childProperties, new PropertyTag(i, j, -1), category, comment, false,
                                new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                                new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));
                        }
                        else
                            //add new sub property
                            subProperties.Add(subProperty);
                    }

                    CustomPropertyItem property;
                    if (multiple)
                    {
                        if (block.Options[i].Values.Count > 0 && block.Options[i].Values[0].SubOptions != null)
                        {
                            //add empty line with children
                            //CustomPropertyCollection emptyChildList = new CustomPropertyCollection();
                            //emptyChildList.Add(new CustomPropertyItem(block.Options[i].Values[0].SubOptions.Name, "", "", new PropertyTag(i, subProperties.Count, block.Options[i].Values[0].SubOptions.Values.Count), category, childComment, false));

                            CustomPropertyCollection emptyList = new CustomPropertyCollection();
                            emptyList.Add(new CustomPropertyItem(name, "", "", new PropertyTag(i, subProperties.Count, -1), category, comment, false));
                            //emptyList.Add(new CustomPropertyItem(block.Options[i].Values[0].SubOptions.Name, emptyChildList, emptyChildList, new PropertyTag(subProperties.Count, block.Options[i].Values[0].SubOptions.Values.Count, -1), category, childComment, false,
                            //    new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                            //    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));

                            subProperties.Add(new CustomPropertyItem(block.Options[i].Name, emptyList, emptyList, new PropertyTag(i, subProperties.Count, -1), category, comment, false,
                                new TypeConverterAttribute(typeof(PropertyListObjectConverter)),
                                new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));
                        }
                        else
                            //add empty line
                            subProperties.Add(new CustomPropertyItem(block.Options[i].Name, "", "", new PropertyTag(i, subProperties.Count, -1), category, comment, false));

                        property = new CustomPropertyItem(block.Options[i].Name, subProperties, subProperties, new PropertyTag(i, 0, -1), category, comment, false,
                            new TypeConverterAttribute(typeof(PropertyListObjectConverter)),
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

                //CustomPropertyItem property = ((CustomPropertyCollection)propertyGrid.SelectedObjects[i])[propertyTag.OptionIndex];

                ////property is multiple
                //if (property.Value is CustomPropertyCollection)
                //{
                //    CustomPropertyCollection properties = (CustomPropertyCollection)property.Value;
                //    if (properties[0].Value is CustomPropertyCollection)
                //    {
                //        //property has children
                //        CustomPropertyCollection subProperties = (CustomPropertyCollection)properties[0].Value;
                //        if (propertyTag.OptionEntryChildIndex != -1)
                //        {
                //            CustomPropertyCollection childProperties = (CustomPropertyCollection)properties[propertyTag.OptionEntryChildIndex].Value;
                //        }
                //        else
                //        {
                //            if (e.ChangedItem.Value.ToString().Trim() != "" && propertyTag.OptionEntryIndex == subProperties.Count - 1)
                //            {
                //                //add new empty property with children
                //                CustomPropertyCollection emptyChildList = new CustomPropertyCollection();
                //                emptyChildList.Add(new CustomPropertyItem(block.Options[i].Values[0].SubOptions.Name, "", "", new PropertyTag(i, subProperties.Count, block.Options[i].Values[0].SubOptions.Values.Count), category, childComment, false));

                //                CustomPropertyCollection emptyList = new CustomPropertyCollection();
                //                emptyList.Add(new CustomPropertyItem(name, "", "", new PropertyTag(i, subProperties.Count, -1), category, comment, false));
                //                emptyList.Add(new CustomPropertyItem(block.Options[i].Values[0].SubOptions.Name, emptyChildList, emptyChildList, new PropertyTag(subProperties.Count, block.Options[i].Values[0].SubOptions.Values.Count, -1), category, childComment, false,
                //                    new TypeConverterAttribute(typeof(CustomExpandableObjectConverter)),
                //                    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));

                //                subProperties.Add(new CustomPropertyItem(block.Options[i].Name, emptyList, emptyList, new PropertyTag(i, subProperties.Count, -1), category, comment, false,
                //                    new TypeConverterAttribute(typeof(PropertyListObjectConverter)),
                //                    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor))));
                //            }
                //            else if (e.ChangedItem.Value.ToString().Trim() == "" && properties.Count > 1 && propertyTag.OptionEntryIndex < properties.Count - 1)
                //            {
                //                //remove empty property
                //                properties.RemoveAt(propertyTag.OptionEntryIndex);

                //                //reset tag of other properties
                //                for (int j = 0; j < properties.Count; j++)
                //                    ((PropertyTag)((CustomPropertyCollection)properties[j].Value)[0].Tag).OptionEntryIndex = j;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (e.ChangedItem.Value.ToString().Trim() != "" && propertyTag.OptionEntryIndex == properties.Count - 1)
                //            //add new empty property
                //            properties.Add(new CustomPropertyItem(e.ChangedItem.Label, "", "", new PropertyTag(propertyTag.OptionIndex, properties.Count, -1), properties[propertyTag.OptionEntryIndex].Category, properties[propertyTag.OptionEntryIndex].Description, false));
                //        else if (e.ChangedItem.Value.ToString().Trim() == "" && properties.Count > 1 && propertyTag.OptionEntryIndex < properties.Count - 1)
                //        {
                //            //remove empty property
                //            properties.RemoveAt(propertyTag.OptionEntryIndex);

                //            //reset tag of other properties
                //            for (int j = 0; j < properties.Count; j++)
                //                ((PropertyTag)properties[j].Tag).OptionEntryIndex = j;
                //        }
                //    }
                //    propertyGrid.Refresh();
                //}

                optionsChanged.Add(new OptionChangedValue(i, propertyTag.OptionIndex, propertyTag.OptionEntryIndex, e.ChangedItem.Value));
            }

            OnOptionsChanged(optionsChanged.ToArray());
        }
    }

    public class PropertyTag
    {
        public int OptionIndex;
        public int OptionEntryIndex;
        public int OptionEntryChildIndex;

        public PropertyTag(int optionIndex, int optionEntryIndex, int optionEntryChildIndex)
        {
            OptionIndex = optionIndex;
            OptionEntryIndex = optionEntryIndex;
            OptionEntryChildIndex = optionEntryChildIndex;
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

    class PropertyListObjectConverter : CustomExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is CustomPropertyCollection)
                return "[" + (((CustomPropertyCollection)value).Count - 1).ToString() + "]";
            else
                return "";
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return false;
        }
    }
}
