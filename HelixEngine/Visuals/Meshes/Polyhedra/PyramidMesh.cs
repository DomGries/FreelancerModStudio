//------------------------------------------------
// TetrahedronMesh.cs (c) 2007 by Charles Petzold
//------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class PyramidMesh : PolyhedronMeshBase
    {
        static double l = 0.5;

        static readonly Point3D[,] faces = new Point3D[6, 3]
        {
            // front
            { new Point3D(l, l, l), new Point3D(-l, l, -l), new Point3D(-l, l, l) },
            { new Point3D(l, l, l), new Point3D(l, l, -l), new Point3D(-l, l, -l) },

            // top
            { new Point3D(-l, l, l), new Point3D(0, -l, 0), new Point3D(l, l, l) },

            // left
            { new Point3D(-l, l, l), new Point3D(-l, l, -l), new Point3D(0, -l, 0) },

            // bottom
            { new Point3D(-l, l, -l), new Point3D(l, l, -l), new Point3D(0, -l, 0) },

            // right
            { new Point3D(l, l, l), new Point3D(0, -l, 0), new Point3D(l, l, -l) },
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
            return new PyramidMesh();
        }
    }
}
