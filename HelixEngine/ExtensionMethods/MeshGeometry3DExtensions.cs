using System.Windows;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    public static class MeshGeometry3DExtensions
    {
        /// <summary>
        /// Adds a triangle to the mesh.
        /// Note: this is not a good solution (perfomance-wise) if you are adding a big list of triangles
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public static void AddTriangle(this MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2)
        {
            mesh.AddTriangle(p0, p1, p2, false);
        }

        public static void AddTriangle(this MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2, bool addNormals)
        {
            Vector3D u = p1 - p0;
            Vector3D v = p2 - p0;
            Vector3D w = Vector3D.CrossProduct(u,v);
            w.Normalize();

            var uv0 = new Point(0, 0);
            var uv1 = new Point(1, 0);
            var uv2 = new Point(0, 1);

            int i0 = mesh.Positions.Count;

            mesh.Positions.Add(p0); mesh.TextureCoordinates.Add(uv0);
            mesh.Positions.Add(p1); mesh.TextureCoordinates.Add(uv1);
            mesh.Positions.Add(p2); mesh.TextureCoordinates.Add(uv2);

            if (addNormals)
            {
                mesh.Normals.Add(w);
                mesh.Normals.Add(w);
                mesh.Normals.Add(w);
            }

            mesh.TriangleIndices.Add(i0 + 0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);

        }

        public static void AddQuad(this MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            mesh.AddQuad(p0,p1,p2,p3,false);
        }

        public static void AddQuad(this MeshGeometry3D mesh, Point3D p0, Point3D p1, Point3D p2, Point3D p3, bool addNormals)
        {
            Vector3D u = p1 - p0;
            Vector3D w = p3 - p0;
            Vector3D v = Vector3D.CrossProduct(w, u);
            u.Normalize();
            v.Normalize();
            w.Normalize();

            var uv0 = new Point(0, 0);
            var uv1 = new Point(1, 0);
            var uv2 = new Point(0, 1);
            var uv3 = new Point(1, 1);

            int i0 = mesh.Positions.Count;

            mesh.Positions.Add(p0); mesh.TextureCoordinates.Add(uv0);
            mesh.Positions.Add(p1); mesh.TextureCoordinates.Add(uv1);
            mesh.Positions.Add(p2); mesh.TextureCoordinates.Add(uv2);
            mesh.Positions.Add(p3); mesh.TextureCoordinates.Add(uv3);

            if (addNormals)
            {
                mesh.Normals.Add(w);
                mesh.Normals.Add(w);
                mesh.Normals.Add(w);
                mesh.Normals.Add(w);
            }
            
            mesh.TriangleIndices.Add(i0 + 0);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 2);

            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 3);

        }

        public static void AddCubeFace(this MeshGeometry3D mesh, Point3D center, Vector3D normal, Vector3D up, double dist, double length, double height)
        {
            mesh.AddCubeFace(center,normal,up,dist,length,height,false);
        }

        /// <summary>
        /// Adds a cube face to the MeshGeometry3D
        /// </summary>
        /// <param name="mesh">A MeshGeometry3D object</param>
        /// <param name="center">Center of the cube</param>
        /// <param name="normal">Face normal</param>
        /// <param name="up">Up vector of the face</param>
        /// <param name="dist">Distance from the center of the cube</param>
        /// <param name="length">Length of the face</param>
        /// <param name="height">Height of the face</param>
        /// <param name="addNormals">Set to true to add normals to the MeshGeometry</param>
        public static void AddCubeFace(this MeshGeometry3D mesh, Point3D center, Vector3D normal, Vector3D up, double dist, double length, double height, bool addNormals)
        {
            var right = Vector3D.CrossProduct(normal, up);
            var n = normal * dist / 2;
            up *= height / 2;
            right *= length / 2;
            var p1 = center + n - up - right;
            var p2 = center + n - up + right;
            var p3 = center + n + up + right;
            var p4 = center + n + up - right;

            int i0 = mesh.Positions.Count;
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.Positions.Add(p3);
            mesh.Positions.Add(p4);
            if (addNormals)
            {
                mesh.Normals.Add(normal);
                mesh.Normals.Add(normal);
                mesh.Normals.Add(normal);
                mesh.Normals.Add(normal);
            }
            mesh.TextureCoordinates.Add(new Point(1, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point(1, 0));
            mesh.TriangleIndices.Add(i0 + 2);
            mesh.TriangleIndices.Add(i0 + 1);
            mesh.TriangleIndices.Add(i0 + 0);
            mesh.TriangleIndices.Add(i0 + 0);
            mesh.TriangleIndices.Add(i0 + 3);
            mesh.TriangleIndices.Add(i0 + 2);
        }
    }
}
