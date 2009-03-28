using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FreelancerModStudio.Settings
{
	public class Template
	{
		public SettingsData Data = new SettingsData();

		public void Load(System.IO.Stream stream)
		{
			this.Data = (SettingsData)Serializer.Load(stream, this.Data.GetType());
		}

		public void Load(string path)
		{
			this.Data = (SettingsData)Serializer.Load(path, this.Data.GetType());
		}

		public void Save(System.IO.Stream stream)
		{
			Serializer.Save(stream, this.Data);
		}

		public void Save(string path)
		{
			Serializer.Save(path, this.Data);
		}

		[System.Xml.Serialization.XmlRootAttribute("FreelancerModStudio-Template-1.0")]
		public class SettingsData
		{
			public CostumTypes CostumTypes;

			public List<Language> Languages;

			public List<File> Files;
		}

		public class Files : SortedList<string, File>, System.Xml.Serialization.IXmlSerializable
		{
			System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
			{
				return null;
			}

			void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(File));

				foreach (string key in this.Keys)
				{
					writer.WriteStartElement("File");

					writer.WriteAttributeString("name", key.ToString());
					/*writer.WriteAttributeString("Path", this.Values[this.IndexOfKey(key)].Type);
					writer.WriteElementString("Rating", this.Values[this.IndexOfKey(key)].Rating.ToString);
					writer.WriteElementString("Played", this.Values[this.IndexOfKey(key)].Played.ToString);
					writer.WriteElementString("Duration", this.Values[this.IndexOfKey(key)].Duration.ToString);
					*/
					writer.WriteEndElement();

				}
			}
		}

		public class File
		{
			[System.Xml.Serialization.XmlAttribute("type")]
			public FileType Type;

			public Identity Identity;

			public FilePath Path;

			public List<Section> Sections;

			[System.Xml.Serialization.XmlElement("Editor")]
			public List<EditorType> Editors;
		}

		public class LinkedString
		{
			public string File, Block, Value;

			public LinkedString() { }

			public LinkedString(string value)
			{
				string[] links = value.Split(new Char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

				for (int index = 0; index < links.Length; index++)
				{
					switch (index)
					{
						case 0:
							this.File = links[index].Trim();
							break;

						case 1:
							this.Block = links[index].Trim();
							break;

						case 2:
							this.Value = links[index].Trim();
							break;
					}
				}
			}

			public LinkedString(string file, string block, string value)
			{
				this.File = file;
				this.Block = block;
				this.Value = value;
			}

			public override string ToString()
			{
				if (this.File == null)
					return null;

				System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(this.File);

				if (this.Block != null)
					stringBuilder.Append("." + this.Block);

				if (this.Value != null)
					stringBuilder.Append("." + this.Value);

				return stringBuilder.ToString();
			}
		}

		public class FilePath
		{
			[System.Xml.Serialization.XmlText()]
			public string mPath;

			[System.Xml.Serialization.XmlAttribute("link")]
			public string mLink;

			public string Path
			{
				get
				{
					if (this.mLink != null)
						return this.mPath;
					else
					{
						//TODO: replace link mask with linked string
						return this.mPath;
					}
				}
			}

			public LinkedString Link
			{
				get
				{
					return new LinkedString(this.mLink);
				}
				set
				{
					this.mLink = value.ToString();
				}
			}
		}

		public enum FileType { ini, dll, exe, thn, utf, wav, db, threedb, cmp, mat, sur, txm, ale, vms, txt, hta, fl, other };

		public class Section
		{
			[System.Xml.Serialization.XmlAttribute("multiple")]
			public bool Multiple;

			public string Name;

			public List<Value> Values;
		}

		public class Value
		{
			[System.Xml.Serialization.XmlAttribute("multiple")]
			public bool Multiple;

			[System.Xml.Serialization.XmlAttribute("type")]
			public DefinedType Type;

			[System.Xml.Serialization.XmlAttribute("enumName")]
			public string EnumName;

			[System.Xml.Serialization.XmlAttribute("linkToType")]
			public string LinkToType;

			[System.Xml.Serialization.XmlAttribute("pathLinkStart")]
			public string PathLinkStart;

			[System.Xml.Serialization.XmlAttribute("category")]
			public string Category;

			[System.Xml.Serialization.XmlAttribute("comment")]
			public string Comment;

			[System.Xml.Serialization.XmlText]
			public string Name;
		}

		public enum EditorType { Default };

		public class Identity
		{
			[System.Xml.Serialization.XmlAttribute("type")]
			public IdentitiyType Type;

			[System.Xml.Serialization.XmlText]
			public string Value;
		}

		public enum IdentitiyType { Name, Header };

		public class Language
		{
			[System.Xml.Serialization.XmlAttribute("id")]
			public string ID;

			public List<Comment> Comments;
		}

		public class Comment
		{
			[System.Xml.Serialization.XmlAttribute("name")]
			public string Name;

			[System.Xml.Serialization.XmlText]
			public string Value;
		}

		public class CostumTypes
		{
			[System.Xml.Serialization.XmlArrayItem("Enum")]
			public List<CostumEnum> Enums;
		}

		public class CostumEnum
		{
			[System.Xml.Serialization.XmlAttribute("name")]
			public string Name;

			[System.Xml.Serialization.XmlAttribute("type")]
			public DefinedType Type;

			[System.Xml.Serialization.XmlArrayItem("Value")]
			public List<string> Values;
		}

		public enum DefinedType { String, Int, Point, Double, Link, PathLink, Enum, RGB, StringArray, IntArray, DoubleArray };
	}
}