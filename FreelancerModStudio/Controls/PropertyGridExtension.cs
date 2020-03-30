namespace FreelancerModStudio.Controls
{
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

    public class PropertyOptionCollectionConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => 
            value is PropertySubOptions opt ? $"[{(opt.Count - 1).ToString(CultureInfo.InvariantCulture)}]" : string.Empty;
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;
    }

    public class PropertyBlock : PropertyOptionCollection
    {
        public PropertyBlock(EditorIniBlock block, Template.Block templateBlock)
        {
            foreach (EditorIniOption option in block.Options)
            {
                this.List.Add(new PropertyOption(option.Values, templateBlock.Options[option.TemplateIndex], option.ChildTemplateIndex != -1));
            }

            // show comments
            this.List.Add(new PropertyOption("comments", block.Comments));
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

        [Browsable(false)]
        public Template.OptionType Type;

        public PropertyOption(string name, string value)
        {
            // comments
            this.Name = name;
            this.Value = value ?? string.Empty;

            this.Attributes = new Attribute[] { new EditorAttribute(typeof(MultilineStringEditor), typeof(UITypeEditor)) };

        }

        public PropertyOption(List<EditorIniEntry> options, Template.Option templateOption, bool children)
        {
            this.Name = templateOption.Name;
            this.Category = templateOption.Category;
            this.Description = templateOption.Description;
            this.Type = templateOption.Type;

            if (templateOption.Multiple)
            {
                this.Attributes = new Attribute[]
                    {
                        new EditorAttribute(typeof(UITypeEditor), typeof(UITypeEditor)),
                        new TypeConverterAttribute(typeof(PropertyOptionCollectionConverter))
                    };

                this.Value = new PropertySubOptions(templateOption.Name, options, children);
            }
            else
            {
                this.Value = options.Count > 0 ? options[0].Value : string.Empty;
            }
        }

        public PropertyOption(string name, object option, List<object> subOptions, bool children)
        {
            this.Name = name;

            if (children)
            {
                this.Attributes = new Attribute[]
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

                        valueCollection.Append(subValue);
                    }
                }

                this.Value = valueCollection.ToString();
            }
            else
            {
                this.Value = option;
            }
        }
    }

    public class PropertySubOptions : PropertyOptionCollection
    {
        public PropertySubOptions(string optionName, List<EditorIniEntry> options, bool children)
        {
            int index = 0;
            foreach (EditorIniEntry entry in options)
            {
                this.List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(CultureInfo.InvariantCulture), entry.Value, entry.SubOptions, children));
                ++index;
            }

            this.List.Add(new PropertyOption(optionName + " " + (index + 1).ToString(CultureInfo.InvariantCulture), string.Empty, null, children));
        }
    }

    public class PropertyOptionCollection : CollectionBase, ICustomTypeDescriptor
    {
        public void Add(PropertyOption value) => this.List.Add(value);
        public void Remove(PropertyOption value) => this.List.Remove(value);
        public PropertyOption this[int index] => (PropertyOption)this.List[index];
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);
        public string GetClassName() => TypeDescriptor.GetClassName(this, true);
        public string GetComponentName() => TypeDescriptor.GetComponentName(this, true);
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);
        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);
        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);
        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);
        public PropertyDescriptorCollection GetProperties() => TypeDescriptor.GetProperties(this, true);
        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);
        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] properties = new PropertyDescriptor[this.List.Count];

            for (int i = 0; i < this.List.Count; ++i)
            {
                PropertyOption propertyValue = this[i];
                properties[i] = new PropertyOptionDescriptor(propertyValue, propertyValue.Attributes);
            }

            return new PropertyDescriptorCollection(properties);
        }
    }

    public class PropertyOptionDescriptor : PropertyDescriptor
    {
        public PropertyOption PropertyOption;

        public PropertyOptionDescriptor(PropertyOption propertyValue, Attribute[] attributes) : base(propertyValue.Name, attributes) => this.PropertyOption = propertyValue;
        public override bool CanResetValue(object component) => true;
        public override Type ComponentType => typeof(PropertyOption);   
        public override string DisplayName => this.PropertyOption.Name;
        public override string Category => this.PropertyOption.Category;
        public override string Description => this.PropertyOption.Description;
        public override object GetValue(object component) => this.PropertyOption.Value;
        public override bool IsReadOnly => false;
        public override Type PropertyType => this.PropertyOption.Value != null ? this.PropertyOption.Value.GetType() : typeof(object);
        public override void ResetValue(object component) => throw new NotImplementedException();
        public override bool ShouldSerializeValue(object component) => false;
        public override void SetValue(object component, object value) => this.PropertyOption.Value = value;
    }
}
