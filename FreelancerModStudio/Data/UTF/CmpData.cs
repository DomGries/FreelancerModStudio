using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    class CmpData
    {
        public bool IsFixed { get; set; }
        public CmpPart[] Parts { get; set; }

        public CmpData()
        {
        }

        public CmpData(byte[] data, bool isFixed)
        {
            IsFixed = isFixed;
            Read(data);
        }

        public void Read(byte[] data)
        {
            int pos = 0;
            int partCount;
            if (IsFixed)
                partCount = data.Length / 0xB0;
            else
                partCount = data.Length / 0xD0;

            Parts = new CmpPart[partCount];
            for (int i = 0; i < partCount; i++)
            {
                CmpPart part = new CmpPart();

                if (IsFixed)
                {
                    part.ParentName = GetString(data, pos, 0x40); pos += 0x40;
                    part.ChildName = GetString(data, pos, 0x30); pos += 0x30;

                    pos += 4; // unknown
                }
                else
                {
                    int index;
                    for (index = pos; index < pos + 0x40; index++)
                    {
                        if (data[index] == 0)
                            break;
                    }
                    index -= pos;

                    part.ParentName = GetString(data, pos, index).Trim();
                    pos += 0x40;

                    for (index = pos; index < pos + 0x40; index++)
                    {
                        if (data[index] == 0)
                            break;
                    }
                    index -= pos;

                    part.ChildName = GetString(data, pos, index).Trim();
                    pos += 0x40;
                }

                float originX = BitConverter.ToSingle(data, pos); pos += 4;
                float originY = BitConverter.ToSingle(data, pos); pos += 4;
                float originZ = BitConverter.ToSingle(data, pos); pos += 4;
                part.Origin = new Vector3D(originX, originZ, originY);

                float offsetX = BitConverter.ToSingle(data, pos); pos += 4;
                float offsetY = BitConverter.ToSingle(data, pos); pos += 4;
                float offsetZ = BitConverter.ToSingle(data, pos); pos += 4;

                float matrixRotationXX = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationXY = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationXZ = BitConverter.ToSingle(data, pos); pos += 4;

                float matrixRotationYX = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationYY = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationYZ = BitConverter.ToSingle(data, pos); pos += 4;

                float matrixRotationZX = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationZY = BitConverter.ToSingle(data, pos); pos += 4;
                float matrixRotationZZ = BitConverter.ToSingle(data, pos); pos += 4;

                part.Matrix = new Matrix3D(
                    matrixRotationXX, matrixRotationZX, matrixRotationYX, 0.0f,
                    matrixRotationXZ, matrixRotationZZ, matrixRotationYZ, 0.0f,
                    matrixRotationXY, matrixRotationZY, matrixRotationYY, 0.0f,
                    offsetX, offsetZ, offsetY, 1.0f);

                if (!IsFixed)
                {
                    float axisRotationX = BitConverter.ToSingle(data, pos); pos += 4;
                    float axisRotationY = BitConverter.ToSingle(data, pos); pos += 4;
                    float axisRotationZ = BitConverter.ToSingle(data, pos); pos += 4;

                    part.AxisRotation = new Vector3D(axisRotationX, axisRotationZ, axisRotationY);

                    part.Unknown = BitConverter.ToSingle(data, pos); pos += 4;
                    part.Angle = BitConverter.ToSingle(data, pos); pos += 4;
                }

                Parts[i] = part;
            }
        }

        private string GetString(byte[] data, int index, int count)
        {
            for (int i = index; i < data.Length; i++)
            {
                if (data[i] == '\0')
                {
                    count = i - index;
                    break;
                }
            }
            return Encoding.ASCII.GetString(data, index, count);
        }

        public byte[] Write()
        {
            int fixedDiff = 0;
            if (IsFixed)
                fixedDiff = -0x1;

            List<byte> data = new List<byte>();
            foreach (CmpPart part in Parts)
            {
                if (part.ParentName.Length > 0x3F)
                    part.ParentName = part.ParentName.Substring(0, 0x3F);

                data.AddRange(Encoding.ASCII.GetBytes(part.ParentName));

                for (int i = 0; i < 0x40 - part.ParentName.Length; i++)
                    data.Add(0);

                if (part.ChildName.Length > 0x3F + fixedDiff)
                    part.ChildName = part.ChildName.Substring(0, 0x3F + fixedDiff);

                data.AddRange(Encoding.ASCII.GetBytes(part.ChildName));

                for (int i = 0; i < 0x40 + fixedDiff - part.ChildName.Length; i++)
                    data.Add(0);

                if (IsFixed)
                    data.AddRange(BitConverter.GetBytes(0));

                data.AddRange(BitConverter.GetBytes(part.Origin.X));
                data.AddRange(BitConverter.GetBytes(part.Origin.Y));
                data.AddRange(BitConverter.GetBytes(part.Origin.Z));

                data.AddRange(BitConverter.GetBytes(part.Matrix.OffsetX));
                data.AddRange(BitConverter.GetBytes(part.Matrix.OffsetZ));
                data.AddRange(BitConverter.GetBytes(part.Matrix.OffsetY));

                data.AddRange(BitConverter.GetBytes(part.Matrix.M11));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M31));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M21));

                data.AddRange(BitConverter.GetBytes(part.Matrix.M13));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M33));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M23));

                data.AddRange(BitConverter.GetBytes(part.Matrix.M12));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M32));
                data.AddRange(BitConverter.GetBytes(part.Matrix.M22));

                if (!IsFixed)
                {
                    data.AddRange(BitConverter.GetBytes(part.AxisRotation.X));
                    data.AddRange(BitConverter.GetBytes(part.AxisRotation.Z));
                    data.AddRange(BitConverter.GetBytes(part.AxisRotation.Y));

                    data.AddRange(BitConverter.GetBytes(part.Unknown));
                    data.AddRange(BitConverter.GetBytes(part.Angle));
                }
            }

            return data.ToArray();
        }
    }

    public class CmpPart
    {
        public string ParentName { get; set; }
        public string ChildName { get; set; }
        public Vector3D Origin { get; set; }
        public Matrix3D Matrix { get; set; }

        public Vector3D AxisRotation { get; set; }
        public float Unknown { get; set; }
        public float Angle { get; set; }
    }
}
