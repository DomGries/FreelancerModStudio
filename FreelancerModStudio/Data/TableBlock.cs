using System;
using System.Text;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Properties;
using FreelancerModStudio.SystemPresenter.Content;

namespace FreelancerModStudio.Data
{
    [Serializable]
    public class TableBlock /*: IComparable<TableBlock>*/
    {
        public int Index;
        public int Id;
        public string Name;
        public string Group;
        public ContentType ObjectType;

        public EditorINIBlock Block;
        public TableModified Modified = TableModified.Normal;

        public ArchetypeInfo Archetype;
        public bool Visibility;

        public TableBlock(int index, int id)
        {
            Index = index;
            Id = id;
            ObjectType = ContentType.None;
        }

        public TableBlock(int index, int id, EditorINIBlock block, int templateIndex)
        {
            Index = index;
            Id = id;
            ObjectType = ContentType.None;

            //name of block
            if (block.MainOptionIndex > -1 && block.Options.Count >= block.MainOptionIndex + 1)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                {
                    Name = block.Options[block.MainOptionIndex].Values[0].Value.ToString();
                }
                else
                {
                    Name = block.Name;
                }
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
            {
                Group = block.Name;
            }
            else
            {
                Group = Strings.FileDefaultCategory;
            }

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
                        {
                            append = option.Name + " = [" + option.Values.Count.ToString() + "]";
                        }
                        else if (option.Values.Count == 1)
                        {
                            append = option.Name + " = " + option.Values[0];
                        }

                        if (append != null)
                        {
                            if (values.Length > 0)
                            {
                                values.Append(Environment.NewLine + append);
                            }
                            else
                            {
                                values.Append(append);
                            }
                        }
                    }

                    if (values.Length > 0)
                    {
                        return values.ToString();
                    }
                }
                return null;
            }
        }

        /*public int CompareTo(TableBlock other)
        {
            //sort by group, object type, name, modified
            int groupComparison = Group.CompareTo(other.Group);
            if (groupComparison == 0)
            {
                int objectTypeComparison = ObjectType.CompareTo(other.ObjectType);
                if (objectTypeComparison == 0)
                {
                    int nameComparison = StringLogicalComparer.Compare(Name, other.Name);
                    if (nameComparison == 0)
                    {
                        return Modified.CompareTo(other.Modified);
                    }

                    return nameComparison;
                }

                return objectTypeComparison;
            }

            return groupComparison;
        }*/
    }
}
