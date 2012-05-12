using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Data.UTF;

namespace FreelancerModStudio.SystemPresenter
{
    public class MeshGroup
    {
        public string Name;
        public VMeshRef RefData;
        public Matrix3D Transform;
    }

    public class CmpModelContent
    {
        private Matrix3D GetTransform(UTFNode root, CmpData data, string partName)
        {
            foreach (CmpPart part in data.Parts)
            {
                if (part.ChildName == partName)
                    return part.Matrix * GetTransform(root, part.ParentName);
            }
            return Matrix3D.Identity;
        }

        private Matrix3D GetTransform(UTFNode root, string partName)
        {
            UTFNode cmpNode = root.FindNode("Fix", true);
            if (cmpNode != null)
                return GetTransform(root, new CmpData(cmpNode.Data, true), partName);

            //cmpNode = root.FindNode("Loose", true);
            //if (cmpNode != null)
            //    return GetTransform(root, new CmpData(cmpNode.Data, true), partName);

            cmpNode = root.FindNode("Rev", true);
            if (cmpNode != null)
                return GetTransform(root, new CmpData(cmpNode.Data, false), partName);

            cmpNode = root.FindNode("Pris", true);
            if (cmpNode != null)
                return GetTransform(root, new CmpData(cmpNode.Data, false), partName);

            return Matrix3D.Identity;
        }

        Model3DGroup GetCmpModelGroup(VMeshData vmesh, List<MeshGroup> meshGroups)
        {
            Model3DGroup modelGroup = new Model3DGroup();
            foreach (MeshGroup meshGroup in meshGroups)
            {
                int endMesh = meshGroup.RefData.MeshStart + meshGroup.RefData.MeshCount;
                for (int meshIndex = meshGroup.RefData.MeshStart; meshIndex < endMesh; meshIndex++)
                {
                    if (meshIndex >= vmesh.Meshes.Length)
                        break;

                    GeometryModel3D gm = GetCmpModel(vmesh, meshIndex, meshGroup.Transform);
                    modelGroup.Children.Add(gm);
                }
            }

            modelGroup.Transform = new Transform3DGroup
            {
                Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(SystemParser.SIZE_FACTOR, SystemParser.SIZE_FACTOR, SystemParser.SIZE_FACTOR),
                        new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 180))
                    }
            };
            return modelGroup;
        }

        GeometryModel3D GetCmpModel(VMeshData vmesh, int meshIndex, Matrix3D transform)
        {
            VMeshData.TMeshHeader mesh = vmesh.Meshes[meshIndex];
            Point3DCollection positions = new Point3DCollection();
            Int32Collection indices = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            //PointCollection texture = new PointCollection();

            int vertexCount = mesh.BaseVertex + mesh.EndVertex - mesh.StartVertex + 1;
            for (int i = mesh.BaseVertex; i < vertexCount; i++)
            {
                positions.Add(vmesh.Vertices[i].Position);
                normals.Add(vmesh.Vertices[i].Normal);
                //texture.Add(new Point
                //{
                //    X = vMesh.Vertices[i].S,
                //    Y = vMesh.Vertices[i].T
                //});
            }

            int triangleCount = (mesh.TriangleStart + mesh.NumRefVertices) / 3;
            for (int i = mesh.TriangleStart / 3; i < triangleCount; i++)
            {
                indices.Add(vmesh.Triangles[i].Vertex1);
                indices.Add(vmesh.Triangles[i].Vertex2);
                indices.Add(vmesh.Triangles[i].Vertex3);
            }

            GeometryModel3D gm = new GeometryModel3D();
            gm.Geometry = new MeshGeometry3D
            {
                Positions = positions,
                TriangleIndices = indices,
                Normals = normals,
                //TextureCoordinates = texture
            };

            gm.Material = SharedMaterials.CmpModel;
            gm.Transform = new MatrixTransform3D(transform);
            gm.Freeze();
            return gm;
        }

        public Model3D LoadModel(string file)
        {
            UTFManager utfManager = new UTFManager(file);
            UTFNode root = utfManager.Read();
            if (root == null)
                return null;

            UTFNode model = root.FindNode("VMeshData", true);
            if (model == null || model.Data == null)
                return null;

            VMeshData vmesh = new VMeshData(model.Data);
            List<MeshGroup> meshGroups = new List<MeshGroup>();

            // Find Cons(truct) nodes. They contain data that links each mesh to the
            // root mesh.
            var mapFileToObj = new Dictionary<string, string> { { "\\", "\\" } };
            foreach (UTFNode nodeObj in root.FindNodes("Object Name", true))
            {
                UTFNode nodeFileName = nodeObj.ParentNode.FindNode("File Name", false);
                if (nodeFileName != null)
                {
                    string objectName = Encoding.ASCII.GetString(nodeObj.Data).Trim('\0');
                    string fileName = Encoding.ASCII.GetString(nodeFileName.Data).Trim('\0');
                    mapFileToObj[fileName] = objectName;
                }
            }

            // Scan the level 0 VMeshRefs to build mesh group list for each 
            // of the construction nodes identified in the previous search.
            foreach (UTFNode meshRefNode in root.FindNodes("VMeshRef", true))
            {
                string levelName = meshRefNode.ParentNode.ParentNode.Name;
                string fileName;
                if (levelName == "Level0")
                    fileName = meshRefNode.ParentNode.ParentNode.ParentNode.ParentNode.Name;
                else if (levelName != "\\")
                    fileName = meshRefNode.ParentNode.ParentNode.ParentNode.Name;
                else
                    fileName = "\\";

                string meshGroupName;
                if (mapFileToObj.TryGetValue(fileName, out meshGroupName))
                {
                    meshGroups.Add(new MeshGroup
                        {
                            Name = meshGroupName,
                            RefData = new VMeshRef(meshRefNode.Data),
                            Transform = GetTransform(root, mapFileToObj[fileName])
                        });
                }
            }

            return GetCmpModelGroup(vmesh, meshGroups);
        }
    }
}
