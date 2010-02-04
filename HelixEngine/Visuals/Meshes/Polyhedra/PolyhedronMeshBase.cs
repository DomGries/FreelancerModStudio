//---------------------------------------------------
// PolyhedronMeshBase.cs (c) 2007 by Charles Petzold
//---------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    public abstract class PolyhedronMeshBase : FlatSurfaceMeshBase
    {
        /// <summary>
        /// 
        /// </summary>
        public PolyhedronMeshBase()
        {
            TextureCoordinates = TextureCoordinates.Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty TextureCoordinatesProperty =
            MeshGeometry3D.TextureCoordinatesProperty.AddOwner(
                typeof(PolyhedronMeshBase),
                new PropertyMetadata(PropertyChanged));
        
        /// <summary>
        /// 
        /// </summary>
        public PointCollection TextureCoordinates
        {
            set { SetValue(TextureCoordinatesProperty, value); }
            get { return (PointCollection)GetValue(TextureCoordinatesProperty); }
        }

        protected abstract Point3D[,] Faces
        {
            get;
        }

        protected override void Triangulate(DependencyPropertyChangedEventArgs args, 
                                            Point3DCollection vertices, 
                                            Vector3DCollection normals, 
                                            Int32Collection indices, 
                                            PointCollection textures)
        {
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            textures.Clear();

            Point3D[,] faces = Faces;
            PointCollection texturesBase = TextureCoordinates;
            int indexTextures = 0;

            for (int face = 0; face < faces.GetLength(0); face++)
            {
                Vector3D normal = Vector3D.CrossProduct(faces[face, 2] - faces[face, 0],
                                                        faces[face, 1] - faces[face, 0]);

                // For faces that are triangles.
                if (faces.GetLength(1) == 3)
                {
                    int indexBase = vertices.Count;

                    for (int i = 0; i < 3; i++)
                    {
                        vertices.Add(faces[face, i]);
                        normals.Add(normal);
                        indices.Add(indexBase + i);

                        if (texturesBase != null && texturesBase.Count > 0)
                        {
                            textures.Add(texturesBase[indexTextures]);
                            indexTextures = (indexTextures + 1) % texturesBase.Count;
                        }
                    }

                    if (Slices > 1)
                        TriangleSubdivide(vertices, normals, indices, textures);
                }

                // For faces that are not triangles.
                else
                {
                    for (int i = 0; i < faces.GetLength(1) - 1; i++)
                    {
                        int indexBase = vertices.Count;
                        int num = faces.GetLength(1) - 1;

                        vertices.Add(faces[face, 0]);
                        vertices.Add(faces[face, i + 1]);
                        vertices.Add(faces[face, (i + 1) % num + 1]);

                        if (texturesBase != null && texturesBase.Count >= faces.GetLength(1))
                        {
                            textures.Add(texturesBase[indexTextures + 0]);
                            textures.Add(texturesBase[indexTextures + i + 1]);
                            textures.Add(texturesBase[indexTextures + (i + 1) % num + 1]);
                        }

                        normals.Add(normal);
                        normals.Add(normal);
                        normals.Add(normal);

                        indices.Add(indexBase + 0);
                        indices.Add(indexBase + 1);
                        indices.Add(indexBase + 2);

                        if (Slices > 1)
                            TriangleSubdivide(vertices, normals, indices, textures);
                    }
                    if (texturesBase != null && texturesBase.Count > 0)
                        indexTextures = (indexTextures + faces.GetLength(1)) % texturesBase.Count;
                }
            }
        }
    }
}

