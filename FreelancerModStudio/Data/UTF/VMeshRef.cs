using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    public class VMeshRef
    {
        // Header - one per lod for each .3db section of cmp - 60 bytes
        //public uint HeaderSize;
        public uint VMeshLibId;
        public ushort VertexStart;
        //public ushort VertexCount;
        //public ushort IndexStart;
        //public ushort IndexCount;
        public ushort MeshStart;
        public ushort MeshCount;
        //public Vector3D BoundingBoxMin;
        //public Vector3D BoundingBoxMax;
        //public Vector3D Center;
        //public float Radius;

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
            CmpParser.ParseUInt32(data, ref pos); //HeaderSize
            VMeshLibId = CmpParser.ParseUInt32(data, ref pos);
            VertexStart = CmpParser.ParseUInt16(data, ref pos);
            CmpParser.ParseUInt16(data, ref pos); //VertexCount
            CmpParser.ParseUInt16(data, ref pos); //IndexStart
            CmpParser.ParseUInt16(data, ref pos); //IndexCount
            MeshStart = CmpParser.ParseUInt16(data, ref pos);
            MeshCount = CmpParser.ParseUInt16(data, ref pos);

            CmpParser.ParseFloat(data, ref pos); //boundingBoxMaxX
            CmpParser.ParseFloat(data, ref pos); //boundingBoxMinX
            CmpParser.ParseFloat(data, ref pos); //boundingBoxMaxY
            CmpParser.ParseFloat(data, ref pos); //boundingBoxMinY
            CmpParser.ParseFloat(data, ref pos); //boundingBoxMaxZ
            CmpParser.ParseFloat(data, ref pos); //boundingBoxMinZ

            //BoundingBoxMin = new Vector3D(boundingBoxMinX, boundingBoxMinZ, boundingBoxMinY);
            //BoundingBoxMax = new Vector3D(boundingBoxMaxX, boundingBoxMaxZ, boundingBoxMaxY);

            CmpParser.ParseVector3D(data, ref pos); //Center

            CmpParser.ParseFloat(data, ref pos); //Radius
        }
    }
}
