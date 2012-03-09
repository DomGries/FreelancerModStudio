//---------------------------------------------------
// HollowCylinderMesh.cs (c) 2007 by Charles Petzold
//---------------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a hollow cylinder.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a 
    ///     HollowCylinderMesh object with multiple 3D visuals. 
    ///     In XAML files, the HollowCylinderMesh
    ///     tag will probably appear in a resource section.
    ///     The cylinder is centered on the positive Y axis.
    /// </remarks>
    public class HollowCylinderMesh : CylindricalMeshBase
    {
        /// <summary>
        ///     Initializes a new instance of the HollowCylinderMesh class.
        /// </summary>
        public HollowCylinderMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
        protected override void Triangulate(
                                    DependencyPropertyChangedEventArgs args,
                                    Point3DCollection vertices,
                                    Vector3DCollection normals,
                                    Int32Collection indices,
                                    PointCollection textures)
        {
            // Clear all four collections.
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            textures.Clear();

            // Fill the vertices, normals, and textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                double y = Length - stack * Length / Stacks;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices;
                    double x = -Radius * Math.Sin(theta);
                    double z = -Radius * Math.Cos(theta);

                    normals.Add(new Vector3D(x, 0, z));
                    vertices.Add(new Point3D(x, y, z));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add((stack + 0) * (Slices + 1) + slice);
                    indices.Add((stack + 1) * (Slices + 1) + slice);
                    indices.Add((stack + 0) * (Slices + 1) + slice + 1);

                    indices.Add((stack + 0) * (Slices + 1) + slice + 1);
                    indices.Add((stack + 1) * (Slices + 1) + slice);
                    indices.Add((stack + 1) * (Slices + 1) + slice + 1);
                }
            }
        }

        /// <summary>
        ///     Creates a new instance of the HollowCylinderMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of HollowCylinderMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new HollowCylinderMesh();
        }
    }
}

