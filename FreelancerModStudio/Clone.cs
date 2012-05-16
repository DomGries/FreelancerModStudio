using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FreelancerModStudio
{
    public static class ObjectClone
    {
        public static T Clone<T>(T o)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, o);
                objectStream.Position = 0;
                return (T)formatter.Deserialize(objectStream);
            }
        }
    }
}
