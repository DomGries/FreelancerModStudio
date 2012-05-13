using System;
using System.Text;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    class CmpParser
    {
        public static CmpPart ParseBaseConstruct(byte[] data, ref int pos)
        {
            CmpPart part = new CmpPart();

            part.ParentName = ParseString(data, ref pos);
            part.ChildName = ParseString(data, ref pos);

            part.Origin = ParseVector3D(data, ref pos);

            return part;
        }

        public static string ParseString(byte[] data, ref int pos)
        {
            string value = Encoding.ASCII.GetString(data, pos, ByteLen.CONSTANT_STRING);
            pos += ByteLen.CONSTANT_STRING;
            return value.Substring(0, value.IndexOf('\0'));
        }

        public static ushort ParseUInt16(byte[] data, ref int pos)
        {
            ushort value = BitConverter.ToUInt16(data, pos);
            pos += ByteLen.INT16;
            return value;
        }

        public static uint ParseUInt32(byte[] data, ref int pos)
        {
            uint value = BitConverter.ToUInt32(data, pos);
            pos += ByteLen.INT;
            return value;
        }

        public static float ParseFloat(byte[] data, ref int pos)
        {
            float value = BitConverter.ToSingle(data, pos);
            pos += ByteLen.FLOAT;
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
            float rotationXX = ParseFloat(data, ref pos);
            float rotationXY = ParseFloat(data, ref pos);
            float rotationXZ = ParseFloat(data, ref pos);
            float rotationYX = ParseFloat(data, ref pos);
            float rotationYY = ParseFloat(data, ref pos);
            float rotationYZ = ParseFloat(data, ref pos);
            float rotationZX = ParseFloat(data, ref pos);
            float rotationZY = ParseFloat(data, ref pos);
            float rotationZZ = ParseFloat(data, ref pos);

            return new Matrix3D(
                rotationXX, rotationZX, rotationYX, 0.0,
                rotationXZ, rotationZZ, rotationYZ, 0.0,
                rotationXY, rotationZY, rotationYY, 0.0,
                0d, 0d, 0d, 1.0);
        }
    }
}
