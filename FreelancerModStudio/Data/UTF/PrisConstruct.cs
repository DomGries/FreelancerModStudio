using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    internal static class PrisConstruct
    {
        public static void Parse(List<CmpPart> constructs, byte[] data)
        {
            int pos = 0;
            while (pos != data.Length)
            {
                CmpPart part = new CmpPart();

                part.ParentName = CmpParser.ParseString(data, ref pos);
                part.Name = CmpParser.ParseString(data, ref pos);

                Vector3D origin = CmpParser.ParseVector3D(data, ref pos);
                Vector3D offset = CmpParser.ParseVector3D(data, ref pos);

                part.Matrix = CmpParser.ParseRotation(data, ref pos);
                part.Matrix.Translate(origin + offset);

                CmpParser.ParseVector3D(data, ref pos); //AxisRotation
                CmpParser.ParseFloat(data, ref pos); //Min
                CmpParser.ParseFloat(data, ref pos); //Max

                constructs.Add(part);
            }
        }
    }
}
