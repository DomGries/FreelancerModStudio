namespace FreelancerModStudio.Data.INI
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EditorIniOption
    {
        public string Name;
        public int TemplateIndex = -1;

        public string ChildName;
        public int ChildTemplateIndex = -1;

        public List<EditorIniEntry> Values = new List<EditorIniEntry>();

        public EditorIniOption(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }
}