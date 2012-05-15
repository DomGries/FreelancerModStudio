using System.Collections.Generic;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.Data
{
    public class TableData
    {
        public List<TableBlock> Blocks;
        public int TemplateIndex;
        public int MaxId;

        public TableData()
        {
            Blocks = new List<TableBlock>();
        }

        public TableData(EditorINIData data)
        {
            Blocks = new List<TableBlock>();
            TemplateIndex = data.TemplateIndex;

            MaxId = data.Blocks.Count;

            for (int i = 0; i < MaxId; ++i)
                Blocks.Add(new TableBlock(i, i, data.Blocks[i], TemplateIndex));
        }

        public EditorINIData GetEditorData()
        {
            EditorINIData data = new EditorINIData(TemplateIndex);

            foreach (TableBlock block in Blocks)
                data.Blocks.Add(block.Block);

            return data;
        }

        public void RefreshIndices(int startIndex)
        {
            for (int i = startIndex; i < Blocks.Count; ++i)
                Blocks[i].Index = i;
        }
    }
}
