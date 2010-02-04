//------------------------------------------------
// TetrahedronMesh.cs (c) 2007 by Charles Petzold
//------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class TetrahedronMesh : PolyhedronMeshBase
    {
        static readonly Point3D[,] faces = new Point3D[4, 3]
        {
            // upper-left front
            { new Point3D(-1, 1, -1), new Point3D(-1, -1, 1), new Point3D(1, 1, 1) },

            // lower-right front 
            { new Point3D(1, -1, -1), new Point3D(1, 1, 1), new Point3D(-1, -1, 1) },

            // upper-right back
            { new Point3D(1, 1, 1), new Point3D(1, -1, -1), new Point3D(-1, 1, -1) },

            // lower-left back
            { new Point3D(-1, -1, 1), new Point3D(-1, 1, -1), new Point3D(1, -1, -1) }
        };

        public TetrahedronMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
            textures.Add(new Point(1, 0));

            textures.Add(new Point(1, 1));
            textures.Add(new Point(1, 0));
            textures.Add(new Point(0, 1));

            TextureCoordinates = textures;
        }

        protected override Point3D[,] Faces
        {
            get
            {
                return faces;
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TetrahedronMesh();
        }
    }
}
