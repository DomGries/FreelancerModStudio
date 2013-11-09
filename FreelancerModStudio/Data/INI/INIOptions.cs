using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.INI
{
    public class INIOptions : Dictionary<string, List<INIOption>>
    {
        public INIOptions()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public new void Add(string key, List<INIOption> values)
        {
            if (ContainsKey(key))
            {
                //add value to existing option
                foreach (INIOption option in values)
                {
                    this[key].Add(option);
                }
            }
            else
            {
                //add new option
                base.Add(key, values);
            }
        }

        public void Add(string key, INIOption value)
        {
            Add(key, new List<INIOption>
                {
                    value
                });
        }
    }
}