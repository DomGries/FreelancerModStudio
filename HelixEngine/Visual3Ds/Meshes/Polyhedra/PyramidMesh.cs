using System.Windows;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class PyramidMesh : PolyhedronMeshBase
    {
        const double r = 0.5;

        static readonly Point3D[,] faces = new Point3D[6, 3]
        {
            // back
            { new Point3D(r, -r, -r), new Point3D(r, -r, r), new Point3D(-r, -r, -r) },
            { new Point3D(r, -r, r), new Point3D(-r, -r, r), new Point3D(-r, -r, -r) },

            // top
            { new Point3D(0, r, 0), new Point3D(-r, -r, r), new Point3D(r, -r, r) },

            // left
            { new Point3D(-r, -r, r), new Point3D(0, r, 0), new Point3D(-r, -r, -r) },

            // bottom
            { new Point3D(-r, -r, -r), new Point3D(0, r, 0), new Point3D(r, -r, -r) },

            // right
            { new Point3D(0, r, 0), new Point3D(r, -r, r), new Point3D(r, -r, -r) },
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
