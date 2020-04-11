namespace FreelancerModStudio.Data.INI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    [Serializable]
    [DebuggerDisplay("{Name} (#{TemplateIndex})")]
    public class EditorIniBlock
    {
        public string Name;
        public List<EditorIniOption> Options = new List<EditorIniOption>();
        public int TemplateIndex;
        public int MainOptionIndex = -1;
        public string Comments;

        public EditorIniBlock(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }

        public EditorIniBlock()
        {
            
        }
    }
}