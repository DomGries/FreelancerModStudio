//------------------------------------------------
// TetrahedronMesh.cs (c) 2007 by Charles Petzold
//------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class Surface2SidedMesh : PolyhedronMeshBase
    {
        static double l = 0.5;

        static readonly Point3D[,] faces = new Point3D[4, 3]
        {
            // front
            { new Point3D(l, l, 0), new Point3D(-l, l, 0), new Point3D(-l, -l, 0) },
            { new Point3D(l, l, 0), new Point3D(-l, -l, 0), new Point3D(l, -l, 0) },

            // bottom
            { new Point3D(-l, -l, 0), new Point3D(-l, l, 0), new Point3D(l, l, 0) },
            { new Point3D(l, -l, 0), new Point3D(-l, -l, 0), new Point3D(l, l, 0) },
        };

        protected override Point3D[,] Faces
        {
            get
            {
                return faces;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Surface2SidedMesh();
        }
    }
    public class SurfaceMesh : PolyhedronMeshBase
    {
        static double l = 0.5;

        static readonly Point3D[,] faces = new Point3D[4, 3]
        {
            // front
            { new Point3D(l, l, 0), new Point3D(-l, l, 0), new Point3D(-l, -l, 0) },
            { new Point3D(l, l, 0), new Point3D(-l, -l, 0), new Point3D(l, -l, 0) },

            // bottom
            { new Point3D(-l, -l, 0), new Point3D(-l, l, 0), new Point3D(l, l, 0) },
            { new Point3D(l, -l, 0), new Point3D(-l, -l, 0), new Point3D(l, l, 0) },
        };

        protected override Point3D[,] Faces
        {
            get
            {
                return faces;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new SurfaceMesh();
        }
    }
}
