using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;

namespace HelixEngine
{
    /// <summary>
    /// Builds MeshGeometry3D objects.
    /// </summary>
    /// <remarks>
    /// Performance tips for MeshGeometry3D (http://msdn.microsoft.com/en-us/library/bb613553.aspx)
    ///   <para>
    /// High impact:
    ///     Mesh animation—changing the individual vertices of a mesh on a per-frame basis—is not always efficient in 
    ///     Windows Presentation Foundation (WPF).  To minimize the performance impact of change notifications when 
    ///     each vertex is modified, detach the mesh from the visual tree before performing per-vertex modification.  
    ///     Once the mesh has been modified, reattach it to the visual tree.  Also, try to minimize the size of meshes 
    ///     that will be animated in this way.
    ///   </para>
    /// <para>
    /// Medium impact:
    ///     When a mesh is defined as abutting triangles with shared vertices and those vertices have the same position, 
    ///     normal, and texture coordinates, define each shared vertex only once and then define your triangles by 
    ///     index with TriangleIndices.
    ///   </para>
    /// <para>
    /// Low impact:
    ///     To minimize the construction time of large collections in Windows Presentation Foundation (WPF), 
    ///     such as a MeshGeometry3D’s Positions, Normals, TextureCoordinates, and TriangleIndices, pre-size 
    ///     the collections before value population. If possible, pass the collections’ constructors prepopulated 
    ///     data structures such as arrays or Lists.
    ///   </para>
    /// </remarks>
    public class MeshBuilder
    {
        /// <summary>
        ///   The normals.
        /// </summary>
        internal Vector3DCollection normals;

        /// <summary>
        ///   The positions.
        /// </summary>
        internal Point3DCollection positions;

        /// <summary>
        ///   The texture coordinates.
        /// </summary>
        internal PointCollection textureCoordinates;

        /// <summary>
        ///   The triangle indices.
        /// </summary>
        internal Int32Collection triangleIndices;

        /// <summary>
        /// 'All curves should have the same number of points' exception message.
        /// </summary>
        private const string AllCurvesShouldHaveTheSameNumberOfPoints =
            "All curves should have the same number of points";

        /// <summary>
        /// 'Wrong number of normals' exception message.
        /// </summary>
        private const string WrongNumberOfNormals = "Wrong number of normals.";

        /// <summary>
        /// 'Wrong number of texture coordinates' exception message.
        /// </summary>
        private const string WrongNumberOfTextureCoordinates = "Wrong number of texture coordinates.";

