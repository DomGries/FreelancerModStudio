using System.Globalization;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data
{
    public static class Parser
    {
        public static int ParseInt(string text, int defaultValue)
        {
            int value;
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static float ParseFloat(string text, float defaultValue)
        {
            float value;
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static double ParseDouble(string text, double defaultValue)
        {
            double value;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static Vector3D ParseVector(string vector)
        {
            //Use Vector3D.Parse after implementation of type handling
            string[] values = vector.Split(new[] { ',' });
            if (values.Length > 2)
            {
                return new Vector3D(ParseDouble(values[0], 0), ParseDouble(values[1], 0), ParseDouble(values[2], 0));
            }

            return new Vector3D(0, 0, 0);
        }
    }
}
