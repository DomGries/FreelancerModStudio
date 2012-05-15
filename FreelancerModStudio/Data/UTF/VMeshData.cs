using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    public class VMeshData
    {
        // repeated <no_meshes> times in segment - 12 bytes
        public class TMeshHeader
        {
            //public uint MaterialId;
            public ushort StartVertex;
            public ushort EndVertex;
            public ushort NumRefVertices;
           // public ushort Padding;

            public int BaseVertex;
            public int TriangleStart;
        }

        // triangle definition - 6 bytes
        public class TTriangle
        {
            public int Vertex1;
            public int Vertex2;
            public int Vertex3;
        }

        // vertex definition - 32 bytes
        public class TVertex
        {
            public D3DFVF FVF;
            public Point3D Position;
            public Vector3D Normal;
            //public uint Diffuse;
            //public float S;
            //public float T;
            //public float U;
            //public float V;
        }

        // Data header - 16 bytes long
        //public uint MeshType;
        //public uint SurfaceType;
        public ushort MeshCount;
        public ushort NumRefVertices;
        public D3DFVF FlexibleVertexFormat;
        public ushort VertexCount;

        public TMeshHeader[] Meshes;
        public TTriangle[] Triangles;
        public TVertex[] Vertices;

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
            CmpParser.ParseUInt32(data, ref pos); //MeshType
            CmpParser.ParseUInt32(data, ref pos); //SurfaceType
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
                CmpParser.ParseUInt32(data, ref pos); //MaterialId
                mesh.StartVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.EndVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.NumRefVertices = CmpParser.ParseUInt16(data, ref pos);
                CmpParser.ParseUInt16(data, ref pos); //Padding

                mesh.TriangleStart = triangleStartOffset;
                triangleStartOffset += mesh.NumRefVertices;

                mesh.BaseVertex = vertexBaseOffset;
                vertexBaseOffset += mesh.EndVertex - mesh.StartVertex + 1;

                Meshes[i] = mesh;
            }

            // read the triangle data
            int triangleCount = NumRefVertices/3;
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

                    if ((FlexibleVertexFormat & D3DFVF.Normal) == D3DFVF.Normal)
                    {
                        vertex.Normal = CmpParser.ParseVector3D(data, ref pos);
                    }
                    if ((FlexibleVertexFormat & D3DFVF.Diffuse) == D3DFVF.Diffuse)
                    {
                        CmpParser.ParseUInt32(data, ref pos); //Diffuse
                    }
                    if ((FlexibleVertexFormat & D3DFVF.Tex1) == D3DFVF.Tex1)
                    {
                        CmpParser.ParseFloat(data, ref pos); //S
                        CmpParser.ParseFloat(data, ref pos); //T
                    }
                    if ((FlexibleVertexFormat & D3DFVF.Tex2) == D3DFVF.Tex2)
                    {
                        CmpParser.ParseFloat(data, ref pos); //S
                        CmpParser.ParseFloat(data, ref pos); //T
                        CmpParser.ParseFloat(data, ref pos); //U
                        CmpParser.ParseFloat(data, ref pos); //V
                    }
                    if ((FlexibleVertexFormat & D3DFVF.Tex4) == D3DFVF.Tex4)
                    {
                        CmpParser.ParseFloat(data, ref pos); //S
                        CmpParser.ParseFloat(data, ref pos); //T
                        CmpParser.ParseFloat(data, ref pos); //TangentX
                        CmpParser.ParseFloat(data, ref pos); //TangentY
                        CmpParser.ParseFloat(data, ref pos); //TangentZ
                        CmpParser.ParseFloat(data, ref pos); //BinormalX
                        CmpParser.ParseFloat(data, ref pos); //BinormalY
                        CmpParser.ParseFloat(data, ref pos); //BinormalZ
                    }
                    if ((FlexibleVertexFormat & D3DFVF.Tex5) == D3DFVF.Tex5)
                    {
                        CmpParser.ParseFloat(data, ref pos); //S
                        CmpParser.ParseFloat(data, ref pos); //T
                        CmpParser.ParseFloat(data, ref pos); //U
                        CmpParser.ParseFloat(data, ref pos); //V
                        CmpParser.ParseFloat(data, ref pos); //TangentX
                        CmpParser.ParseFloat(data, ref pos); //TangentY
                        CmpParser.ParseFloat(data, ref pos); //TangentZ
                        CmpParser.ParseFloat(data, ref pos); //BinormalX
                        CmpParser.ParseFloat(data, ref pos); //BinormalY
                        CmpParser.ParseFloat(data, ref pos); //BinormalZ
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
