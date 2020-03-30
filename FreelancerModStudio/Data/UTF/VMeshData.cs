namespace FreelancerModStudio.Data.UTF
{
    using System.Windows.Media.Media3D;

    public class VMeshData
    {
        // repeated <no_meshes> times in segment - 12 bytes
        public class MeshHeader
        {
            // public uint MaterialId;
            public ushort StartVertex;

            public ushort EndVertex;

            public ushort NumRefVertices;

            // public ushort Padding;
            public int TriangleStart;
        }

        // triangle definition - 6 bytes
        public class Triangle
        {
            public int Vertex1;
            public int Vertex2;
            public int Vertex3;
        }

        // vertex definition - 32 bytes
        public class Vertex
        {
            public D3Dfvf Fvf;

            public Point3D Position;

            public Vector3D Normal;

            // public uint Diffuse;
            // public float S;
            // public float T;
            // public float U;
            // public float V;
        }

        // Data header - 16 bytes long
        // public uint MeshType;
        // public uint SurfaceType;
        public ushort MeshCount;
        public ushort NumRefVertices;
        public D3Dfvf FlexibleVertexFormat;
        public ushort VertexCount;

        public MeshHeader[] Meshes;
        public Triangle[] Triangles;
        public Vertex[] Vertices;

        public VMeshData()
        {
        }

        public VMeshData(byte[] data)
        {
            this.Read(data);
        }

        public void Read(byte[] data)
        {
            int pos = 0;

            // read the data header
            CmpParser.ParseUInt32(data, ref pos); // MeshType
            CmpParser.ParseUInt32(data, ref pos); // SurfaceType
            this.MeshCount = CmpParser.ParseUInt16(data, ref pos);
            this.NumRefVertices = CmpParser.ParseUInt16(data, ref pos);
            this.FlexibleVertexFormat = (D3Dfvf)CmpParser.ParseUInt16(data, ref pos);
            this.VertexCount = CmpParser.ParseUInt16(data, ref pos);

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
                    throw new Exception(string.Format("FVF 0x{0:X} not supported.", FlexibleVertexFormat));
            }*/

            // read the mesh headers
            int triangleStartOffset = 0;
            this.Meshes = new MeshHeader[this.MeshCount];
            for (int i = 0; i < this.MeshCount; ++i)
            {
                MeshHeader mesh = new MeshHeader();
                CmpParser.ParseUInt32(data, ref pos); // MaterialId
                mesh.StartVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.EndVertex = CmpParser.ParseUInt16(data, ref pos);
                mesh.NumRefVertices = CmpParser.ParseUInt16(data, ref pos);
                CmpParser.ParseUInt16(data, ref pos); // Padding

                mesh.TriangleStart = triangleStartOffset;
                triangleStartOffset += mesh.NumRefVertices;

                this.Meshes[i] = mesh;
            }

            // read the triangle data
            int triangleCount = this.NumRefVertices/3;
            this.Triangles = new Triangle[triangleCount];
            for (int i = 0; i < triangleCount; ++i)
            {
                Triangle triangle = new Triangle();
                triangle.Vertex1 = CmpParser.ParseUInt16(data, ref pos);
                triangle.Vertex3 = CmpParser.ParseUInt16(data, ref pos);
                triangle.Vertex2 = CmpParser.ParseUInt16(data, ref pos);
                this.Triangles[i] = triangle;
            }

            // read the vertex data
            try
            {
                this.Vertices = new Vertex[this.VertexCount];
                for (int i = 0; i < this.VertexCount; ++i)
                {
                    Vertex vertex = new Vertex();
                    vertex.Fvf = this.FlexibleVertexFormat;

                    vertex.Position = CmpParser.ParsePoint3D(data, ref pos);

                    if ((this.FlexibleVertexFormat & D3Dfvf.Normal) == D3Dfvf.Normal)
                    {
                        vertex.Normal = CmpParser.ParseVector3D(data, ref pos);
                    }

                    if ((this.FlexibleVertexFormat & D3Dfvf.Diffuse) == D3Dfvf.Diffuse)
                    {
                        CmpParser.ParseUInt32(data, ref pos); // Diffuse
                    }

                    if ((this.FlexibleVertexFormat & D3Dfvf.Tex1) == D3Dfvf.Tex1)
                    {
                        CmpParser.ParseFloat(data, ref pos); // S
                        CmpParser.ParseFloat(data, ref pos); // T
                    }

                    if ((this.FlexibleVertexFormat & D3Dfvf.Tex2) == D3Dfvf.Tex2)
                    {
                        CmpParser.ParseFloat(data, ref pos); // S
                        CmpParser.ParseFloat(data, ref pos); // T
                        CmpParser.ParseFloat(data, ref pos); // U
                        CmpParser.ParseFloat(data, ref pos); // V
                    }

                    if ((this.FlexibleVertexFormat & D3Dfvf.Tex4) == D3Dfvf.Tex4)
                    {
                        CmpParser.ParseFloat(data, ref pos); // S
                        CmpParser.ParseFloat(data, ref pos); // T
                        CmpParser.ParseFloat(data, ref pos); // TangentX
                        CmpParser.ParseFloat(data, ref pos); // TangentY
                        CmpParser.ParseFloat(data, ref pos); // TangentZ
                        CmpParser.ParseFloat(data, ref pos); // BinormalX
                        CmpParser.ParseFloat(data, ref pos); // BinormalY
                        CmpParser.ParseFloat(data, ref pos); // BinormalZ
                    }

                    if ((this.FlexibleVertexFormat & D3Dfvf.Tex5) == D3Dfvf.Tex5)
                    {
                        CmpParser.ParseFloat(data, ref pos); // S
                        CmpParser.ParseFloat(data, ref pos); // T
                        CmpParser.ParseFloat(data, ref pos); // U
                        CmpParser.ParseFloat(data, ref pos); // V
                        CmpParser.ParseFloat(data, ref pos); // TangentX
                        CmpParser.ParseFloat(data, ref pos); // TangentY
                        CmpParser.ParseFloat(data, ref pos); // TangentZ
                        CmpParser.ParseFloat(data, ref pos); // BinormalX
                        CmpParser.ParseFloat(data, ref pos); // BinormalY
                        CmpParser.ParseFloat(data, ref pos); // BinormalZ
                    }

                    this.Vertices[i] = vertex;
                }
            }
            catch
            {
                // MessageBox.Show("Header has more vertices then data", "Error");
            }
        }
    }
}
