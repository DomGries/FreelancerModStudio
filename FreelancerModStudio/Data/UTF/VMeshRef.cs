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
            HeaderSize = CmpParser.ParseUInt32(data, ref pos);
            VMeshLibId = CmpParser.ParseUInt32(data, ref pos);
            VertexStart = CmpParser.ParseUInt16(data, ref pos);
            VertexCount = CmpParser.ParseUInt16(data, ref pos);
            IndexStart = CmpParser.ParseUInt16(data, ref pos);
            IndexCount = CmpParser.ParseUInt16(data, ref pos);
            MeshStart = CmpParser.ParseUInt16(data, ref pos);
            MeshCount = CmpParser.ParseUInt16(data, ref pos);

            float boundingBoxMaxX = CmpParser.ParseFloat(data, ref pos);
            float boundingBoxMinX = CmpParser.ParseFloat(data, ref pos);
            float boundingBoxMaxY = CmpParser.ParseFloat(data, ref pos);
            float boundingBoxMinY = CmpParser.ParseFloat(data, ref pos);
            float boundingBoxMaxZ = CmpParser.ParseFloat(data, ref pos);
            float boundingBoxMinZ = CmpParser.ParseFloat(data, ref pos);

            BoundingBoxMin = new Vector3D(boundingBoxMinX, boundingBoxMinZ, boundingBoxMinY);
            BoundingBoxMax = new Vector3D(boundingBoxMaxX, boundingBoxMaxZ, boundingBoxMaxY);

            Center = CmpParser.ParseVector3D(data, ref pos);

            Radius = CmpParser.ParseFloat(data, ref pos);
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
