//-----------------------------------------------
// OctahedronMesh.cs (c) 2007 by Charles Petzold
//-----------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public class OctahedronMesh : PolyhedronMeshBase
    {
        public static double l = 0.5;
        public static double w = 0.35;

        static readonly Point3D[,] faces = new Point3D[8, 3]
        {
            // front upper right
            { new Point3D(0, w, 0), new Point3D(0, 0, l), new Point3D(w, 0, 0) },

            // front upper left
            { new Point3D(0, w, 0), new Point3D(-w, 0, 0), new Point3D(0, 0, l) },

            // front lower left
            { new Point3D(0, -w, 0), new Point3D(0, 0, l), new Point3D(-w, 0, 0) },

            // front lower right
            { new Point3D(0, -w, 0), new Point3D(w, 0, 0), new Point3D(0, 0, l) },

            // back lower right
            { new Point3D(0, -w, 0), new Point3D(0, 0, -l), new Point3D(w, 0, 0) },

            // back lower left
            { new Point3D(0, -w, 0), new Point3D(-w, 0, 0), new Point3D(0, 0, -l) },

            // back upper left
            { new Point3D(0, w, 0), new Point3D(0, 0, -l), new Point3D(-w, 0, 0) },

            // back upper right
            { new Point3D(0, w, 0), new Point3D(w, 0, 0), new Point3D(0, 0, -l) }
        };

        public OctahedronMesh()
        {
            // Set TextureCoordinates to default values.
            PointCollection textures = TextureCoordinates;
            TextureCoordinates = null;

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(1, 0));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(0, 1));
            textures.Add(new Point(1, 1));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 1));
            textures.Add(new Point(0, 1));

            textures.Add(new Point(0, 0));
            textures.Add(new Point(1, 0));
            textures.Add(new Point(1, 1));

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
            return new OctahedronMesh();
        }
    }
}
