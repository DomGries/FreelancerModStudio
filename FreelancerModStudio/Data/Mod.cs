using System.Collections.Generic;
using System.Xml.Serialization;

namespace FreelancerModStudio.Data
{
    public class Mod
    {
        public ModData Data = new ModData();

        public void Load(System.IO.Stream stream)
        {
            Data = (ModData)Serializer.Load(stream, Data.GetType());
        }

        public void Load(string path)
        {
            Data = (ModData)Serializer.Load(path, Data.GetType());
        }

        public void Save(System.IO.Stream stream)
        {
            Serializer.Save(stream, Data);
        }

        public void Save(string path)
        {
            Serializer.Save(path, Data);
        }

        [XmlRoot("FreelancerModStudio-Mod-1.0")]
        public class ModData
        {
            public About About;

            [XmlArrayItem("File")]
            public List<string> Files;

            [XmlArrayItem("Note")]
            public List<string> Notes;
        }

        public class About
        {
            public string Name;
            public string Author;
            public string Version;
            public string HomePage;
            public string Description;
        }

        public Mod() { }

        public Mod(About about)
        {
            Data.About = about;
        }
    }
}