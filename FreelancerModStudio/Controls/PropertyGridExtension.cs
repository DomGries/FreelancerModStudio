using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio
{
    public class PropertyOptionCollectionConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is PropertySubOptions)
                return "[" + (((PropertySubOptions)value).Count - 1).ToString() + "]";
            else
                return "";
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return false;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return base.GetProperties(context, value, attributes);
        }
    }

    public class PropertyBlock : PropertyOptionCollection
    {
        public PropertyBlock(EditorINIBlock block, Template.Block templateBlock)
        {
            foreach (EditorINIOption option in block.Options)
                this.List.Add(new PropertyOption(option.Values, templateBlock.Options[option.TemplateIndex], option.ChildTemplateIndex != -1));
        }
    }

    public class PropertyOption
    {
        public string Name;

        public object Value;

        [Browsable(false)]
        public Attribute[] Attributes;

        public PropertyOption(List<EditorINIEntry> options, Template.Option templateOption, bool children)
        {
            this.Name = templateOption.Name;

            if (templateOption.Multiple)
            {
                Attributes = new Attribute[] { 
                    new EditorAttribute(typeof(System.Drawing.Design.UITypeEditor), typeof(System.Drawing.Design.UITypeEditor)),
                    new TypeConverterAttribute(typeof(PropertyOptionCollectionConverter)) };

                this.Value = new PropertySubOptions(templateOption.Name, options, children);
            }
            else
            {
                if (options.Count > 0)
                    this.Value = options[0].Value;
                else
                    this.Value = string.Empty;
            }
        }

        public PropertyOption(string name, object option, List<object> subOptions, bool children)
        {
            this.Name = name;

            if (children)
            {
                Attributes = new Attribute[] { 
                    new EditorAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)) };

                StringBuilder valueCollection = new StringBuilder();
                valueCollection.Append(option);
                if (subOptions != null)
                {
                    foreach (object subValue in subOptions)
                    {
                        if (valueCollection.Length > 0)
                            valueCollection.Append(Environment.NewLine);

                        valueCollection.Append(subValue.ToString());
                    }
                }
                this.Value = valueCollection.ToString();
            }
            else
                this.Value = option;
        }
    }

    public class PropertySubOptions : PropertyOptionCollection
    {
        public PropertySubOptions(string optionName, List<EditorINIEntry> options, bool children)
        {
            int index = 0;
            foreach (EditorINIEntry entry in options)
            {
                this.List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(), entry.Value, entry.SubOptions, children));
                index++;
            }

            this.List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(), "", null, children));
        }
    }

    public class PropertyOptionCollection : System.Collections.CollectionBase, ICustomTypeDescriptor
    {
        public void Add(PropertyOption value)
        {
            this.List.Add(value);
        }

        public void Remove(PropertyOption value)
        {
            this.List.Remove(value);
        }

        public PropertyOption this[int index]
        {
            get
            {
                return (PropertyOption)this.List[index];
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
            PropertyDescriptor[] properties = new PropertyDescriptor[this.List.Count];

            for (int i = 0; i < this.List.Count; i++)
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
        public PropertyOption PropertyOption = null;

        public PropertyOptionDescriptor(PropertyOption propertyValue, Attribute[] attributes)
            : base(propertyValue.Name, attributes)
        {
            this.PropertyOption = propertyValue;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(PropertyOption); }
        }

        public override string DisplayName
        {
            get { return this.PropertyOption.Name; }
        }

        public override string Description
        {
            get { return ""; }
        }

        public override object GetValue(object component)
        {
            return this.PropertyOption.Value;
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get
            {
                if (PropertyOption.Value != null)
                    return this.PropertyOption.Value.GetType();
                else
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
            this.PropertyOption.Value = value;
        }
    }
}
