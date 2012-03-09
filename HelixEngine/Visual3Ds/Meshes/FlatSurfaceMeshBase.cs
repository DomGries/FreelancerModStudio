//----------------------------------------------------
// FlatSurfaceMeshBase.cs (c) 2007 by Charles Petzold
//----------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public abstract class FlatSurfaceMeshBase : MeshGeneratorBase
    {
        // Precreated arrayes used in the TriangleSubdivide method.
        Point3D[] verticesBase = new Point3D[3];
        Vector3D[] normalsBase = new Vector3D[3];
        int[] indicesBase = new int[3];
        Point[] texturesBase = new Point[3];

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(FlatSurfaceMeshBase),
                new PropertyMetadata(1, PropertyChanged), 
                ValidateSlices);

        /// <summary>
        /// 
        /// </summary>
        public int Slices
        {
            set { SetValue(SlicesProperty, value); }
            get { return (int)GetValue(SlicesProperty); }
        }

        static bool ValidateSlices(object obj)
        {
            return (int)obj > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
        protected void TriangleSubdivide(Point3DCollection vertices,
                                         Vector3DCollection normals,
                                         Int32Collection indices,
                                         PointCollection textures)
        {
            for (int i = 0; i < 3; i++)
            {
                verticesBase[2 - i] = vertices[vertices.Count - 1];
                normalsBase[2 - i] = normals[vertices.Count - 1];
                texturesBase[2 - i] = textures[vertices.Count - 1];

                vertices.RemoveAt(vertices.Count - 1);
                normals.RemoveAt(normals.Count - 1);
                indices.RemoveAt(indices.Count - 1);
                textures.RemoveAt(textures.Count - 1);
            }

            int indexStart = vertices.Count;

            for (int slice = 0; slice <= Slices; slice++)
            {
                double weight = (double)slice / Slices;

                Point3D vertex1 = Point3DWeight(verticesBase[0], verticesBase[1], weight);
                Point3D vertex2 = Point3DWeight(verticesBase[0], verticesBase[2], weight);

                Vector3D normal1 = Vector3DWeight(normalsBase[0], normalsBase[1], weight);
                Vector3D normal2 = Vector3DWeight(normalsBase[0], normalsBase[2], weight);

                Point texture1 = PointWeight(texturesBase[0], texturesBase[1], weight);
                Point texture2 = PointWeight(texturesBase[0], texturesBase[2], weight);

                for (int i = 0; i <= slice; i++)
                {
                    weight = (double)i / slice;

                    if (Double.IsNaN(weight))
                        weight = 0;

                    vertices.Add(Point3DWeight(vertex1, vertex2, weight));
                    normals.Add(Vector3DWeight(normal1, normal2, weight));
                    textures.Add(PointWeight(texture1, texture2, weight));
                }
            }

            for (int slice = 0; slice < Slices; slice++)
            {
                int base1 = (slice + 1) * slice / 2;
                int base2 = base1 + slice + 1;

                for (int i = 0; i <= 2 * slice; i++)
                {
                    int half = i / 2;

                    if ((i & 1) == 0)         // even
                    {
                        indices.Add(indexStart + base1 + half);
                        indices.Add(indexStart + base2 + half);
                        indices.Add(indexStart + base2 + half + 1);
                    }
                    else                    // odd
                    {
                        indices.Add(indexStart + base1 + half);
                        indices.Add(indexStart + base2 + half + 1);
                        indices.Add(indexStart + base1 + half + 1);
                    }
                }
            }
        }

        Point3D Point3DWeight(Point3D one, Point3D two, double wt2)
        {
            double wt1 = 1 - wt2;
            return new Point3D(wt1 * one.X + wt2 * two.X,
                               wt1 * one.Y + wt2 * two.Y,
                               wt1 * one.Z + wt2 * two.Z);
        }

        Vector3D Vector3DWeight(Vector3D one, Vector3D two, double wt2)
        {
            double wt1 = 1 - wt2;
            return new Vector3D(wt1 * one.X + wt2 * two.X,
                                wt1 * one.Y + wt2 * two.Y,
                                wt1 * one.Z + wt2 * two.Z);
        }

        Point PointWeight(Point one, Point two, double wt2)
        {
            double wt1 = 1 - wt2;
            return new Point(wt1 * one.X + wt2 * two.X,
                             wt1 * one.Y + wt2 * two.Y);
        }
    }
}
