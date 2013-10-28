using System;
using System.Globalization;
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
        public TableModified Modified;

        public ArchetypeInfo Archetype;
        public bool Visibility;

        public TableBlock(int index, int id)
        {
            Index = index;
            Id = id;
        }

        public TableBlock(int index, int id, EditorINIBlock block, int templateIndex)
        {
            Index = index;
            Id = id;

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
                    Helper.String.StringBuilder.Length = 0;
                    foreach (EditorINIOption option in Block.Options)
                    {
                        string append = null;
                        if (option.Values.Count > 1)
                        {
                            append = option.Name + " = [" + option.Values.Count.ToString(CultureInfo.InvariantCulture) + "]";
                        }
                        else if (option.Values.Count == 1)
                        {
                            append = option.Name + " = " + option.Values[0];
                        }

                        if (append != null)
                        {
                            if (Helper.String.StringBuilder.Length > 0)
                            {
                                Helper.String.StringBuilder.Append(Environment.NewLine + append);
                            }
                            else
                            {
                                Helper.String.StringBuilder.Append(append);
                            }
                        }
                    }

                    if (Helper.String.StringBuilder.Length > 0)
                    {
                        return Helper.String.StringBuilder.ToString();
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

        public void SetVisibleIfPossible()
        {
            if (ObjectType != ContentType.None)
            {
                Visibility = true;
            }
        }

        public bool IsRealModel()
        {
            // objects which have real model support (all solid objects except planets and suns)
            switch (ObjectType)
            {
                case ContentType.Construct:
                case ContentType.Depot:
                case ContentType.DockingRing:
                case ContentType.JumpGate:
                case ContentType.JumpHole:
                case ContentType.Satellite:
                case ContentType.Ship:
                case ContentType.Station:
                case ContentType.TradeLane:
                case ContentType.WeaponsPlatform:
                case ContentType.ModelPreview:
                    return true;
                default:
                    return false;
            }
        }

        public void SetModifiedChanged()
        {
            if (Modified != TableModified.ChangedAdded)
            {
                Modified = TableModified.Changed;
            }
        }
    }
}
