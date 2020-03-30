namespace FreelancerModStudio.Data
{
    using System.Collections.Generic;

    using FreelancerModStudio.Data.INI;

    public class TableData
    {
        public List<TableBlock> Blocks;
        public int TemplateIndex;
        public int MaxId;

        public TableData()
        {
            this.Blocks = new List<TableBlock>();
        }

        public TableData(EditorIniData data)
        {
            this.Blocks = new List<TableBlock>();
            this.TemplateIndex = data.TemplateIndex;

            this.MaxId = data.Blocks.Count;

            for (int i = 0; i < this.MaxId; ++i)
            {
                this.Blocks.Add(new TableBlock(i, i, data.Blocks[i], this.TemplateIndex));
            }
        }

        public EditorIniData GetEditorData()
        {
            EditorIniData data = new EditorIniData(this.TemplateIndex);

            foreach (TableBlock block in this.Blocks)
            {
                data.Blocks.Add(block.Block);
            }

            return data;
        }

        public void RefreshIndices(int startIndex)
        {
            for (int i = startIndex; i < this.Blocks.Count; ++i)
            {
                this.Blocks[i].Index = i;
            }
        }
    }
}
