using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.INI
{
    [Serializable]
    public class EditorINIBlock
    {
        public string Name;
        public List<EditorINIOption> Options = new List<EditorINIOption>();
        public int TemplateIndex;
        public int MainOptionIndex = -1;

        public EditorINIBlock(string name, int templateIndex)
        {
            Name = name;
            TemplateIndex = templateIndex;
        }
    }
}