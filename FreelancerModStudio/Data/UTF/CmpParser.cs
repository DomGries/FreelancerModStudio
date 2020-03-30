using System;
using System.Text;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    internal static class CmpParser
    {
        public static string ParseString(byte[] data, ref int pos)
        {
            string value = Encoding.ASCII.GetString(data, pos, ByteLen.ConstantString);
            pos += ByteLen.ConstantString;
            return value.Substring(0, value.IndexOf('\0'));
        }

        public static ushort ParseUInt16(byte[] data, ref int pos)
        {
            ushort value = BitConverter.ToUInt16(data, pos);
            pos += ByteLen.Int16;
            return value;
        }

        public static uint ParseUInt32(byte[] data, ref int pos)
        {
            uint value = BitConverter.ToUInt32(data, pos);
            pos += ByteLen.Int;
            return value;
        }

        public static float ParseFloat(byte[] data, ref int pos)
        {
            float value = BitConverter.ToSingle(data, pos);
            pos += ByteLen.Float;
            return value;
        }

        public static Vector3D ParseVector3D(byte[] data, ref int pos)
        {
            float x = ParseFloat(data, ref pos);
            float y = ParseFloat(data, ref pos);
            float z = ParseFloat(data, ref pos);
            return new Vector3D(x, z, y);
        }

        public static Point3D ParsePoint3D(byte[] data, ref int pos)
        {
            float x = ParseFloat(data, ref pos);
            float y = ParseFloat(data, ref pos);
            float z = ParseFloat(data, ref pos);
            return new Point3D(x, z, y);
        }

        public static Matrix3D ParseRotation(byte[] data, ref int pos)
        {
            float rotationXx = ParseFloat(data, ref pos);
            float rotationXy = ParseFloat(data, ref pos);
            float rotationXz = ParseFloat(data, ref pos);
            float rotationYx = ParseFloat(data, ref pos);
            float rotationYy = ParseFloat(data, ref pos);
            float rotationYz = ParseFloat(data, ref pos);
            float rotationZx = ParseFloat(data, ref pos);
            float rotationZy = ParseFloat(data, ref pos);
            float rotationZz = ParseFloat(data, ref pos);

            return new Matrix3D(
                rotationXx, rotationZx, rotationYx, 0.0,
                rotationXz, rotationZz, rotationYz, 0.0,
                rotationXy, rotationZy, rotationYy, 0.0,
                0d, 0d, 0d, 1.0);
        }
    }
}
