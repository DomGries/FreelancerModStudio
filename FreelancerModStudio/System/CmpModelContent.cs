using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Data.UTF;

namespace FreelancerModStudio.SystemPresenter
{
    public class MeshGroup
    {
        public VMeshRef MeshReference;
        public VMeshData Mesh;
        public Matrix3D Transform;
    }

    public class CmpModelContent
    {
        static readonly Matrix3D ConversionMatrix = GetConversionMatrix();

        static Matrix3D GetConversionMatrix()
        {
            Matrix3D newMatrix = new Matrix3D(1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            newMatrix.Scale(new Vector3D(SystemParser.SIZE_FACTOR, SystemParser.SIZE_FACTOR, SystemParser.SIZE_FACTOR));
            return newMatrix;
        }

        static Matrix3D GetTransform(List<CmpPart> parts, string partName)
        {
            Matrix3D matrix = Matrix3D.Identity;
            foreach (CmpPart part in parts)
            {
                if (part.Name == partName)
                {
                    return matrix*part.Matrix*GetTransform(parts, part.ParentName);
                }
            }
            return matrix;
        }

        static Model3DGroup GetCmpModelGroup(List<MeshGroup> meshGroups)
        {
            Model3DGroup modelGroup = new Model3DGroup();
            foreach (MeshGroup meshGroup in meshGroups)
            {
                int endMesh = meshGroup.MeshReference.MeshStart + meshGroup.MeshReference.MeshCount;
                for (int meshIndex = meshGroup.MeshReference.MeshStart; meshIndex < endMesh; ++meshIndex)
                {
                    if (meshIndex >= meshGroup.Mesh.Meshes.Length)
                    {
                        break;
                    }

                    GeometryModel3D gm = GetCmpModel(meshGroup.Mesh, meshIndex, meshGroup.Transform*ConversionMatrix);
                    modelGroup.Children.Add(gm);
                }
            }

            return modelGroup;
        }

        static GeometryModel3D GetCmpModel(VMeshData vmesh, int meshIndex, Matrix3D transform)
        {
            VMeshData.TMeshHeader mesh = vmesh.Meshes[meshIndex];
            Point3DCollection positions = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            //PointCollection texture = new PointCollection();

            int vertexCount = mesh.BaseVertex + mesh.EndVertex - mesh.StartVertex + 1;
            for (int i = mesh.BaseVertex; i < vertexCount; ++i)
            {
                positions.Add(vmesh.Vertices[i].Position);
                normals.Add(vmesh.Vertices[i].Normal);
                //texture.Add(new Point
                //{
                //    X = vMesh.Vertices[i].S,
                //    Y = vMesh.Vertices[i].T
                //});
            }

            int triangleCount = (mesh.TriangleStart + mesh.NumRefVertices)/3;
            for (int i = mesh.TriangleStart/3; i < triangleCount; ++i)
            {
                indices.Add(vmesh.Triangles[i].Vertex1);
                indices.Add(vmesh.Triangles[i].Vertex2);
                indices.Add(vmesh.Triangles[i].Vertex3);
            }

            GeometryModel3D gm = new GeometryModel3D
                {
                    Geometry = new MeshGeometry3D
                        {
                            Positions = positions,
                            TriangleIndices = indices,
                            Normals = normals,
                            //TextureCoordinates = texture
                        },
                    Material = SharedMaterials.CmpModel,
                    Transform = new MatrixTransform3D(transform)
                };

            gm.Freeze();
            return gm;
        }

        public Model3D LoadModel(string file)
        {
            UTFManager utfManager = new UTFManager(file);
            UTFNode root = utfManager.Read();
            if (root == null || root.Nodes.Count == 0)
            {
                return null;
            }

            // select root (\) node
            root = root.Nodes[0];

            Dictionary<uint, VMeshData> meshes = null;
            List<CmpPart> constructs = new List<CmpPart>();
            Dictionary<string, string> mapFileToObj = new Dictionary<string, string>
                {
                    { "\\", "Model" }
                };

            foreach (UTFNode node in root.Nodes)
            {
                switch (node.Name.ToLowerInvariant())
                {
                    case "vmeshlibrary":
                        meshes = new Dictionary<uint, VMeshData>(node.Nodes.Count);
                        foreach (UTFNode vmsNode in node.Nodes)
                        {
                            if (vmsNode.Nodes.Count > 0)
                            {
                                meshes.Add(CrcTool.FlModelCrc(vmsNode.Name), new VMeshData(vmsNode.Nodes[0].Data));
                            }
                        }
                        break;
                    case "cmpnd":
                        foreach (UTFNode cmpndNode in node.Nodes)
                        {
                            if (cmpndNode.Name.Equals("cons", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (UTFNode constructNode in cmpndNode.Nodes)
                                {
                                    switch (constructNode.Name.ToLowerInvariant())
                                    {
                                        case "fix":
                                            FixConstruct.Parse(constructs, constructNode.Data);
                                            break;
                                        case "rev":
                                            RevConstruct.Parse(constructs, constructNode.Data);
                                            break;
                                        case "pris":
                                            PrisConstruct.Parse(constructs, constructNode.Data);
                                            break;
                                        case "sphere":
                                            SphereConstruct.Parse(constructs, constructNode.Data);
                                            break;
                                    }
                                }
                            }
                            else if (cmpndNode.Name.StartsWith("part_", StringComparison.OrdinalIgnoreCase) ||
                                     cmpndNode.Name.Equals("root", StringComparison.OrdinalIgnoreCase))
                            {
                                string objectName = null;
                                string fileName = null;
                                //int index = -1;

                                foreach (UTFNode partNode in cmpndNode.Nodes)
                                {
                                    switch (partNode.Name.ToLowerInvariant())
                                    {
                                        case "object name":
                                            objectName = Encoding.ASCII.GetString(partNode.Data).TrimEnd('\0');
                                            break;
                                        case "file name":
                                            fileName = Encoding.ASCII.GetString(partNode.Data).TrimEnd('\0');
                                            break;
                                            //case "index":
                                            //    index = BitConverter.ToInt32(partNode.Data, 0);
                                            //    break;
                                    }
                                }
                                if (objectName != null && fileName != null)
                                {
                                    mapFileToObj[fileName] = objectName;
                                }
                            }
                        }
                        break;
                        /*default:
                        if (node.Name.EndsWith(".3db", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (UTFNode subNode in node.Nodes)
                            {
                                if (subNode.Name.Equals("multilevel", StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (UTFNode levelNode in subNode.Nodes)
                                    {
                                        if (levelNode.Name.Equals("level0", StringComparison.OrdinalIgnoreCase) && levelNode.Nodes.Count > 0 && levelNode.Nodes[0].Nodes.Count > 0)
                                        {
                                            meshReferences.Add(new ExtentedMeshRef
                                                {
                                                    MeshReference = new VMeshRef(levelNode.Nodes[0].Nodes[0].Data), // Level0.VMeshPart.VMeshRef
                                                    Name = node.Name
                                                });
                                        }
                                    }
                                }
                            }
                        }
                        break;*/
                }
            }

            if (meshes == null)
            {
                return null;
            }

            List<MeshGroup> meshGroups = new List<MeshGroup>();

            // Scan the level 0 VMeshRefs to build mesh group list for each 
            // of the construction nodes identified in the previous search.
            foreach (UTFNode meshReferenceNode in root.FindNodes("VMeshRef", true))
            {
                string fileName;
                string levelName = meshReferenceNode.ParentNode.ParentNode.Name;
                if (levelName.Equals("level0", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = meshReferenceNode.ParentNode.ParentNode.ParentNode.ParentNode.Name;
                }
                else if (levelName == "\\")
                {
                    fileName = levelName;
                }
                else
                {
                    fileName = meshReferenceNode.ParentNode.ParentNode.ParentNode.Name;
                }

                string meshGroupName;
                if (mapFileToObj.TryGetValue(fileName, out meshGroupName))
                {
                    VMeshRef meshReference = new VMeshRef(meshReferenceNode.Data);
                    meshGroups.Add(new MeshGroup
                        {
                            MeshReference = meshReference,
                            Mesh = meshes[meshReference.VMeshLibId],
                            Transform = GetTransform(constructs, meshGroupName)
                        });
                }
            }

            return GetCmpModelGroup(meshGroups);
        }
    }
}