        /// <summary>
        ///   The circle cache.
        /// </summary>
        private static readonly Dictionary<int, IList<Point>> CircleCache = new Dictionary<int, IList<Point>>();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "MeshBuilder" /> class.
        /// </summary>
        /// <remarks>
        ///   Normal and texture coordinate generation are included.
        /// </remarks>
        public MeshBuilder()
            : this(true, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshBuilder"/> class.
        /// </summary>
        /// <param name="generateNormals">
        /// Generate normals.
        /// </param>
        /// <param name="generateTextureCoordinates">
        /// Generate texture coordinates.
        /// </param>
        public MeshBuilder(bool generateNormals, bool generateTextureCoordinates)
        {
            this.positions = new Point3DCollection();
            this.triangleIndices = new Int32Collection();

            if (generateNormals)
            {
                this.normals = new Vector3DCollection();
            }

            if (generateTextureCoordinates)
            {
                this.textureCoordinates = new PointCollection();
            }
        }

        /// <summary>
        /// Gets a circle section (cached).
        /// </summary>
        /// <param name="thetaDiv">
        /// The number of division.
        /// </param>
        /// <returns>
        /// A circle.
        /// </returns>
        public static IList<Point> GetCircle(int thetaDiv)
        {
            IList<Point> circle;
            if (!CircleCache.TryGetValue(thetaDiv, out circle))
            {
                circle = new PointCollection();
                CircleCache.Add(thetaDiv, circle);
                for (int i = 0; i < thetaDiv; i++)
                {
                    double theta = Math.PI * 2 * ((double)i / (thetaDiv - 1));
                    circle.Add(new Point(Math.Cos(theta), -Math.Sin(theta)));
                }
            }

            return circle;
        }

        /// <summary>
        /// Adds an arrow to the mesh.
        /// </summary>
        /// <param name="point1">
        /// The start point.
        /// </param>
        /// <param name="point2">
        /// The end point.
        /// </param>
        /// <param name="diameter">
        /// The diameters of the tube.
        /// </param>
        /// <param name="headLength">
        /// Length of the head (in diameters units).
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the arrow.
        /// </param>
        public void AddArrow(Point3D point1, Point3D point2, double diameter, double headLength, int thetaDiv)
        {
            var dir = point2 - point1;
            double length = dir.Length;
            double r = diameter / 2;

            var pc = new PointCollection
                {
                    new Point(0, 0),
                    new Point(0, r),
                    new Point(length - diameter * headLength, r),
                    new Point(length - diameter * headLength, r * 2),
                    new Point(length, 0)
                };

            this.AddRevolvedGeometry(pc, point1, dir, thetaDiv);
        }

        /// <summary>
        /// Adds a cube face.
        /// </summary>
        /// <param name="center">
        /// The center of the cube.
        /// </param>
        /// <param name="normal">
        /// The normal.
        /// </param>
        /// <param name="up">
        /// The up vector for the face.
        /// </param>
        /// <param name="dist">
        /// The dist from the center of the cube to the face.
        /// </param>
        /// <param name="width">
        /// The width of the face.
        /// </param>
        /// <param name="height">
        /// The height of the face.
        /// </param>
        public void AddCubeFace(Point3D center, Vector3D normal, Vector3D up, double dist, double width, double height)
        {
            var right = Vector3D.CrossProduct(normal, up);
            var n = normal * dist / 2;
            up *= height / 2;
            right *= width / 2;
            var p1 = center + n - up - right;
            var p2 = center + n - up + right;
            var p3 = center + n + up + right;
            var p4 = center + n + up - right;

            int i0 = this.positions.Count;
            this.positions.Add(p1);
            this.positions.Add(p2);
            this.positions.Add(p3);
            this.positions.Add(p4);
            if (this.normals != null)
            {
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
            }

            if (this.textureCoordinates != null)
            {
                this.textureCoordinates.Add(new Point(1, 1));
                this.textureCoordinates.Add(new Point(0, 1));
                this.textureCoordinates.Add(new Point(0, 0));
                this.textureCoordinates.Add(new Point(1, 0));
            }

            this.triangleIndices.Add(i0 + 2);
            this.triangleIndices.Add(i0 + 1);
            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 3);
            this.triangleIndices.Add(i0 + 2);
        }

        /// <summary>
        /// Add the extruded geometry of the specified curve and direction.
        /// </summary>
        /// <param name="points">
        /// The points.
        /// </param>
        /// <param name="xdirection">
        /// The x axis direction.
        /// </param>
        /// <param name="p0">
        /// The p0.
        /// </param>
        /// <param name="p1">
        /// The p1.
        /// </param>
        public void AddExtrudedGeometry(IList<Point> points, Vector3D xdirection, Point3D p0, Point3D p1)
        {
            var ydirection = Vector3D.CrossProduct(xdirection, p1 - p0);
            ydirection.Normalize();
            xdirection.Normalize();

            int index0 = this.positions.Count;
            int np = 2 * points.Count;
            foreach (var p in points)
            {
                var v = xdirection * p.X + ydirection * p.Y;
                this.positions.Add(p0 + v);
                this.positions.Add(p1 + v);
                v.Normalize();
                if (this.normals != null)
                {
                    this.normals.Add(v);
                    this.normals.Add(v);
                }

                if (this.textureCoordinates != null)
                {
                    this.textureCoordinates.Add(new Point(0, 0));
                    this.textureCoordinates.Add(new Point(1, 0));
                }

                int i1 = index0 + 1;
                int i2 = (index0 + 2) % np;
                int i3 = (index0 + 2) % np + 1;

                this.triangleIndices.Add(i1);
                this.triangleIndices.Add(i2);
                this.triangleIndices.Add(index0);

                this.triangleIndices.Add(i1);
                this.triangleIndices.Add(i3);
                this.triangleIndices.Add(i2);
            }
        }

