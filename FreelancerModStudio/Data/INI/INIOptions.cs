namespace FreelancerModStudio.Data.INI
{
    using System;
    using System.Collections.Generic;

    public class IniOptions : Dictionary<string, List<IniOption>>
    {
        public IniOptions()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public new void Add(string key, List<IniOption> values)
        {
            if (this.ContainsKey(key))
            {
                // add value to existing option
                foreach (IniOption option in values)
                {
                    this[key].Add(option);
                }
            }
            else
            {
                // add new option
                base.Add(key, values);
            }
        }

        public void Add(string key, IniOption value)
        {
            this.Add(key, new List<IniOption>
                {
                    value
                });
        }
    }
}