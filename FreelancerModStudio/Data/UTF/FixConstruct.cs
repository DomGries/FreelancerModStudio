using System.Collections.Generic;

namespace FreelancerModStudio.Data.UTF
{
    class FixConstruct
    {
        public static List<CmpPart> Parse(byte[] data)
        {
            List<CmpPart> parts = new List<CmpPart>();
            int pos = 0;

            while (pos != data.Length)
            {
                CmpPart part = CmpParser.ParseBaseConstruct(data, ref pos);

                part.Matrix = CmpParser.ParseRotation(data, ref pos);
                part.Matrix.Translate(part.Origin);

                parts.Add(part);
            }
            return parts;
        }
    }
}
