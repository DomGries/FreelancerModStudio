using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.SystemPresenter;

namespace FreelancerModStudio.Data
{
    public class TableData
    {
        public List<TableBlock> Blocks { get; set; }
        public int TemplateIndex { get; set; }
        public int MaxID { get; set; }

        public TableData()
        {
            Blocks = new List<TableBlock>();
        }

        public TableData(EditorINIData data)
        {
            Blocks = new List<TableBlock>();
            TemplateIndex = data.TemplateIndex;

            foreach (EditorINIBlock block in data.Blocks)
            {
                MaxID++;
                Blocks.Add(new TableBlock(MaxID, block, TemplateIndex));
            }
        }

        public EditorINIData GetEditorData()
        {
            EditorINIData data = new EditorINIData(TemplateIndex);

            foreach (TableBlock block in Blocks)
                data.Blocks.Add(block.Block);

            return data;
        }
    }

    [Serializable]
    public class TableBlock : IComparable<TableBlock>
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public ContentType ObjectType { get; set; }

        public EditorINIBlock Block;
        public TableModified Modified = TableModified.Normal;

        public ArchtypeInfo Archtype { get; set; }
        public bool Visibility { get; set; }

        public TableBlock(int id)
        {
            ID = id;
            ObjectType = ContentType.None;
        }

        public TableBlock(int id, EditorINIBlock block, int templateIndex)
        {
            ID = id;
            ObjectType = ContentType.None;

            //name of block
            if (block.MainOptionIndex > -1 && block.Options.Count >= block.MainOptionIndex + 1)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                    Name = block.Options[block.MainOptionIndex].Values[0].Value.ToString();
                else
                    Name = block.Name;
            }
            else
            {
                //if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[block.TemplateIndex].Multiple)
                //    blockName = blockName + i.ToString();
                //else
                Name = block.Name;
                //todo: different block name if they are all the same
            }

            //name of group
            if (Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Multiple)
                Group = block.Name;
            else
                Group = Properties.Strings.FileDefaultCategory;

            Block = block;
        }

        public int CompareTo(TableBlock other)
        {
            //sort by group, object type, name, modified
            int groupComparison = this.Group.CompareTo(other.Group);
            if (groupComparison == 0)
            {
                int objectTypeComparison = this.ObjectType.CompareTo(other.ObjectType);
                if (objectTypeComparison == 0)
                {
                    int nameComparison = StringLogicalComparer.Compare(this.Name, other.Name);
                    if (nameComparison == 0)
                        return this.Modified.CompareTo(other.Modified);

                    return nameComparison;
                }

                return objectTypeComparison;
            }

            return groupComparison;
        }
    }

    public enum TableModified
    {
        Normal,
        Changed,
        ChangedSaved
    }
}
