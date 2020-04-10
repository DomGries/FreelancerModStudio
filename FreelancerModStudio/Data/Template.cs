// ReSharper disable MemberHidesStaticFromOuterClass
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1034 // Nested types should not be visible

namespace FreelancerModStudio.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public class Template
    {
        public TemplateData Data { get; set; } = new TemplateData();
        public void Load(string path) => this.Data = (TemplateData)Serializer.Load(path, typeof(TemplateData));
        public void Save(string path) => Serializer.Save(path, this.Data, typeof(TemplateData));

        public void GenerateFileStructure(string freelancerIniPath)
        {
            // Hardcoded structure 
        }

        [XmlRoot("FreelancerModStudio-Template-1.0")]
        public class TemplateData
        {
            [XmlArrayItem("File")]
            public List<File> Files = new List<File>();

            // public CostumTypes CostumTypes;
        }

        public class File
        {
            [XmlAttribute("name")]
            public string Name;

            // public FileType Type = FileType.ini;
            [XmlArrayItem("Path")]
            public List<string> Paths;

            [XmlArrayItem("Block")]
            public Table<string, Block> Blocks = new Table<string, Block>(StringComparer.OrdinalIgnoreCase);
        }


        public class Block : ITableRow<string>
        {
            [XmlAttribute("name")]
            public string Name;

            [XmlAttribute("multiple")]
            [DefaultValue(false)]
            public bool Multiple;

            [XmlAttribute("identifier")]
            public string Identifier;

            [XmlArrayItem("Option")]
            public Options Options;

            public string Id => this.Name;
        }

        public sealed class Option : IComparable<Option>
        {
            [XmlAttribute("multiple")]
            [DefaultValue(false)]
            public bool Multiple;

            [XmlAttribute("parent")]
            public string Parent;

            [XmlAttribute("type")]
            public OptionType Type = OptionType.String;

            [XmlAttribute("enum")]
            public string EnumName;

            [XmlAttribute("renameFrom")]
            public string RenameFrom;

            [XmlAttribute("category")]
            public string Category;

            [XmlAttribute("description")]
            public string Description;

            [XmlText]
            public string Name;

            int IComparable<Option>.CompareTo(Option obj) => string.CompareOrdinal(this.Name, obj.Name);
        }

        public class Language
        {
            [XmlAttribute("id")]
            public string Id;

            public List<Description> Comments;

            public List<Category> Categories;
        }

        public class Category
        {
            [XmlAttribute("id")]
            public int Id;

            [XmlText]
            public string Value;
        }

        public class Description
        {
            [XmlAttribute("file")]
            public string File;

            [XmlAttribute("block")]
            public string Block;

            [XmlAttribute("option")]
            public string Option;

            [XmlText]
            public string Value;
        }

        public class CostumTypes
        {
            [XmlArrayItem("Enum")]
            public List<CostumEnum> Enums;
        }

        public class CostumEnum
        {
            [XmlAttribute("name")]
            public string Name;

            [XmlAttribute("type")]
            public OptionType Type = OptionType.String;

            [XmlArrayItem("Value")]
            public List<string> Values;
        }

        public enum FileType
        {
            Ini,
            Dll,
            Exe,
            Thn,
            Utf,
            Wav,
            Db,
            Threedb,
            Cmp,
            Mat,
            Sur,
            Txm,
            Ale,
            Vms,
            Txt,
            Hta,
            Fl,
            Other
        }

        public enum OptionType
        {
            String,
            Int,
            Bool,
            Point,
            Double,
            Path,
            Vector,
            Rgb,
            StringArray,
            IntArray,
            DoubleArray
        }

        public class Options : List<Option>
        {
            public int IndexOf(string name)
            {
                for (int i = 0; i < this.Count; ++i)
                {
                    if (this[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}
