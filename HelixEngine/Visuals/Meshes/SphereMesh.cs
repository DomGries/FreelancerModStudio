//-------------------------------------------
// SphereMesh.cs (c) 2007 by Charles Petzold
//-------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a sphere.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a SphereMesh
    ///     object with multiple 3D visuals. In XAML files, the SphereMesh
    ///     tag will probably appear in a resource section.
    /// </remarks>
    public class SphereMesh : MeshGeneratorBase
    {
        /// <summary>
        ///     Initializes a new instance of the SphereMesh class.
        /// </summary>
        public SphereMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the Radius dependency property.
        /// </summary>
        /// <value>
        ///     The radius of the sphere in world units.
        ///     The default is 1. 
        ///     The Radius property can be set to a negative value,
        ///     but in effect the sphere is turned inside out so that
        ///     the surface of the sphere is colored with the BackMaterial
        ///     brush rather than the Material brush.
        /// </value>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius",
                typeof(double),
                typeof(SphereMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the radius of the sphere.
        /// </summary>
        public double Radius
        {
            set { SetValue(RadiusProperty, value); }
            get { return (double)GetValue(RadiusProperty); }
        }

        /// <summary>
        ///     Identifies the Slices dependency property.
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(SphereMesh),
                new PropertyMetadata(36, PropertyChanged),
                ValidateSlices);

        // Validation callback for Slices.
        static bool ValidateSlices(object obj)
        {
            return (int)obj > 2;
        }

        /// <summary>
        ///     Gets or sets the number of divisions around the Y axis
        ///     used to approximate the sphere.
        /// </summary>
        /// <value>
        ///     The number of divisions around the Y axis. 
        ///     This property must be at least 3. 
        ///     The default value is 36.
        /// </value>
        /// <remarks>
        ///     If the sphere is pictured as a globe with the north pole 
        ///     to the top, the Slices property divides the sphere along 
        ///     lines of longitude. Each slice is equivalent to a number
        ///     of degrees of longitude equal to 360 divided by the 
        ///     Slices value.
        /// </remarks>
        public int Slices
        {
            set { SetValue(SlicesProperty, value); }
            get { return (int)GetValue(SlicesProperty); }
        }

        /// <summary>
        ///     Identifies the Stacks dependency property.
        /// </summary>
        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks",
                typeof(int),
                typeof(SphereMesh),
                new PropertyMetadata(18, PropertyChanged),
                ValidateStacks);

        // Validation callback for Stacks.
        static bool ValidateStacks(object obj)
        {
            return (int)obj > 1;
        }

        /// <summary>
        ///     Gets or sets the number of divisions parallel to the XZ
        ///     plane used to approximate the sphere.
        /// </summary>
        /// <value>
        ///     The number of divisions parallel to the XZ plane. 
        ///     This property must be at least 2. 
        ///     The default value is 18.
        /// </value>
        /// <remarks>
        ///     If the sphere is pictured as a globe with the north pole 
        ///     to the top, the Stacks property divides the sphere along 
        ///     lines of latitude. Each stack is equivalent to a number
        ///     of degrees of latitude equal to 180 divided by the 
        ///     Stacks value.
        /// </remarks>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
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

            // Fill the vertices, normals, and textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                double phi = Math.PI / 2 - stack * Math.PI / Stacks;
                double y = Radius * Math.Sin(phi);
                double scale = -Radius * Math.Cos(phi);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices;
                    double x = scale * Math.Sin(theta);
                    double z = scale * Math.Cos(theta);

                    Vector3D normal = new Vector3D(x, y, z);
                    normals.Add(normal);
                    vertices.Add(normal.ToPoint3D());
                    textures.Add(new Point((double)slice / Slices,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    if (stack != 0)
                    {
                        indices.Add((stack + 0) * (Slices + 1) + slice);
                        indices.Add((stack + 1) * (Slices + 1) + slice);
                        indices.Add((stack + 0) * (Slices + 1) + slice + 1);
                    }

                    if (stack != Stacks - 1)
                    {
                        indices.Add((stack + 0) * (Slices + 1) + slice + 1);
                        indices.Add((stack + 1) * (Slices + 1) + slice);
                        indices.Add((stack + 1) * (Slices + 1) + slice + 1);
                    }
                }
            }
        }

        /// <summary>
        ///     Creates a new instance of the SphereMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of SphereMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new SphereMesh();
        }
    }
}

