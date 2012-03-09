//------------------------------------------
// TorusMesh.cs (c) 2007 by Charles Petzold
//------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a torus.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a TorusMesh
    ///     object with multiple 3D visuals. In XAML files, the TorusMesh
    ///     tag will probably appear in a resource section.
    /// </remarks>
    public class TorusMesh : MeshGeneratorBase
    {
        /// <summary>
        ///     Initializes a new instance of the TorusMesh class.
        /// </summary>
        public TorusMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the Radius dependency property.
        /// </summary>
        /// <value>
        ///     The radius of the torus in world units.
        ///     The default is 1. 
        ///     The Radius property can be set to a negative value,
        ///     but in effect the torus is turned inside out so that
        ///     the surface of the torus is colored with the BackMaterial
        ///     brush rather than the Material brush.
        /// </value>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius",
                typeof(double),
                typeof(TorusMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the radius of the torus.
        /// </summary>
        public double Radius
        {
            set { SetValue(RadiusProperty, value); }
            get { return (double)GetValue(RadiusProperty); }
        }

        /// <summary>
        ///     Identifies the TubeRadius dependency property.
        /// </summary>
        /// <value>
        ///     The identifier for the TubeRadius dependency property.
        /// </value>
        public static readonly DependencyProperty TubeRadiusProperty =
            DependencyProperty.Register("TubeRadius",
                typeof(double),
                typeof(TorusMesh),
                new PropertyMetadata(0.25, PropertyChanged));

        /// <summary>
        ///     Gets or sets the tube radius of the torus.
        /// </summary>
        /// <value>
        ///     The radius of the tube that makes up the torus.
        ///     The default is 0.25.
        /// </value>
        public double TubeRadius
        {
            set { SetValue(TubeRadiusProperty, value); }
            get { return (double)GetValue(TubeRadiusProperty); }
        }

        /// <summary>
        ///     Identifies the Slices dependency property.
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(TorusMesh),
                new PropertyMetadata(18, PropertyChanged),
                ValidateSlices);

        // Validation callback for Slices.
        static bool ValidateSlices(object obj)
        {
            return (int)obj > 2;
        }

        /// <summary>
        ///     Gets or sets the number of divisions around the torus tube.
        /// </summary>
        /// <value>
        ///     The number of divisions around the torus tube. 
        ///     This property must be at least 3. 
        ///     The default value is 18.
        /// </value>
        /// <remarks>
        ///     Each slice is equivalent to a number
        ///     of degrees around the torus tube equal to 360 divided by the 
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
                typeof(TorusMesh),
                new PropertyMetadata(36, PropertyChanged),
                ValidateStacks);

        // Validation callback for Stacks.
        static bool ValidateStacks(object obj)
        {
            return (int)obj > 2;
        }

        /// <summary>
        ///     Gets or sets the number of divisions around the
        ///     entire torus.
        /// </summary>
        /// <value>
        ///     This property must be at least 3. 
        ///     The default value is 36.
        /// </value>
        /// <remarks>
        /// </remarks>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
        }
/*
        // Static method called for any property change.
        static void PropertyChanged(DependencyObject obj,
                                    DependencyPropertyChangedEventArgs args)
        {
            (obj as TorusMesh).PropertyChanged(args);
        }
 */
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
                double phi = stack * 2 * Math.PI / Stacks;

                double xCenter = Radius * Math.Sin(phi);
                double yCenter = Radius * Math.Cos(phi);
                Point3D pointCenter = new Point3D(xCenter, yCenter, 0);

                for (int slice = 0; slice <= Slices; slice++)
                {
                    double theta = slice * 2 * Math.PI / Slices + Math.PI;
                    double x = (Radius + TubeRadius * Math.Cos(theta)) * Math.Sin(phi);
                    double y = (Radius + TubeRadius * Math.Cos(theta)) * Math.Cos(phi);
                    double z = -TubeRadius * Math.Sin(theta);
                    Point3D point = new Point3D(x, y, z);

                    vertices.Add(point);
                    normals.Add(point - pointCenter);
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
        ///     Creates a new instance of the TorusMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of TorusMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new TorusMesh();
        }
    }
}

