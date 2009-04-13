using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace FreelancerModStudio
{
    public class CustomPropertyItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool ReadOnly { get; set; }
        public object Value { get; set; }
        public object Tag { get; set; }
        public string Category { get; set; }
        public virtual Attribute[] Attributes { get; set; }

        public CustomPropertyItem(string name, object value, object tag, string category, string description, bool readOnly, params Attribute[] attributes)
        {
            Name = name;
            Value = value;
            Tag = tag;
            Description = description;
            ReadOnly = readOnly;
            Category = category;
            Attributes = attributes;
        }
    }

    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        private CustomPropertyItem mPropertyItem;

        public CustomPropertyDescriptor(CustomPropertyItem propertyItem, Attribute[] attrs)
            : base(propertyItem.Name, attrs)
        {
            mPropertyItem = propertyItem;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override object GetValue(object component)
        {
            return mPropertyItem.Value;
        }

        public override bool IsReadOnly
        {
            get { return mPropertyItem.ReadOnly; }
        }

        public override Type PropertyType
        {
            get { return mPropertyItem.Value.GetType(); }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            mPropertyItem.Value = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override string Description
        {
            get
            {
                return mPropertyItem.Description;
            }
        }

        public override string Category
        {
            get
            {
                return mPropertyItem.Category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return mPropertyItem.Name;
            }
        }
    }

    public class CustomPropertyCollection : CollectionBase, ICustomTypeDescriptor
    {
        public void Add(CustomPropertyItem Property)
        {
            base.List.Add(Property);
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

}
