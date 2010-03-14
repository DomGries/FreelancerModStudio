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
        static readonly Point3D[,] faces = new Point3D[6, 3]
        {
            // front
            { new Point3D(0.5, 0.5, 0.5), new Point3D(-0.5, 0.5, -0.5), new Point3D(-0.5, 0.5, 0.5) },
            { new Point3D(0.5, 0.5, 0.5), new Point3D(0.5, 0.5, -0.5), new Point3D(-0.5, 0.5, -0.5) },

            // top
            { new Point3D(-0.5, 0.5, 0.5), new Point3D(0, -0.5, 0), new Point3D(0.5, 0.5, 0.5) },

            // left
            { new Point3D(-0.5, 0.5, 0.5), new Point3D(-0.5, 0.5, -0.5), new Point3D(0, -0.5, 0) },

            // bottom
            { new Point3D(-0.5, 0.5, -0.5), new Point3D(0.5, 0.5, -0.5), new Point3D(0, -0.5, 0) },

            // right
            { new Point3D(0.5, 0.5, 0.5), new Point3D(0, -0.5, 0), new Point3D(0.5, 0.5, -0.5) },
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
