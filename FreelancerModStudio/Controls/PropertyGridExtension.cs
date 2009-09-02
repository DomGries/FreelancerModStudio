using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;
using FreelancerModStudio.Settings;

namespace FreelancerModStudio
{
    public class CustomPropertyItem
    {
        [Browsable(false)]
        public string Name { get; set; }
        [Browsable(false)]
        public string Description { get; set; }
        [Browsable(false)]
        public bool ReadOnly { get; set; }
        public object Value { get; set; }
        [Browsable(false)]
        public object DefaultValue { get; set; }
        [Browsable(false)]
        public object Tag { get; set; }
        [Browsable(false)]
        public string Category { get; set; }
        [Browsable(false)]
        public virtual Attribute[] Attributes { get; set; }

        public CustomPropertyItem(string name, object value, object defaultValue, object tag, string category, string description, bool readOnly, params Attribute[] attributes)
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            Tag = tag;
            Description = description;
            ReadOnly = readOnly;
            Category = category;
            Attributes = attributes;
        }
    }

    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        public CustomPropertyItem PropertyItem {get; set;}

        public CustomPropertyDescriptor(CustomPropertyItem property, Attribute[] attrs)
            : base(property.Name, attrs)
        {
            PropertyItem = property;
        }

        public override bool CanResetValue(object component)
        {
            return !PropertyItem.Value.Equals(PropertyItem.DefaultValue);
        }

        public override Type ComponentType
        {
            get { return typeof(CustomPropertyItem); }
        }

        public override object GetValue(object component)
        {
            return PropertyItem.Value;
        }

        public override bool IsReadOnly
        {
            get { return PropertyItem.ReadOnly; }
        }

        public override Type PropertyType
        {
            get
            {
                if (PropertyItem.Value != null)
                    return PropertyItem.Value.GetType();
                else
                    return typeof(object);
            }
        }

        public override void ResetValue(object component)
        {
            PropertyItem.Value = PropertyItem.DefaultValue;
        }

        public override void SetValue(object component, object value)
        {
            PropertyItem.Value = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return CanResetValue(component);
        }

        public override string Description
        {
            get
            {
                return PropertyItem.Description;
            }
        }

        public override string Category
        {
            get
            {
                return PropertyItem.Category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return PropertyItem.Name;
            }
        }
    }

    public class CustomPropertyCollection : CollectionBase, ICustomTypeDescriptor
    {
        public void Add(CustomPropertyItem property)
        {
            base.List.Add(property);
        }

        public void Insert(int index, CustomPropertyItem property)
        {
            base.List.Insert(index, property);
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < base.List.Count; i++)
            {
                if (((CustomPropertyItem)base.List[i]).Name == name)
                    return i;
            }
            return -1;
        }

        public void Remove(string name)
        {
            int index = IndexOf(name);
            if (index != -1)
                base.List.Remove(base.List[index]);
        }

        public CustomPropertyItem this[int index]
        {
            get
            {
                return (CustomPropertyItem)base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
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

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] newProps = new PropertyDescriptor[this.Count];

            for (int i = 0; i < this.Count; i++)
            {
                CustomPropertyItem propertyItem = (CustomPropertyItem)this[i];
                newProps[i] = new CustomPropertyDescriptor(propertyItem, propertyItem.Attributes);
            }

            return new PropertyDescriptorCollection(newProps);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }

    public class CustomExpandableObjectConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            return "";
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return false;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value is CustomPropertyCollection)
                return ((CustomPropertyCollection)value).GetProperties(attributes);
            else
                return base.GetProperties(context, value, attributes);
        }
    }
}
