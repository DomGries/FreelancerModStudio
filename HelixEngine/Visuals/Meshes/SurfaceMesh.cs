//----------------------------------------
// BoxMesh.cs (c) 2007 by Charles Petzold
//----------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Generates a MeshGeometry3D object for a box centered on the origin.
    /// </summary>
    /// <remarks>
    ///     The MeshGeometry3D object this class creates is available as the
    ///     Geometry property. You can share the same instance of a BoxMesh
    ///     object with multiple 3D visuals. In XAML files, the BoxMesh
    ///     tag will probably appear in a resource section.
    /// </remarks>
    public class SurfaceMesh : MeshGeneratorBase
    {
        /// <summary>
        ///     Initializes a new instance of the BoxMesh class.
        /// </summary>
        public SurfaceMesh()
        {
            PropertyChanged(new DependencyPropertyChangedEventArgs());
        }

        /// <summary>
        ///     Identifies the Width dependency property.
        /// </summary>
        /// <value>
        ///     The width of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width",
                typeof(double),
                typeof(SurfaceMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the width of the box.
        /// </summary>
        public double Width
        {
            set { SetValue(WidthProperty, value); }
            get { return (double)GetValue(WidthProperty); }
        }

        /// <summary>
        ///     Identifies the Height dependency property.
        /// </summary>
        /// <value>
        ///     The height of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height",
                typeof(double),
                typeof(SurfaceMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the height of the box.
        /// </summary>
        public double Height
        {
            set { SetValue(HeightProperty, value); }
            get { return (double)GetValue(HeightProperty); }
        }

        /// <summary>
        ///     Identifies the Depth dependency property.
        /// </summary>
        /// <value>
        ///     The depth of the box in world units.
        ///     The default is 1. 
        /// </value>
        public static readonly DependencyProperty DepthProperty =
            DependencyProperty.Register("Depth",
                typeof(double),
                typeof(SurfaceMesh),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the depth of the box.
        /// </summary>
        public double Depth
        {
            set { SetValue(DepthProperty, value); }
            get { return (double)GetValue(DepthProperty); }
        }

        /// <summary>
        ///     Identifies the Slices dependency property.
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(SurfaceMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateDivisions);

        /// <summary>
        ///     Gets or sets the number of divisions across the box width.
        /// </summary>
        /// <value>
        ///     The number of divisions across the box width. 
        ///     This property must be at least 1. 
        ///     The default value is 1.
        /// </value>
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
                typeof(SurfaceMesh),
                new PropertyMetadata(1, PropertyChanged),
                ValidateDivisions);

        /// <summary>
        ///     Gets or sets the number of divisions in the box height.
        /// </summary>
        /// <value>
        ///     This property must be at least 1. 
        ///     The default value is 1.
        /// </value>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
        }

        // Validation callback for Slices, Stacks, Layers.
        static bool ValidateDivisions(object obj)
        {
            return (int)obj > 0;
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

            double x, y;
            int indexBase = 0;

            // Front side.
            // -----------

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = -Width / 2 + slice * Width / Slices;
                    Point3D point = new Point3D(x, y, 0);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, y, 0));
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

            // Rear side.
            // -----------
            indexBase = vertices.Count;

            // Fill the vertices, normals, textures collections.
            for (int stack = 0; stack <= Stacks; stack++)
            {
                y = Height / 2 - stack * Height / Stacks;

                for (int slice = 0; slice <= Slices; slice++)
                {
                    x = Width / 2 - slice * Width / Slices;
                    Point3D point = new Point3D(x, y, 0);
                    vertices.Add(point);

                    normals.Add(point - new Point3D(x, y, 0));
                    textures.Add(new Point((double)slice / Slices,
                                           (double)stack / Stacks));
                }
            }

            // Fill the indices collection.
            for (int stack = 0; stack < Stacks; stack++)
            {
                for (int slice = 0; slice < Slices; slice++)
                {
                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice + 1);

                    indices.Add(indexBase + (stack + 0) * (Slices + 1) + slice + 1);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice);
                    indices.Add(indexBase + (stack + 1) * (Slices + 1) + slice + 1);
                }
            }
        }

        /// <summary>
        ///     Creates a new instance of the BoxMesh class.
        /// </summary>
        /// <returns>
        ///     A new instance of BoxMesh.
        /// </returns>
        /// <remarks>
        ///     Overriding this method is required when deriving 
        ///     from the Freezable class.
        /// </remarks>
        protected override Freezable CreateInstanceCore()
        {
            return new SurfaceMesh();
        }
    }
}

