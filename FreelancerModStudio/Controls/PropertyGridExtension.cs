using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using FreelancerModStudio.Settings;

namespace FreelancerModStudio
{
    public class PropertyListObjectConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if (value is string)
                return ((string)value).Replace(Environment.NewLine, "; ");
            else
                return value;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
                return ((string)value).Replace("; ", Environment.NewLine);
            else
                return value;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return null;
        }
    }

    public class PropertyBlock : PropertyValueCollection
    {
        public PropertyBlock(Settings.EditorINIBlock block, Settings.Template.Block templateBlock)
        {
            foreach (Settings.EditorINIOption option in block.Options)
                this.List.Add(new PropertyValue(option.Name, option.Values, templateBlock.Options[option.TemplateIndex]));
        }
    }

    public class PropertyValue
    {
        public string Name;

        public object Value;

        [Browsable(false)]
        public Attribute[] Attributes;

        public PropertyValue(string name, List<Settings.EditorINIEntry> values, Settings.Template.Option templateOption)
        {
            this.Name = name;

            if (templateOption.Multiple)
            {
                Attributes = new Attribute[] { 
                    new EditorAttribute(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor)) };

                StringBuilder subValues = new StringBuilder();
                foreach (Settings.EditorINIEntry entry in values)
                {
                    if (subValues.Length > 0)
                        subValues.Append(Environment.NewLine);

                    subValues.Append(entry.Value.ToString());

                    if (entry.SubOptions != null)
                    {
                        foreach (object subOption in entry.SubOptions)
                        {
                            if (subValues.Length > 0)
                                subValues.Append(Environment.NewLine);

                            subValues.Append("+" + subOption.ToString());
                        }
                    }
                }

                this.Value = subValues.ToString();
            }
            else
            {
                if (values.Count > 0)
                    this.Value = values[0].Value;
                else
                    this.Value = string.Empty;
            }
        }
    }

    public class PropertyValueCollection : System.Collections.CollectionBase, ICustomTypeDescriptor
    {
        public void Add(PropertyValue value)
        {
            this.List.Add(value);
        }

        public void Remove(PropertyValue value)
        {
            this.List.Remove(value);
        }

        public PropertyValue this[int index]
        {
            get
            {
                return (PropertyValue)this.List[index];
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
                PropertyValue propertyValue = this[i];
                properties[i] = new PropertyValueDescriptor(propertyValue, propertyValue.Attributes);
            }

            return new PropertyDescriptorCollection(properties);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }
    }

    public class PropertyValueDescriptor : PropertyDescriptor
    {
        private PropertyValue PropertyValue = null;

        public PropertyValueDescriptor(PropertyValue propertyValue, Attribute[] attributes)
            : base(propertyValue.Name, attributes)
        {
            this.PropertyValue = propertyValue;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(PropertyValue); }
        }

        public override string DisplayName
        {
            get { return this.PropertyValue.Name; }
        }

        public override string Description
        {
            get { return ""; }
        }

        public override object GetValue(object component)
        {
            return this.PropertyValue.Value;
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get
            {
                if (PropertyValue.Value != null)
                    return this.PropertyValue.Value.GetType();
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
            this.PropertyValue.Value = value;
        }
    }
}
