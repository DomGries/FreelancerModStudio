namespace FreelancerModStudio.Data.INI
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EditorIniData
    {
        public List<EditorIniBlock> Blocks = new List<EditorIniBlock>();
        public int TemplateIndex;

        public EditorIniData(int templateIndex)
        {
            this.TemplateIndex = templateIndex;
        }
    }
}