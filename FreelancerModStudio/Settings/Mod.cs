using System.Collections.Generic;

namespace FreelancerModStudio.Settings
{
	public class Mod
	{
		public ModData Data = new ModData();

		public void Load(System.IO.Stream stream)
		{
			this.Data = (ModData)Serializer.Load(stream, this.Data.GetType());
		}

		public void Load(string path)
		{
			this.Data = (ModData)Serializer.Load(path, this.Data.GetType());
		}

		public void Save(System.IO.Stream stream)
		{
			Serializer.Save(stream, this.Data);
		}

		public void Save(string path)
		{
			Serializer.Save(path, this.Data);
		}

		[System.Xml.Serialization.XmlRootAttribute("FreelancerModStudio-Mod-1.0")]
		public class ModData
		{
			public About About;

			[System.Xml.Serialization.XmlArrayItem("File")]
			public List<string> Files;

			[System.Xml.Serialization.XmlArrayItem("Note")]
			public List<string> Notes;
		}

		public class About
		{
			public string Name;
			public string Author;
			public string Version;
			public string HomePage;
			public string Description;

			public About() { }
			public About(string name, string author, string version, string homePage, string description)
			{
				this.Name = name;
				this.Author = author;
				this.Version = version;
				this.HomePage = homePage;
				this.Description = description;
			}
		}

		public Mod() { }

		public Mod(About about)
		{
			this.Data.About = about;
		}
	}
}