        /// <summary>
        /// Adds the lofted geometry.
        /// </summary>
        /// <param name="positionsList">
        /// List of lofting sections.
        /// </param>
        /// <param name="normalList">
        /// The normal list.
        /// </param>
        /// <param name="textureCoordinateList">
        /// The texture coordinate list.
        /// </param>
        public void AddLoftedGeometry(
            IList<IList<Point3D>> positionsList,
            IList<IList<Vector3D>> normalList,
            IList<IList<Point>> textureCoordinateList)
        {
            //// http://en.wikipedia.org/wiki/Loft_(3D)
            int index0 = this.positions.Count;
            int n = -1;
            for (int i = 0; i < positionsList.Count; i++)
            {
                var pc = positionsList[i];

                // check that all curves have same number of points
                if (n == -1)
                {
                    n = pc.Count;
                }

                if (pc.Count != n)
                {
                    throw new InvalidOperationException(AllCurvesShouldHaveTheSameNumberOfPoints);
                }

                // add the points
                foreach (var p in pc)
                {
                    this.positions.Add(p);
                }

                // add normals 
                if (this.normals != null && normalList != null)
                {
                    var nc = normalList[i];
                    foreach (var normal in nc)
                    {
                        this.normals.Add(normal);
                    }
                }

                // add texcoords
                if (this.textureCoordinates != null && textureCoordinateList != null)
                {
                    var tc = textureCoordinateList[i];
                    foreach (var t in tc)
                    {
                        this.textureCoordinates.Add(t);
                    }
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
                    this.triangleIndices.Add(i0);
                    this.triangleIndices.Add(i1);
                    this.triangleIndices.Add(i2);

                    this.triangleIndices.Add(i2);
                    this.triangleIndices.Add(i3);
                    this.triangleIndices.Add(i0);
                }
            }
        }

        /// <summary>
        /// Add a surface of revolution.
        /// </summary>
        /// <param name="points">
        /// The points (x coordinates are radius, y coordinates are distance from the origin along the axis of revolution)
        /// </param>
        /// <param name="origin">
        /// The origin of the revolution axis.
        /// </param>
        /// <param name="direction">
        /// The direction of the revolution axis.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the mesh.
        /// </param>
        public void AddRevolvedGeometry(IList<Point> points, Point3D origin, Vector3D direction, int thetaDiv)
        {
            ////   http://en.wikipedia.org/wiki/Surface_of_revolution
            direction.Normalize();

            // Find two unit vectors orthogonal to the specified direction
            var u = direction.FindAnyPerpendicular();
            var v = Vector3D.CrossProduct(direction, u);

            u.Normalize();
            v.Normalize();

            var circle = GetCircle(thetaDiv);

            int index0 = this.positions.Count;
            int n = points.Count;

            int totalNodes = (points.Count - 1) * 2 * thetaDiv;
            int rowNodes = (points.Count - 1) * 2;

            for (int i = 0; i < thetaDiv; i++)
            {
                Vector3D w = v * circle[i].X + u * circle[i].Y;

                for (int j = 0; j + 1 < n; j++)
                {
                    // Add segment
                    var q1 = origin + direction * points[j].X + w * points[j].Y;
                    var q2 = origin + direction * points[j + 1].X + w * points[j + 1].Y;

                    // todo:should not add segment if q1==q2 (corner point)
                    // const double eps = 1e-6;
                    // if (Point3D.Subtract(q1, q2).LengthSquared < eps)
                    // continue;
                    double tx = points[j + 1].X - points[j].X;
                    double ty = points[j + 1].Y - points[j].Y;

                    var normal = -direction * ty + w * tx;
                    normal.Normalize();

                    this.positions.Add(q1);
                    this.positions.Add(q2);

                    if (this.normals != null)
                    {
                        this.normals.Add(normal);
                        this.normals.Add(normal);
                    }

                    if (this.textureCoordinates != null)
                    {
                        this.textureCoordinates.Add(new Point((double)i / (thetaDiv - 1), (double)j / (n - 1)));
                        this.textureCoordinates.Add(new Point((double)i / (thetaDiv - 1), (double)(j + 1) / (n - 1)));
                    }

                    int i0 = index0 + i * rowNodes + j * 2;
                    int i1 = i0 + 1;
                    int i2 = index0 + ((i + 1) * rowNodes + j * 2) % totalNodes;
                    int i3 = i2 + 1;

                    this.triangleIndices.Add(i1);
                    this.triangleIndices.Add(i0);
                    this.triangleIndices.Add(i2);

                    this.triangleIndices.Add(i1);
                    this.triangleIndices.Add(i2);
                    this.triangleIndices.Add(i3);
                }
            }
        }

