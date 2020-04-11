namespace FreelancerModStudio.Data.INI
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EditorIniEntry
    {
        public object Value;
        public List<object> SubOptions;

        public EditorIniEntry()
        {
            
        }

        public EditorIniEntry(object value)
        {
            this.Value = value;
        }

        public EditorIniEntry(object value, List<object> subOptions)
        {
            this.Value = value;
            this.SubOptions = subOptions;
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}