//---------------------------------------------
// CylinderMesh.cs (c) 2007 by Charles Petzold
//---------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a cylinder.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a 
    ///     CylinderMesh object with multiple 3D visuals. 
    ///     In XAML files, the CylinderMesh
    ///     tag will probably appear in a resource section.
    ///     The cylinder is centered on the positive Y axis.
    /// </remarks>
    public class CylinderMesh : CylindricalMeshBase
    {
        /// <summary>
        ///     Initializes a new instance of the CylinderMesh class.
        /// </summary>
        public CylinderMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the EndStacks dependency property.
        /// </summary>
        public static readonly DependencyProperty EndStacksProperty =
            DependencyProperty.Register("EndStacks",
                typeof(int),
                typeof(CylinderMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateEndStacks);

        // Validation callback for EndStacks.
        static bool ValidateEndStacks(object obj)
        {
            return (int)obj > 0;
        }

        /// <summary>
        ///     Gets or sets the number of radial divisions on each end of 
        ///     the cylinder.
        /// </summary>
        /// <value>
        ///     The number of radial divisions on the end of the cylinder. 
        ///     This property must be at least 1, which is also the default value. 
        /// </value>
        /// <remarks>
        ///     The default value of 1 is appropriate in many cases. 
        ///     However, if PointLight or SpotLight objects are applied to the
        ///     cylinder, or if non-linear transforms are used to deform
        ///     the figure, you should set EndStacks to a higher value.
        /// </remarks>
        public int EndStacks
        {
            set { SetValue(EndStacksProperty, value); }
            get { return (int)GetValue(EndStacksProperty); }
        }

        /// <summary>
        ///     Identifies the Fold dependency property.
        /// </summary>
        public static readonly DependencyProperty FoldProperty =
            DependencyProperty.Register("Fold",
                typeof(double),
                typeof(CylinderMesh),
                new PropertyMetadata(1.0 / 3, PropertyChanged),
                ValidateFold);

        // Validation callback for Fold.
        static bool ValidateFold(object obj)
        {
            return (double)obj < 0.5;
        }

        /// <summary>
        ///     Gets or sets the fraction of the brush that appears on
        ///     the top and bottom ends of the cylinder.
        /// </summary>
        /// <value>
        ///     The fraction of the brush that folds over the top and
        ///     bottom ends of the cylinder. The default is 1/3. The
        ///     property cannot be greater than 1/2.
        /// </value>
        public double Fold
        {
            set { SetValue(FoldProperty, value); }
            get { return (double)GetValue(FoldProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
        protected override void Triangulate(DependencyPropertyChangedEventArgs args,
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

            // Begin at the top end. Fill the collections.
            for (int stack = 0; stack <= EndStacks; stack++)
            {
                double y = Length;
                double radius = stack * Radius / EndStacks;
                int top = (stack + 0) * (Slices + 1);
                int bot = (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices;
                    double x = -radius * Math.Sin(theta);
                    double z = -radius * Math.Cos(theta);

                    vertices.Add(new Point3D(x, y, z));
                    normals.Add(new Vector3D(0, 1, 0));
                    textures.Add(new Point((double)slice / Slices,
                                           Fold * stack / EndStacks));

                    if (stack < EndStacks && slice < Slices)
                    {
                        if (stack != 0)
                        {
                            indices.Add(top + slice);
                            indices.Add(bot + slice);
                            indices.Add(top + slice + 1);
                        }

                        indices.Add(top + slice + 1);
                        indices.Add(bot + slice);
                        indices.Add(bot + slice + 1);
                    }
                }
            }

            int offset = vertices.Count;

            // Length of the cylinder: Fill in the collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                double y = Length - stack * Length / Stacks;
                int top = offset + (stack + 0) * (Slices + 1);
                int bot = offset + (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices;
                    double x = -Radius * Math.Sin(theta);
                    double z = -Radius * Math.Cos(theta);

                    vertices.Add(new Point3D(x, y, z));
                    normals.Add(new Vector3D(x, 0, z));
                    textures.Add(new Point((double)slice / Slices,
                                           Fold + (1 - 2 * Fold) * stack / Stacks));

                    if (stack < Stacks && slice < Slices)
                    {
                        indices.Add(top + slice);
                        indices.Add(bot + slice);
                        indices.Add(top + slice + 1);

                        indices.Add(top + slice + 1);
                        indices.Add(bot + slice);
                        indices.Add(bot + slice + 1);
                    }
                }
            }

            offset = vertices.Count;

            // Finish with the bottom end. Fill the collections.
            for (int stack = 0; stack <= EndStacks; stack++)
            {
                double y = 0;
                double radius = (EndStacks - stack) * Radius / EndStacks;
                int top = offset + (stack + 0) * (Slices + 1);
                int bot = offset + (stack + 1) * (Slices + 1);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices;
                    double x = -radius * Math.Sin(theta);
                    double z = -radius * Math.Cos(theta);

                    vertices.Add(new Point3D(x, y, z));
                    normals.Add(new Vector3D(0, -1, 0));
                    textures.Add(new Point((double)slice / Slices,
                                           (1 - Fold) + Fold * stack / EndStacks));

                    if (stack < EndStacks && slice < Slices)
                    {
                        indices.Add(top + slice);
                        indices.Add(bot + slice);
                        indices.Add(top + slice + 1);

                        if (stack != EndStacks - 1)
                        {
                            indices.Add(top + slice + 1);
                            indices.Add(bot + slice);
                            indices.Add(bot + slice + 1);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a new instance of the CylinderMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of CylinderMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new CylinderMesh();
        }
    }
}