        /// <summary>
        /// Add a sphere.
        /// </summary>
        /// <param name="center">
        /// The center.
        /// </param>
        /// <param name="radius">
        /// The radius.
        /// </param>
        /// <param name="thetaDiv">
        /// The number of divisions around the sphere.
        /// </param>
        /// <param name="phiDiv">
        /// The number of divisions from top to bottom of the sphere.
        /// </param>
        public void AddSphere(Point3D center, double radius, int thetaDiv, int phiDiv)
        {
            int index0 = this.positions.Count;
            double dt = 2 * Math.PI / thetaDiv;
            double dp = Math.PI / phiDiv;

            for (int pi = 0; pi <= phiDiv; pi++)
            {
                double phi = pi * dp;

                for (int ti = 0; ti <= thetaDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    double theta = ti * dt;

                    // Spherical coordinates
                    // http://mathworld.wolfram.com/SphericalCoordinates.html
                    double x = Math.Cos(theta) * Math.Sin(phi);
                    double y = Math.Sin(theta) * Math.Sin(phi);
                    double z = Math.Cos(phi);

                    var p = new Point3D(center.X + radius * x, center.Y + radius * y, center.Z + radius * z);
                    this.positions.Add(p);

                    if (this.normals != null)
                    {
                        var n = new Vector3D(x, y, z);
                        this.normals.Add(n);
                    }

                    if (this.textureCoordinates != null)
                    {
                        var uv = new Point(theta / (2 * Math.PI), phi / Math.PI);
                        this.textureCoordinates.Add(uv);
                    }
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, phiDiv + 1, thetaDiv + 1, true);
        }

        /// <summary>
        /// Adds a triangle.
        /// </summary>
        /// <param name="p0">
        /// The p0.
        /// </param>
        /// <param name="p1">
        /// The p1.
        /// </param>
        /// <param name="p2">
        /// The p2.
        /// </param>
        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            var uv0 = new Point(0, 0);
            var uv1 = new Point(1, 0);
            var uv2 = new Point(0, 1);
            this.AddTriangle(p0, p1, p2, uv0, uv1, uv2);
        }

