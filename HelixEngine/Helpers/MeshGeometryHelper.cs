using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;

namespace HelixEngine
{
    public class MeshGeometryHelper
    {
        // using a 'helper' class, not extension methods

        // http://en.wikipedia.org/wiki/Geodesic_dome

        // remember to disconnect collections from the MeshGeometry when changing it

        // Optimizing 3D Collections in WPF
        // http://blogs.msdn.com/timothyc/archive/2006/08/31/734308.aspx


        /// <summary>
        /// Adds a triangle strip to the MeshGeometry3D
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="positions1">First list of vertices</param>
        /// <param name="positions2">Second list of vertices</param>
        /// <param name="normals1">List of normals (can be null)</param>
        /// <param name="normals2">List of normals (can be null)</param>
        /// <param name="textureCoordinates1">Texture coordinate (can be null)</param>
        /// <param name="textureCoordinates2">Texture coordinate (can be null)</param>
        public static void AddTriangleStrip(MeshGeometry3D mesh,
            Point3DCollection positions1, Point3DCollection positions2,
            Vector3DCollection normals1, Vector3DCollection normals2,
            PointCollection textureCoordinates1, PointCollection textureCoordinates2)
        {
            // http://en.wikipedia.org/wiki/Triangle_strip

            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            int index0 = positions.Count;
            for (int i = 0; i < positions1.Count || i < positions2.Count; i++)
            {
                if (i < positions1.Count)
                {
                    positions.Add(positions1[i]);
                    if (normals1 != null)
                        normals.Add(normals1[i]);
                    if (textureCoordinates1 != null)
                        textureCoordinates.Add(textureCoordinates1[i]);
                }
                if (i < positions2.Count)
                {
                    positions.Add(positions2[i]);
                    if (normals1 != null)
                        normals.Add(normals1[i]);
                    if (textureCoordinates1 != null)
                        textureCoordinates.Add(textureCoordinates1[i]);
                }
            }
            int indexEnd = positions.Count;
            for (int i = index0; i + 2 < indexEnd; i += 2)
            {
                triangleIndices.Add(i);
                triangleIndices.Add(i + 1);
                triangleIndices.Add(i + 2);

                if (i + 3 < indexEnd)
                {
                    triangleIndices.Add(i + 1);
                    triangleIndices.Add(i + 3);
                    triangleIndices.Add(i + 2);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;

        }

        /// <summary>
        /// Adds a triangle fan to the MeshGeometry3D
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="positions1"></param>
        /// <param name="normals1"></param>
        /// <param name="textureCoordinates1"></param>
        public static void AddTriangleFan(MeshGeometry3D mesh,
            Point3DCollection positions1,
            Vector3DCollection normals1,
            PointCollection textureCoordinates1)
        {
            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            int index0 = positions.Count;
            foreach (var p in positions1)
                positions.Add(p);
            if (normals1 != null)
                foreach (var n in normals1)
                    normals.Add(n);
            if (normals1 != null)
                foreach (var n in normals1)
                    normals.Add(n);

            int indexEnd = positions.Count;
            for (int i = index0 + 1; i + 1 < indexEnd; i++)
            {
                triangleIndices.Add(index0);
                triangleIndices.Add(i + 1);
                triangleIndices.Add(index0);
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
        }

        public static MeshGeometry3D CreateLoftedGeometry(
                        IList<Point3DCollection> positionsList,
            IList<Vector3DCollection> normalList,
            IList<PointCollection> textureCoordinateList)
        {
            var mesh = new MeshGeometry3D();
            AddLoftedGeometry(mesh, positionsList, normalList, textureCoordinateList);
            return mesh;
        }

        public static void AddLoftedGeometry(MeshGeometry3D mesh,
            IList<Point3DCollection> positionsList,
            IList<Vector3DCollection> normalList,
            IList<PointCollection> textureCoordinateList
           )
        {
            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            int index0 = positions.Count;
            int n = -1;
            for (int i = 0; i < positionsList.Count; i++)
            {
                var pc = positionsList[i];

                // check that all curves have same number of points
                if (n == -1)
                    n = pc.Count;
                if (pc.Count != n)
                    throw new InvalidOperationException("All curves should have the same number of points");

                // add the points
                foreach (var p in pc)
                    positions.Add(p);

                // add normals 
                if (normalList != null)
                {
                    var nc = normalList[i];
                    foreach (var normal in nc) normals.Add(normal);
                }

                // add texcoords
                if (textureCoordinateList != null)
                {
                    var tc = textureCoordinateList[i];
                    foreach (var t in tc) textureCoordinates.Add(t);
                }
            }

            for (int i = 0; i + 1 < positionsList.Count; i++)
            {
                for (int j = 0; j + 1 < n; j++)
                {
                    int i0 = index0 + i * n + j;
                    int i1 = i0 + n;
                    int i2 = i1 + 1;
                    int i3 = i0 + 1;
                    triangleIndices.Add(i0);
                    triangleIndices.Add(i1);
                    triangleIndices.Add(i2);

                    triangleIndices.Add(i2);
                    triangleIndices.Add(i3);
                    triangleIndices.Add(i0);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
        }

        // todo: how to chamfer??

        public static void ChamferVertex(MeshGeometry3D mesh, int index)
        {
            throw new NotImplementedException();
        }

        public static void ChamferEdge(MeshGeometry3D mesh, int index0, int index1)
        {
            throw new NotImplementedException();

        }

        public static MeshGeometry3D CreateExtrudedGeometry(PointCollection points, Vector3D dir, Point3D p0, Point3D p1)
        {
            var mesh = new MeshGeometry3D();
            AddExtrudedGeometry(mesh, points, dir, p0, p1);
            return mesh;
        }

        public static void AddExtrudedGeometry(MeshGeometry3D mesh, PointCollection points, Vector3D dir, Point3D p0, Point3D p1)
        {
            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            var dir2 = Vector3D.CrossProduct(dir, p1 - p0);
            dir2.Normalize();
            dir.Normalize();
            int i0 = positions.Count;
            int np = 2 * points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                var v = dir * points[i].X + dir2 * points[i].Y;
                positions.Add(p0 + v);
                positions.Add(p1 + v);
                v.Normalize();
                normals.Add(v);
                normals.Add(v);

                int i1 = i0 + 1;
                int i2 = (i0 + 2) % np;
                int i3 = (i0 + 2) % np + 1;

                triangleIndices.Add(i1);
                triangleIndices.Add(i2);
                triangleIndices.Add(i0);

                triangleIndices.Add(i1);
                triangleIndices.Add(i3);
                triangleIndices.Add(i2);
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
        }

        /// <summary>
        /// Create surface of revolution
        /// </summary>
        public static MeshGeometry3D CreateRevolvedGeometry(PointCollection points, Point3D origin, Vector3D direction, int thetaDiv)
        {
            var mesh = new MeshGeometry3D();
            AddRevolvedGeometry(mesh, points, origin, direction, thetaDiv);
            return mesh;
        }

        public static void AddRevolvedGeometry(MeshGeometry3D mesh, PointCollection points, Point3D origin, Vector3D direction, int thetaDiv)
        {
            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            direction.Normalize();

            // Find two unit vectors orthogonal to the specified direction
            var u = direction.FindAnyPerpendicular();
            var v = Vector3D.CrossProduct(direction, u);

            direction.Normalize();
            u.Normalize();
            v.Normalize();

            int totalNodes = (points.Count - 1) * 2 * thetaDiv;

            for (int i = 0; i < thetaDiv; i++)
            {
                double theta = Math.PI * 2 * ((double)i / (thetaDiv - 1));
                var w = v * Math.Cos(theta) + u * Math.Sin(theta);

                for (int j = 0; j + 1 < points.Count; j++)
                {
                    // Add segment
                    Point3D q1 = origin + direction * points[j].X + w * points[j].Y;
                    Point3D q2 = origin + direction * points[j + 1].X + w * points[j + 1].Y;
                    // todo: don't add segment if q1==q2 (corner point)

                    double tx = points[j + 1].X - points[j].X;
                    double ty = points[j + 1].Y - points[j].Y;

                    Vector3D normal = -direction * ty + w * tx;
                    normal.Normalize();

                    int i0 = positions.Count;
                    positions.Add(q1);
                    positions.Add(q2);
                    if (normals != null)
                    {
                        normals.Add(normal);
                        normals.Add(normal);
                    }

                    int i1 = i0 + 1;
                    int i2 = (i0 + (points.Count - 1) * 2) % totalNodes;
                    int i3 = i2 + 1;

                    triangleIndices.Add(i1);
                    triangleIndices.Add(i2);
                    triangleIndices.Add(i0);

                    triangleIndices.Add(i1);
                    triangleIndices.Add(i3);
                    triangleIndices.Add(i2);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
            Validate(mesh);
        }

        public static void Validate(MeshGeometry3D mesh)
        {
            if (mesh.Normals == null)
                Debug.WriteLine("No normals defined");
            if (mesh.Normals != null && mesh.Normals.Count != 0 && mesh.Normals.Count != mesh.Positions.Count)
                Debug.WriteLine("Wrong number of normals");
            if (mesh.TextureCoordinates == null)
                Debug.WriteLine("No TextureCoordinates defined");
            if (mesh.TextureCoordinates != null && mesh.TextureCoordinates.Count != 0 && mesh.TextureCoordinates.Count != mesh.Positions.Count)
                Debug.WriteLine("Wrong number of TextureCoordinates");
            if (mesh.TriangleIndices.Count % 3 != 0)
                Debug.WriteLine("TriangleIndices not complete");
            for (int i = 0; i < mesh.TriangleIndices.Count; i++)
            {
                int index = mesh.TriangleIndices[i];
                Debug.Assert(index >= 0 || index < mesh.Positions.Count,
                             "Wrong index " + index + " in triangle " + i / 3 + " vertex " + i % 3);
            }
        }

        // todo: there are better ways to tesselate the sphere
        public static MeshGeometry3D CreateSphere(Point3D center, double radius, int tDiv, int pDiv)
        {
            var mesh = new MeshGeometry3D();

            // disconnect the collections while we change...
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            double dt = 2 * Math.PI / tDiv;
            double dp = Math.PI / pDiv;


            for (int pi = 0; pi <= pDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    // Spherical coordinates
                    // http://mathworld.wolfram.com/SphericalCoordinates.html
                    double x = Math.Cos(theta) * Math.Sin(phi);
                    double y = Math.Sin(theta) * Math.Sin(phi);
                    double z = Math.Cos(phi);
                    var p = new Point3D(center.X + radius * x, center.Y + radius * y, center.Z + radius * z);
                    var n = new Vector3D(x, y, z);
                    var uv = new Point(theta / (2 * Math.PI), phi / (Math.PI));

                    positions.Add(p);
                    normals.Add(n);
                    textureCoordinates.Add(uv);
                }
            }

            for (int pi = 0; pi < pDiv; pi++)
            {
                for (int ti = 0; ti < tDiv; ti++)
                {
                    int x0 = ti;
                    int x1 = (ti + 1);
                    int y0 = pi * (tDiv + 1);
                    int y1 = (pi + 1) * (tDiv + 1);

                    triangleIndices.Add(x0 + y0);
                    triangleIndices.Add(x0 + y1);
                    triangleIndices.Add(x1 + y0);

                    triangleIndices.Add(x1 + y0);
                    triangleIndices.Add(x0 + y1);
                    triangleIndices.Add(x1 + y1);
                }
            }

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;

            return mesh;
        }

        public static void AddBox(MeshGeometry3D mesh, Point3D center, double width, double length, double height)
        {
            mesh.AddCubeFace(center, new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), width, length, height);
            mesh.AddCubeFace(center, new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1), length, width, height);
            mesh.AddCubeFace(center, new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), length, width, height);
            mesh.AddCubeFace(center, new Vector3D(0, -1, 0), new Vector3D(0, 0, 1), width, length, height);
            mesh.AddCubeFace(center, new Vector3D(0, 0, 1), new Vector3D(0, -1, 0), height, length, width);
            mesh.AddCubeFace(center, new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), height, length, width);
        }

        public static Point3D ProjectTo3D(Point3D point, double theta, double phi)
        {
            theta *= 0.01745;
            phi *= 0.01745;
            double projX = point.X * Math.Cos(theta) * Math.Sin(phi) +
                           point.Y * Math.Sin(theta) * Math.Sin(phi) +
                           point.Z * Math.Cos(phi);

            double projY = -point.X * Math.Sin(theta) + point.Y * Math.Cos(theta);
            double projZ = -point.X * Math.Cos(theta) * Math.Cos(phi) -
                           point.Y * Math.Sin(theta) * Math.Cos(phi) +
                           point.Z * Math.Sin(phi);

            return new Point3D(projX, projY, projZ);
        }

        public static Point3DCollection RotatePoints(Point3DCollection points, double theta, double phi)
        {
			for (int i = 0; i < points.Count; i++)
			{
				Point3D point = ProjectTo3D(points[i], theta, phi);
				points[i] = point;
			}
            return points;
        }
    }
}
