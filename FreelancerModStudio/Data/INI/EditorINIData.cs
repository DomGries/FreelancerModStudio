using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.INI
{
    [Serializable]
    public class EditorINIData
    {
        public List<EditorINIBlock> Blocks = new List<EditorINIBlock>();
        public int TemplateIndex;

        public EditorINIData(int templateIndex)
        {
            TemplateIndex = templateIndex;
        }
    }
}