        /// <summary>
        /// Adds a triangle.
        /// </summary>
        /// <param name="p0">
        /// The p0.
        /// </param>
        /// <param name="p1">
        /// The p1.
        /// </param>
        /// <param name="p2">
        /// The p2.
        /// </param>
        /// <param name="uv0">
        /// The uv0.
        /// </param>
        /// <param name="uv1">
        /// The uv1.
        /// </param>
        /// <param name="uv2">
        /// The uv2.
        /// </param>
        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Point uv0, Point uv1, Point uv2)
        {
            int i0 = this.positions.Count;

            this.positions.Add(p0);
            this.positions.Add(p1);
            this.positions.Add(p2);

            if (this.textureCoordinates != null)
            {
                this.textureCoordinates.Add(uv0);
                this.textureCoordinates.Add(uv1);
                this.textureCoordinates.Add(uv2);
            }

            if (this.normals != null)
            {
                var w = Vector3D.CrossProduct(p1 - p0, p2 - p0);
                w.Normalize();
                this.normals.Add(w);
                this.normals.Add(w);
                this.normals.Add(w);
            }

            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 1);
            this.triangleIndices.Add(i0 + 2);
        }

        /// <summary>
        /// Adds a triangle fan to the mesh
        /// </summary>
        /// <param name="fanPositions">
        /// The triangle fan points.
        /// </param>
        /// <param name="fanNormals">
        /// The normals.
        /// </param>
        /// <param name="fanTextureCoordinates">
        /// The texture coordinates.
        /// </param>
        public void AddTriangleFan(
            IList<Point3D> fanPositions, IList<Vector3D> fanNormals, IList<Point> fanTextureCoordinates)
        {
            if (this.positions == null)
            {
                throw new ArgumentNullException("fanPositions");
            }

            if (this.normals != null && this.normals == null)
            {
                throw new ArgumentNullException("fanNormals");
            }

            if (this.textureCoordinates != null && this.textureCoordinates == null)
            {
                throw new ArgumentNullException("fanTextureCoordinates");
            }

            int index0 = this.positions.Count;
            foreach (var p in fanPositions)
            {
                this.positions.Add(p);
            }

            if (this.textureCoordinates != null && fanTextureCoordinates != null)
            {
                foreach (var tc in fanTextureCoordinates)
                {
                    this.textureCoordinates.Add(tc);
                }
            }

            if (this.normals != null && fanNormals != null)
            {
                foreach (var n in fanNormals)
                {
                    this.normals.Add(n);
                }
            }

            int indexEnd = this.positions.Count;
            for (int i = index0; i + 2 < indexEnd; i++)
            {
                this.triangleIndices.Add(index0);
                this.triangleIndices.Add(i + 1);
                this.triangleIndices.Add(i + 2);
            }
        }

        /// <summary>
        /// Adds a triangle strip to the mesh.
        /// </summary>
        /// <param name="stripPositions">
        /// The points.
        /// </param>
        /// <param name="stripNormals">
        /// The normals.
        /// </param>
        /// <param name="stripTextureCoordinates">
        /// The texture coordinates.
        /// </param>
        public void AddTriangleStrip(
            IList<Point3D> stripPositions,
            IList<Vector3D> stripNormals,
            IList<Point> stripTextureCoordinates)
        {
            // http://en.wikipedia.org/wiki/Triangle_strip
            if (stripPositions == null)
            {
                throw new ArgumentNullException("stripPositions");
            }

            if (this.normals != null && stripNormals == null)
            {
                throw new ArgumentNullException("stripNormals");
            }

            if (this.textureCoordinates != null && stripTextureCoordinates == null)
            {
                throw new ArgumentNullException("stripTextureCoordinates");
            }

            if (stripNormals != null && stripNormals.Count != stripPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (stripTextureCoordinates != null && stripTextureCoordinates.Count != stripPositions.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            // http://en.wikipedia.org/wiki/Triangle_strip
            int index0 = this.positions.Count;
            for (int i = 0; i < stripPositions.Count; i++)
            {
                this.positions.Add(stripPositions[i]);
                if (this.normals != null && stripNormals != null)
                {
                    this.normals.Add(stripNormals[i]);
                }

                if (this.textureCoordinates != null && stripTextureCoordinates != null)
                {
                    this.textureCoordinates.Add(stripTextureCoordinates[i]);
                }
            }

            int indexEnd = this.positions.Count;
            for (int i = index0; i + 2 < indexEnd; i += 2)
            {
                this.triangleIndices.Add(i);
                this.triangleIndices.Add(i + 1);
                this.triangleIndices.Add(i + 2);

                if (i + 3 < indexEnd)
                {
                    this.triangleIndices.Add(i + 1);
                    this.triangleIndices.Add(i + 3);
                    this.triangleIndices.Add(i + 2);
                }
            }
        }

        /// <summary>
        /// <summary>
        /// Converts the geometry to a <see cref="MeshGeometry3D"/>.
        /// </summary>
        /// <param name="freeze">
        /// freeze the mesh if set to <c>true</c>.
        /// </param>
        /// <returns>
        /// A mesh geometry.
        /// </returns>
        public MeshGeometry3D ToMesh(bool freeze)
        {
            if (this.normals != null && this.positions.Count != this.normals.Count)
            {
                throw new InvalidOperationException(WrongNumberOfNormals);
            }

            if (this.textureCoordinates != null && this.positions.Count != this.textureCoordinates.Count)
            {
                throw new InvalidOperationException(WrongNumberOfTextureCoordinates);
            }

            var mg = new MeshGeometry3D
                {
                    Positions = this.positions,
                    TriangleIndices = this.triangleIndices,
                    Normals = this.normals,
                    TextureCoordinates = this.textureCoordinates
                };
            if (freeze)
            {
                mg.Freeze();
            }

            return mg;
        }

        /// <summary>
        /// Add triangle indices for a rectangular mesh.
        /// </summary>
        /// <param name="index0">
        /// The index offset.
        /// </param>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <param name="isSpherical">
        /// set the flag to true to create a sphere mesh (triangles at top and bottom).
        /// </param>
        private void AddRectangularMeshTriangleIndices(int index0, int rows, int columns, bool isSpherical)
        {
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < columns - 1; j++)
                {
                    int ij = i * columns + j;
                    if (!isSpherical || i > 0)
                    {
                        this.triangleIndices.Add(index0 + ij);
                        this.triangleIndices.Add(index0 + ij + 1 + columns);
                        this.triangleIndices.Add(index0 + ij + 1);
                    }

                    if (!isSpherical || i < rows - 2)
                    {
                        this.triangleIndices.Add(index0 + ij + 1 + columns);
                        this.triangleIndices.Add(index0 + ij);
                        this.triangleIndices.Add(index0 + ij + columns);
                    }
                }
            }
        }
    }
}
