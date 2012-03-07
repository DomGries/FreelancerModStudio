using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    public class VMeshRef
    {
        // Header - one per lod for each .3db section of cmp - 60 bytes
        public uint HeaderSize { get; set; }            // 0x0000003C
        public uint VMeshLibId { get; set; }            // crc of 3db name
        public ushort VertexStart { get; set; }
        public ushort VertexCount { get; set; }
        public ushort IndexStart { get; set; }
        public ushort IndexCount { get; set; }
        public ushort MeshStart { get; set; }
        public ushort MeshCount { get; set; }
        public Vector3D BoundingBoxMin { get; set; }
        public Vector3D BoundingBoxMax { get; set; }
        public Vector3D Center { get; set; }
        public float Radius { get; set; }

        public VMeshRef()
        {
        }

        public VMeshRef(byte[] data)
        {
            Read(data);
        }

        public void Read(byte[] data)
        {
            int pos = 0;
            HeaderSize = BitConverter.ToUInt32(data, pos); pos += 4;
            VMeshLibId = BitConverter.ToUInt32(data, pos); pos += 4;
            VertexStart = BitConverter.ToUInt16(data, pos); pos += 2;
            VertexCount = BitConverter.ToUInt16(data, pos); pos += 2;
            IndexStart = BitConverter.ToUInt16(data, pos); pos += 2;
            IndexCount = BitConverter.ToUInt16(data, pos); pos += 2;
            MeshStart = BitConverter.ToUInt16(data, pos); pos += 2;
            MeshCount = BitConverter.ToUInt16(data, pos); pos += 2;

            float boundingBoxMaxX = BitConverter.ToSingle(data, pos); pos += 4;
            float boundingBoxMinX = BitConverter.ToSingle(data, pos); pos += 4;
            float boundingBoxMaxY = BitConverter.ToSingle(data, pos); pos += 4;
            float boundingBoxMinY = BitConverter.ToSingle(data, pos); pos += 4;
            float boundingBoxMaxZ = BitConverter.ToSingle(data, pos); pos += 4;
            float boundingBoxMinZ = BitConverter.ToSingle(data, pos); pos += 4;

            BoundingBoxMin = new Vector3D(boundingBoxMinX, boundingBoxMinZ, boundingBoxMinY);
            BoundingBoxMax = new Vector3D(boundingBoxMaxX, boundingBoxMaxZ, boundingBoxMaxY);

            float centerX = BitConverter.ToSingle(data, pos); pos += 4;
            float centerY = BitConverter.ToSingle(data, pos); pos += 4;
            float centerZ = BitConverter.ToSingle(data, pos); pos += 4;
            Center = new Vector3D(centerX, centerZ, centerY);

            Radius = BitConverter.ToSingle(data, pos); pos += 4;
        }

        public byte[] Write()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(HeaderSize));
            data.AddRange(BitConverter.GetBytes(VMeshLibId));
            data.AddRange(BitConverter.GetBytes(VertexStart));
            data.AddRange(BitConverter.GetBytes(VertexCount));
            data.AddRange(BitConverter.GetBytes(IndexStart));
            data.AddRange(BitConverter.GetBytes(IndexCount));
            data.AddRange(BitConverter.GetBytes(MeshStart));
            data.AddRange(BitConverter.GetBytes(MeshCount));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMax.X));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMin.X));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMax.Z));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMin.Z));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMax.Y));
            data.AddRange(BitConverter.GetBytes(BoundingBoxMin.Y));
            data.AddRange(BitConverter.GetBytes(Center.X));
            data.AddRange(BitConverter.GetBytes(Center.Y));
            data.AddRange(BitConverter.GetBytes(Center.Z));
            data.AddRange(BitConverter.GetBytes(Radius));
            return data.ToArray();
        }
    }
}
