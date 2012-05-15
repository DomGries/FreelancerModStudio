using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    internal class PrisConstruct
    {
        public static List<CmpPart> Parse(byte[] data)
        {
            List<CmpPart> parts = new List<CmpPart>();
            int pos = 0;

            while (pos != data.Length)
            {
                CmpPart part = CmpParser.ParseBaseConstruct(data, ref pos);

                Vector3D offset = CmpParser.ParseVector3D(data, ref pos);

                part.Matrix = CmpParser.ParseRotation(data, ref pos);
                part.Matrix.Translate(part.Origin + offset);

                part.AxisRotation = CmpParser.ParseVector3D(data, ref pos);
                part.Min = CmpParser.ParseFloat(data, ref pos);
                part.Max = CmpParser.ParseFloat(data, ref pos);

                parts.Add(part);
            }
            return parts;
        }
    }
}
