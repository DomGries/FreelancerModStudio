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
            MeshType = CmpParser.ParseUInt32(data, ref pos);
            SurfaceType = CmpParser.ParseUInt32(data, ref pos);
            MeshCount = CmpParser.ParseUInt16(data, ref pos);
            NumRefVertices = CmpParser.ParseUInt16(data, ref pos);
            FlexibleVertexFormat = (D3DFVF)CmpParser.ParseUInt16(data, ref pos);
            VertexCount = CmpParser.ParseUInt16(data, ref pos);

            // the FVF defines what fields are included for each vertex
            /*switch (FlexibleVertexFormat)
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
            }*/

            // read the mesh headers
            int triangleStartOffset = 0;
            int vertexBaseOffset = 0;
            Meshes = new TMeshHeader[MeshCount];
            for (int i = 0; i < MeshCount; ++i)
            {
                TMeshHeader mesh = new TMeshHeader();
                mesh.MaterialId = CmpParser.ParseUInt32(data, ref pos);
                mesh.StartVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.EndVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.NumRefVertices = CmpParser.ParseUInt16(data, ref pos);
                mesh.Padding = CmpParser.ParseUInt16(data, ref pos);
               
                mesh.TriangleStart = triangleStartOffset;
                triangleStartOffset += mesh.NumRefVertices;

                mesh.BaseVertex = vertexBaseOffset;
                vertexBaseOffset += mesh.EndVertex - mesh.StartVertex + 1;

                Meshes[i] = mesh;
            }

            // read the triangle data
            int triangleCount = NumRefVertices / 3;
            Triangles = new TTriangle[triangleCount];
            for (int i = 0; i < triangleCount; ++i)
            {
                TTriangle triangle = new TTriangle();
                triangle.Vertex1 = CmpParser.ParseUInt16(data, ref pos);
                triangle.Vertex3 = CmpParser.ParseUInt16(data, ref pos);
                triangle.Vertex2 = CmpParser.ParseUInt16(data, ref pos);
                Triangles[i] = triangle;
            }

            // read the vertex data
            try
            {
                Vertices = new TVertex[VertexCount];
                for (int i = 0; i < VertexCount; ++i)
                {                 
                    TVertex vertex = new TVertex();
                    vertex.FVF = FlexibleVertexFormat;

                    vertex.Position = CmpParser.ParsePoint3D(data, ref pos);

                    if ((FlexibleVertexFormat & D3DFVF.NORMAL) == D3DFVF.NORMAL)
                    {
                        vertex.Normal = CmpParser.ParseVector3D(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.DIFFUSE) == D3DFVF.DIFFUSE)
                    {
                        vertex.Diffuse = CmpParser.ParseUInt32(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX1) == D3DFVF.TEX1)
                    {
                        vertex.S = CmpParser.ParseFloat(data, ref pos);
                        vertex.T = CmpParser.ParseFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX2) == D3DFVF.TEX2)
                    {
                        vertex.S = CmpParser.ParseFloat(data, ref pos);
                        vertex.T = CmpParser.ParseFloat(data, ref pos);
                        vertex.U = CmpParser.ParseFloat(data, ref pos);
                        vertex.V = CmpParser.ParseFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX4) == D3DFVF.TEX4)
                    {
                        vertex.S = CmpParser.ParseFloat(data, ref pos);
                        vertex.T = CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX4) == D3DFVF.TEX4)
                    {
                        vertex.S = CmpParser.ParseFloat(data, ref pos);
                        vertex.T = CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.TEX5) == D3DFVF.TEX5)
                    {
                        vertex.S = CmpParser.ParseFloat(data, ref pos);
                        vertex.T = CmpParser.ParseFloat(data, ref pos);
                        vertex.U = CmpParser.ParseFloat(data, ref pos);
                        vertex.V = CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
                        CmpParser.ParseFloat(data, ref pos);
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
