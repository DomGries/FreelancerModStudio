using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.INI
{
    [Serializable]
    public class EditorINIEntry
    {
        public object Value;
        public List<object> SubOptions;

        public EditorINIEntry(object value)
        {
            Value = value;
        }

        public EditorINIEntry(object value, List<object> subOptions)
        {
            Value = value;
            SubOptions = subOptions;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}