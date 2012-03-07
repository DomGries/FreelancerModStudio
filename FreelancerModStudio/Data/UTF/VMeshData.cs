using System;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    public class VMeshData
    {
        // repeated <no_meshes> times in segment - 12 bytes
        public class TMeshHeader
        {
            public uint MaterialId { get; set; }            // crc of texture name for mesh
            public ushort StartVertex { get; set; }
            public ushort EndVertex { get; set; }
            public ushort NumRefVertices { get; set; }
            public ushort Padding { get; set; }             // 0x00CC

            public int BaseVertex { get; set; }
            public int TriangleStart { get; set; }
        }

        // triangle definition - 6 bytes
        public class TTriangle
        {
            public int Vertex1 { get; set; }
            public int Vertex2 { get; set; }
            public int Vertex3 { get; set; }
        }

        // vertex definition - 32 bytes
        public class TVertex
        {
            public D3DFVF FVF { get; set; }
            public Point3D Position { get; set; }
            public Vector3D Normal { get; set; }
            public uint Diffuse { get; set; }
            public float S { get; set; }
            public float T { get; set; }
            public float U { get; set; }
            public float V { get; set; }
        }

        // Data header - 16 bytes long
        public uint MeshType { get; set; }                  // 0x00000001
        public uint SurfaceType { get; set; }               // 0x00000004
        public ushort MeshCount { get; set; }
        public ushort NumRefVertices { get; set; }
        public D3DFVF FlexibleVertexFormat { get; set; }    // 0x0112
        public ushort VertexCount { get; set; }

        public TMeshHeader[] Meshes { get; set; }
        public TTriangle[] Triangles { get; set; }
        public TVertex[] Vertices { get; set; }

        public VMeshData()
        {
        }

        public VMeshData(byte[] data)
        {
            Read(data);
        }

        public void Read(byte[] data)
        {
            int pos = 0;

            // read the data header
            MeshType = BitConverter.ToUInt32(data, pos); pos += 4;
            SurfaceType = BitConverter.ToUInt32(data, pos); pos += 4;
            MeshCount = BitConverter.ToUInt16(data, pos); pos += 2;
            NumRefVertices = BitConverter.ToUInt16(data, pos); pos += 2;
            FlexibleVertexFormat = (D3DFVF)BitConverter.ToUInt16(data, pos); pos += 2;
            VertexCount = BitConverter.ToUInt16(data, pos); pos += 2;

            // the FVF defines what fields are included for each vertex
            switch (FlexibleVertexFormat)
            {
                case D3DFVF.XYZ:
                case D3DFVF.XYZ | D3DFVF.NORMAL:
                case D3DFVF.XYZ | D3DFVF.TEX1:
                case D3DFVF.XYZ | D3DFVF.NORMAL  | D3DFVF.TEX1:
                case D3DFVF.XYZ | D3DFVF.DIFFUSE | D3DFVF.TEX1:
                case D3DFVF.XYZ | D3DFVF.NORMAL  | D3DFVF.DIFFUSE | D3DFVF.TEX1:
                case D3DFVF.XYZ | D3DFVF.NORMAL  | D3DFVF.TEX2:
                case D3DFVF.XYZ | D3DFVF.NORMAL  | D3DFVF.DIFFUSE | D3DFVF.TEX2:
                    break;
                default:
                    throw new Exception(String.Format("FVF 0x{0:X} not supported.", FlexibleVertexFormat));
            }

            // read the mesh headers
            int triangleStartOffset = 0;
            int vertexBaseOffset = 0;
            Meshes = new TMeshHeader[MeshCount];
            for (int i = 0; i < MeshCount; i++)
            {
                TMeshHeader mesh = new TMeshHeader();
                mesh.MaterialId = BitConverter.ToUInt32(data, pos); pos += 4;
                mesh.StartVertex = BitConverter.ToUInt16(data, pos); pos += 2;
                mesh.EndVertex = BitConverter.ToUInt16(data, pos); pos += 2;
                mesh.NumRefVertices = BitConverter.ToUInt16(data, pos); pos += 2;
                mesh.Padding = BitConverter.ToUInt16(data, pos); pos += 2;
               
                mesh.TriangleStart = triangleStartOffset;
                triangleStartOffset += mesh.NumRefVertices;

                mesh.BaseVertex = vertexBaseOffset;
                vertexBaseOffset += mesh.EndVertex - mesh.StartVertex + 1;

                Meshes[i] = mesh;
            }

            // read the triangle data
            int triangleCount = NumRefVertices / 3;
            Triangles = new TTriangle[triangleCount];
            for (int i = 0; i < triangleCount; i++)
            {
                TTriangle triangle = new TTriangle();
                triangle.Vertex1 = BitConverter.ToUInt16(data, pos); pos += 2;
                triangle.Vertex3 = BitConverter.ToUInt16(data, pos); pos += 2;
                triangle.Vertex2 = BitConverter.ToUInt16(data, pos); pos += 2;
                Triangles[i] = triangle;
            }

            // read the vertex data
            try
            {
                Vertices = new TVertex[VertexCount];
                for (int i = 0; i < VertexCount; i++)
                {                 
                    TVertex vertex = new TVertex();
                    vertex.FVF = FlexibleVertexFormat;

                    float x = BitConverter.ToSingle(data, pos); pos += 4;
                    float y = BitConverter.ToSingle(data, pos); pos += 4;
                    float z = BitConverter.ToSingle(data, pos); pos += 4;
                    vertex.Position = new Point3D(x, z, y);

                    if ((FlexibleVertexFormat & D3DFVF.NORMAL) == D3DFVF.NORMAL)
                    {
                        float normalX = BitConverter.ToSingle(data, pos); pos += 4;
                        float normalY = BitConverter.ToSingle(data, pos); pos += 4;
                        float normalZ = BitConverter.ToSingle(data, pos); pos += 4;
                        vertex.Normal = new Vector3D(normalX, normalZ, normalY);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.DIFFUSE) == D3DFVF.DIFFUSE)
                    {
                        vertex.Diffuse = BitConverter.ToUInt32(data, pos); pos += 4;
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX1) == D3DFVF.TEX1)
                    {
                        vertex.S = BitConverter.ToSingle(data, pos); pos += 4;
                        vertex.T = BitConverter.ToSingle(data, pos); pos += 4;
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX2) == D3DFVF.TEX2)
                    {
                        vertex.S = BitConverter.ToSingle(data, pos); pos += 4;
                        vertex.T = BitConverter.ToSingle(data, pos); pos += 4;
                        vertex.U = BitConverter.ToSingle(data, pos); pos += 4;
                        vertex.V = BitConverter.ToSingle(data, pos); pos += 4;
                    }

                    Vertices[i] = vertex;
                }
            }
            catch
            {
                //MessageBox.Show("Header has more vertices then data", "Error");
            }
        }
    }
}
