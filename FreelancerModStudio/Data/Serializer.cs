using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace FreelancerModStudio.Data
{
    public static class Serializer
    {
        public static object Load(string path, Type type)
        {
            return Load(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), type);
        }

        public static object Load(Stream stream, Type type)
        {
            object o;
            using (StreamReader textReader = new StreamReader(stream))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                o = serializer.Deserialize(textReader);
            }

            return o;
        }

        public static void Save(string path, object o, Type type)
        {
            Save(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write), o, type);
        }

        public static void Save(Stream stream, object o, Type type)
        {
            using (StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                serializer.Serialize(textWriter, o);
            }
        }
    }
}
