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
        //public int MaxID { get; set; }

        public TableData()
        {
            Blocks = new List<TableBlock>();
        }

        public TableData(EditorINIData data)
        {
            Blocks = new List<TableBlock>();
            TemplateIndex = data.TemplateIndex;

            for (int i = 0; i < data.Blocks.Count; i++)
                Blocks.Add(new TableBlock(i, data.Blocks[i], TemplateIndex));
        }

        public EditorINIData GetEditorData()
        {
            EditorINIData data = new EditorINIData(TemplateIndex);

            foreach (TableBlock block in Blocks)
                data.Blocks.Add(block.Block);

            return data;
        }

        public void RefreshID()
        {
            for (int i = 0; i < Blocks.Count; i++)
                Blocks[i].ID = i;
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

        public ArchetypeInfo Archetype { get; set; }
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
            if (Helper.Template.Data.Files[templateIndex].Blocks.Values[block.TemplateIndex].Multiple)
                Group = block.Name;
            else
                Group = Properties.Strings.FileDefaultCategory;

            Block = block;
        }

        public string ToolTip
        {
            get
            {
                if (Block != null)
                {
                    StringBuilder values = new StringBuilder();
                    foreach (EditorINIOption option in Block.Options)
                    {
                        string append = null;
                        if (option.Values.Count > 1)
                            append = option.Name + " = [" + option.Values.Count.ToString() + "]";
                        else if (option.Values.Count == 1)
                            append = option.Name + " = " + option.Values[0].ToString();

                        if (append != null)
                        {
                            if (values.Length > 0)
                                values.Append(Environment.NewLine + append);
                            else
                                values.Append(append);
                        }
                    }

                    if (values.Length > 0)
                        return values.ToString();
                }
                return null;
            }
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
