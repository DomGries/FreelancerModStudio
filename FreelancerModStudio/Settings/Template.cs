using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace FreelancerModStudio.Settings
{
    public class Template
    {
        public TemplateData Data = new TemplateData();

        public void Load(System.IO.Stream stream)
        {
            this.Data = (TemplateData)Serializer.Load(stream, this.Data.GetType());
        }

        public void Load(string path)
        {
            this.Data = (TemplateData)Serializer.Load(path, this.Data.GetType());
        }

        public void Save(System.IO.Stream stream)
        {
            Serializer.Save(stream, this.Data);
        }

        public void Save(string path)
        {
            Serializer.Save(path, this.Data);
        }

        [XmlRootAttribute("FreelancerModStudio-Template-1.0")]
        public class TemplateData
        {
            [XmlArrayItem("File")]
            public Files Files = new Files();

            public CostumTypes CostumTypes;

            [XmlArrayItem("Language")]
            public List<Language> Languages;
        }

        public class File
        {
            [XmlAttribute("name")]
            public string Name;

            //[XmlAttribute("name")]
            public FileType Type = FileType.ini;

            public string Path;

            [XmlArrayItem("Block")]
            public List<Block> Blocks;
        }

        public class Block
        {
            [XmlAttribute("name")]
            public string Name;

            [XmlAttribute("multiple")]
            public bool Multiple = false;

            [XmlAttribute("identifier")]
            public string Identifier;

            [XmlArrayItem("Option")]
            public Options Options;
        }

        public class Option : IComparable<Option>
        {
            [XmlAttribute("multiple")]
            public bool Multiple = false;

            [XmlAttribute("type")]
            public OptionType Type = OptionType.String;

            [XmlAttribute("enum")]
            public string EnumName;

            [XmlAttribute("category")]
            public int Category = 0;

            [XmlAttribute("comment")]
            public string Comment;

            [XmlText]
            public string Name;

            int IComparable<Option>.CompareTo(Option obj)
            {
                return this.Name.CompareTo(obj.Name);
            }
        }

        public class Language
        {
            [XmlAttribute("id")]
            public string ID;

            public List<Comment> Comments;

            public List<Category> Categories;
        }

        public class Category
        {
            [XmlAttribute("id")]
            public int ID;

            [XmlText]
            public string Value;
        }

        public class Comment
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

        public enum FileType { ini, dll, exe, thn, utf, wav, db, threedb, cmp, mat, sur, txm, ale, vms, txt, hta, fl, other };

        public enum OptionType { String, Int, Bool, Point, Double, Enum, RGB, StringArray, IntArray, DoubleArray };

        public class Files : List<File>
        {
            public int IndexOf(string name)
            {
                for (int i = 0; i < this.Count; i++)
                    if (this[i].Name.ToLower() == name.ToLower())
                        return i;

                return -1;
            }
        }

        public class Options : List<Option>
        {
            public int IndexOf(string name)
            {
                for (int i = 0; i < this.Count; i++)
                    if (this[i].Name.ToLower() == name.ToLower())
                        return i;

                return -1;
            }
        }
    }
}