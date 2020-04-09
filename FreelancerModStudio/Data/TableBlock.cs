namespace FreelancerModStudio.Data
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.Properties;
    using FreelancerModStudio.SystemDesigner.Content;

    [DebuggerDisplay("{Name} ({ObjectType})")]
    [Serializable]
    public class TableBlock /*: IComparable<TableBlock>*/
    {
        public int Index;
        public int Id;
        public string Name;
        public string Group;
        public ContentType ObjectType;

        public EditorIniBlock Block;
        public TableModified Modified;

        public ArchetypeInfo Archetype;
        public bool Visibility;

        public TableBlock(int index, int id)
        {
            this.Index = index;
            this.Id = id;
        }

        public TableBlock(int index, int id, EditorIniBlock block, int templateIndex)
        {
            this.Index = index;
            this.Id = id;

            // name of block
            if (block.MainOptionIndex > -1 && block.Options.Count >= block.MainOptionIndex + 1)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                {
                    this.Name = block.Options[block.MainOptionIndex].Values[0].Value.ToString();
                }
                else
                {
                    this.Name = block.Name;
                }
            }
            else
            {
                // if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[block.TemplateIndex].Multiple)
                // blockName = blockName + i.ToString();
                // else
                this.Name = block.Name;

                // todo: different block name if they are all the same
            }

            // name of group
            if (Helper.Template.Data.Files[templateIndex].Blocks.Values[block.TemplateIndex].Multiple)
            {
                this.Group = block.Name;
            }
            else
            {
                this.Group = Strings.FileDefaultCategory;
            }

            this.Block = block;
        }

        public string ToolTip
        {
            get
            {
                if (this.Block != null)
                {
                    Helper.String.StringBuilder.Length = 0;
                    foreach (EditorIniOption option in this.Block.Options)
                    {
                        if (option.Values.Count >= 1)
                        {
                            if (Helper.String.StringBuilder.Length > 0)
                            {
                                Helper.String.StringBuilder.Append(Environment.NewLine);
                            }

                            Helper.String.StringBuilder.Append(option.Name);

                            if (option.Values.Count == 1)
                            {
                                string value = option.Values[0].ToString();
                                if (value != "=")
                                {
                                    Helper.String.StringBuilder.Append(" = ");
                                    Helper.String.StringBuilder.Append(value);
                                }
                            }
                            else
                            {
                                Helper.String.StringBuilder.Append(" = [");
                                Helper.String.StringBuilder.Append(option.Values.Count.ToString(CultureInfo.InvariantCulture));
                                Helper.String.StringBuilder.Append("]");
                            }
                        }
                    }

                    return Helper.String.StringBuilder.ToString();
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
            if (this.ObjectType != ContentType.None && !Helper.Settings.Data.Data.General.IgnoredEditorTypes.Contains(this.ObjectType))
                this.Visibility = true;
        }

        public bool IsRealModel()
        {
            // objects which have real model support (all solid objects except planets and suns)
            switch (this.ObjectType)
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
            if (this.Modified != TableModified.ChangedAdded)
            {
                this.Modified = TableModified.Changed;
            }
        }
    }
}
