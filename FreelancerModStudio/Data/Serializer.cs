using System;

namespace FreelancerModStudio.Data
{
    public class Serializer
    {
        public static object Load(string path, Type type)
        {
            return Load(new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read), type);
        }

        public static object Load(System.IO.Stream stream, Type type)
        {
            object o;
            using (var textReader = new System.IO.StreamReader(stream))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(type);
                o = serializer.Deserialize(textReader);
            }

            return o;
        }

        public static void Save(string path, object o)
        {
            Save(new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Write), o);
        }

        public static void Save(System.IO.Stream stream, object o)
        {
            using (var textWriter = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8))
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(o.GetType());
                serializer.Serialize(textWriter, o);
            }
        }
    }
}