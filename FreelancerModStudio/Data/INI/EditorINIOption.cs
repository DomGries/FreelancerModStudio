using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.INI
{
    [Serializable]
    public class EditorINIOption
    {
        public string Name;
        public int TemplateIndex = -1;

        public string ChildName;
        public int ChildTemplateIndex = -1;

        public List<EditorINIEntry> Values = new List<EditorINIEntry>();

        public EditorINIOption(string name, int templateIndex)
        {
            Name = name;
            TemplateIndex = templateIndex;
        }
    }
}