using System;

namespace FreelancerModStudio.Data
{
    public class Serializer
    {
        public static object Load(string path, Type type)
        {
            var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

            return Load(fileStream, type);
        }

        public static object Load(System.IO.Stream stream, Type type)
        {
            // create a TextReader using a FileStream
            var textReader = new System.IO.StreamReader(stream);
            var serializer = new System.Xml.Serialization.XmlSerializer(type);

            // deserialize using the TextReader
            var o = serializer.Deserialize(textReader);

            // close the reader
            textReader.Close();

            return o;
        }

        public static void Save(string path, object o)
        {
            Save(new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Write), o);
        }

        public static void Save(System.IO.Stream stream, object o)
        {
            var type = o.GetType();

            // create a TextWriter using a FileStream
            var textWriter = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8);
            var serializer = new System.Xml.Serialization.XmlSerializer(type);

            serializer.Serialize(textWriter, o);

            // close the writer
            textWriter.Close();
        }
    }
}