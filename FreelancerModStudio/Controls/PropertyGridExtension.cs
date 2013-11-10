using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;
using System.Text;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.INI;

namespace FreelancerModStudio.Controls
{
    public class PropertyOptionCollectionConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            PropertySubOptions propertySubOptions = value as PropertySubOptions;
            if (propertySubOptions != null)
            {
                return "[" + ((propertySubOptions).Count - 1).ToString(CultureInfo.InvariantCulture) + "]";
            }

            return string.Empty;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }
    }

    public class PropertyBlock : PropertyOptionCollection
    {
        public PropertyBlock(EditorINIBlock block, Template.Block templateBlock)
        {
            foreach (EditorINIOption option in block.Options)
            {
                List.Add(new PropertyOption(option.Values, templateBlock.Options[option.TemplateIndex], option.ChildTemplateIndex != -1));
            }

            // show comments
            List.Add(new PropertyOption("comments", block.Comments));
        }
    }

    public class PropertyOption
    {
        public string Name;

        public object Value;

        [Browsable(false)]
        public string Category;

        [Browsable(false)]
        public string Description;

        [Browsable(false)]
        public Attribute[] Attributes;

        public PropertyOption(string name, string value)
        {
            // comments
            Name = name;
            Value = value ?? string.Empty;

            Attributes = new Attribute[]
                {
                    new EditorAttribute(typeof(MultilineStringEditor), typeof(UITypeEditor))
                };
        }

        public PropertyOption(List<EditorINIEntry> options, Template.Option templateOption, bool children)
        {
            Name = templateOption.Name;

            Category = templateOption.Category;
            Description = templateOption.Description;

            if (templateOption.Multiple)
            {
                Attributes = new Attribute[]
                    {
                        new EditorAttribute(typeof(UITypeEditor), typeof(UITypeEditor)),
                        new TypeConverterAttribute(typeof(PropertyOptionCollectionConverter))
                    };

                Value = new PropertySubOptions(templateOption.Name, options, children);
            }
            else
            {
                Value = options.Count > 0 ? options[0].Value : string.Empty;
            }
        }

        public PropertyOption(string name, object option, List<object> subOptions, bool children)
        {
            Name = name;

            if (children)
            {
                Attributes = new Attribute[]
                    {
                        new EditorAttribute(typeof(MultilineStringEditor), typeof(UITypeEditor))
                    };

                StringBuilder valueCollection = new StringBuilder();
                valueCollection.Append(option);
                if (subOptions != null)
                {
                    foreach (object subValue in subOptions)
                    {
                        if (valueCollection.Length > 0)
                        {
                            valueCollection.Append(Environment.NewLine);
                        }

                        valueCollection.Append(subValue.ToString());
                    }
                }
                Value = valueCollection.ToString();
            }
            else
            {
                Value = option;
            }
        }
    }

    public class PropertySubOptions : PropertyOptionCollection
    {
        public PropertySubOptions(string optionName, List<EditorINIEntry> options, bool children)
        {
            int index = 0;
            foreach (EditorINIEntry entry in options)
            {
                List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(CultureInfo.InvariantCulture), entry.Value, entry.SubOptions, children));
                ++index;
            }

            List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(CultureInfo.InvariantCulture), string.Empty, null, children));
        }
    }

    public class PropertyOptionCollection : CollectionBase, ICustomTypeDescriptor
    {
        public void Add(PropertyOption value)
        {
            List.Add(value);
        }

        public void Remove(PropertyOption value)
        {
            List.Remove(value);
        }

        public PropertyOption this[int index]
        {
            get
            {
                return (PropertyOption)List[index];
            }
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] properties = new PropertyDescriptor[List.Count];

            for (int i = 0; i < List.Count; ++i)
            {
                PropertyOption propertyValue = this[i];
                properties[i] = new PropertyOptionDescriptor(propertyValue, propertyValue.Attributes);
            }

            return new PropertyDescriptorCollection(properties);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }
    }

    public class PropertyOptionDescriptor : PropertyDescriptor
    {
        public PropertyOption PropertyOption;

        public PropertyOptionDescriptor(PropertyOption propertyValue, Attribute[] attributes)
            : base(propertyValue.Name, attributes)
        {
            PropertyOption = propertyValue;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(PropertyOption);
            }
        }

        public override string DisplayName
        {
            get
            {
                return PropertyOption.Name;
            }
        }

        public override string Category
        {
            get
            {
                return PropertyOption.Category;
            }
        }

        public override string Description
        {
            get
            {
                return PropertyOption.Description;
            }
        }

        public override object GetValue(object component)
        {
            return PropertyOption.Value;
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                if (PropertyOption.Value != null)
                {
                    return PropertyOption.Value.GetType();
                }

                return typeof(object);
            }
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            PropertyOption.Value = value;
        }
    }
}
