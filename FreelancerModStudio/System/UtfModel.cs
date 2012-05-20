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

    public class MeshReferenceMatch
    {
        public string FileName;
        public VMeshRef MeshReference;
    }

    public static class UtfModel
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
                    if (gm != null)
                    {
                        modelGroup.Children.Add(gm);
                    }
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
            if (vmesh.Vertices.Length < vertexCount)
            {
                return null;
            }

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
            if (vmesh.Triangles.Length < triangleCount)
            {
                return null;
            }

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

        static VMeshRef ParseMeshPartNode(UTFNode meshPartNode)
        {
            if (meshPartNode.Nodes.Count > 0)
            {
                return new VMeshRef(meshPartNode.Nodes[0].Data);
            }
            return null;
        }

        static VMeshRef ParseMultiLevelNode(UTFNode node)
        {
            foreach (UTFNode levelNode in node.Nodes)
            {
                if (levelNode.Name.Equals("level0", StringComparison.OrdinalIgnoreCase) && levelNode.Nodes.Count > 0)
                {
                    return ParseMeshPartNode(levelNode.Nodes[0]);
                }
            }
            return null;
        }

        public static Model3D LoadModel(string file)
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
            List<MeshReferenceMatch> meshReferenceNodes = new List<MeshReferenceMatch>();
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
                    case "multilevel":
                        // multi LoD 3db model (\MultiLevel\Level0\VMeshPart\VMeshRef => \)
                        VMeshRef meshReference2 = ParseMultiLevelNode(node);
                        if (meshReference2 != null)
                        {
                            meshReferenceNodes.Add(new MeshReferenceMatch { FileName = "\\", MeshReference = meshReference2 });
                        }
                        break;
                    case "vmeshpart":
                        // single LoD 3db model (\VMeshPart\VMeshRef => \)
                        VMeshRef meshReference3 = ParseMeshPartNode(node);
                        if (meshReference3 != null)
                        {
                            meshReferenceNodes.Add(new MeshReferenceMatch { FileName = "\\", MeshReference = meshReference3 });
                        }
                        break;
                    default:
                        if (node.Name.EndsWith(".3db", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (UTFNode subNode in node.Nodes)
                            {
                                if (subNode.Name.Equals("multilevel", StringComparison.OrdinalIgnoreCase))
                                {
                                    // multi LoD cmp model (\PARTNAME.3db\MultiLevel\Level0\VMeshPart\VMeshRef => PARTNAME.3db)
                                    VMeshRef meshReference = ParseMultiLevelNode(subNode);
                                    if (meshReference != null)
                                    {
                                        meshReferenceNodes.Add(new MeshReferenceMatch { FileName = node.Name, MeshReference = meshReference });
                                    }
                                    break;
                                }
                                if (subNode.Name.Equals("vmeshpart", StringComparison.OrdinalIgnoreCase))
                                {
                                    // single LoD cmp model (\PARTNAME.3db\VMeshPart\VMeshRef => PARTNAME.3db)
                                    VMeshRef meshReference = ParseMeshPartNode(subNode);
                                    if (meshReference != null)
                                    {
                                        meshReferenceNodes.Add(new MeshReferenceMatch { FileName = node.Name, MeshReference = meshReference });
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                }
            }

            if (meshes == null || meshReferenceNodes.Count == 0)
            {
                return null;
            }

            List<MeshGroup> meshGroups = new List<MeshGroup>();
            foreach (MeshReferenceMatch meshReferenceNode in meshReferenceNodes)
            {
                string meshGroupName;
                if (mapFileToObj.TryGetValue(meshReferenceNode.FileName, out meshGroupName))
                {
                    VMeshData mesh;
                    if (meshes.TryGetValue(meshReferenceNode.MeshReference.VMeshLibId, out mesh))
                    {
                        meshGroups.Add(new MeshGroup
                            {
                                MeshReference = meshReferenceNode.MeshReference,
                                Mesh = mesh,
                                Transform = GetTransform(constructs, meshGroupName)
                            });
                    }
                }
            }

            return GetCmpModelGroup(meshGroups);
        }
    }
}
