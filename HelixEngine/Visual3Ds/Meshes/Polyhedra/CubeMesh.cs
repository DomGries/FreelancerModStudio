//-----------------------------------------
// CubeMesh.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class CubeMesh : PolyhedronMeshBase
    {
        static double l = 0.5;

        static readonly Point3D[,] faces = new Point3D[6, 5]
        {
            // front
            { new Point3D(0, 0, l), 
              new Point3D(-l, l, l), new Point3D(-l, -l, l), 
              new Point3D(l, -l, l), new Point3D(l, l, l) },
            // back
            { new Point3D(0, 0, -l),
              new Point3D(l, l, -l), new Point3D(l, -l, -l),
              new Point3D(-l, -l, -l), new Point3D(-l, l, -l) },
            // left
            { new Point3D(-l, 0, 0),
              new Point3D(-l, l, -l), new Point3D(-l, -l, -l),
              new Point3D(-l, -l, l), new Point3D(-l, l, l) },
            // right
            { new Point3D(l, 0, 0),
              new Point3D(l, l, l), new Point3D(l, -l, l),
              new Point3D(l, -l, -l), new Point3D(l, l, -l) },
            // top
            { new Point3D(0, l, 0),
              new Point3D(-l, l, -l), new Point3D(-l, l, l),
              new Point3D(l, l, l), new Point3D(l, l, -l) },
            // bottom
            { new Point3D(0, -l, 0),
              new Point3D(-l, -l, l), new Point3D(-l, -l, -l),
              new Point3D(l, -l, -l), new Point3D(l, -l, l) }
        };

        public CubeMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(0.5, 0.5));
            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(1, 0));

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
            return new CubeMesh();
        }
    }
}
