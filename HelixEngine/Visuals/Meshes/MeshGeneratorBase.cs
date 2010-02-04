//--------------------------------------------------
// MeshGeneratorBase.cs (c) 2007 by Charles Petzold
//--------------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Abstract base class for classes that generate 
    ///     MeshGeometry3D objects.
    /// </summary>
    [RuntimeNameProperty("Name")]
    public abstract class MeshGeneratorBase : Animatable
    {
        /// <summary>
        ///     Identifies the Name dependency property.
        /// </summary>
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name",
                typeof(string),
                typeof(MeshGeneratorBase));

        /// <summary>
        ///     Gets or sets the identifying name of this mesh
        ///     generator object.
        /// </summary>
        public string Name
        {
            set { SetValue(NameProperty, value); }
            get { return (string)GetValue(NameProperty); }
        }

        static DependencyPropertyKey GeometryKey =
            DependencyProperty.RegisterReadOnly("Geometry",
                typeof(MeshGeometry3D),
                typeof(MeshGeneratorBase),
                new PropertyMetadata(new MeshGeometry3D()));

        /// <summary>
        ///     Identifies the Geometry dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryProperty =
            GeometryKey.DependencyProperty;

        /// <summary>
        ///     Gets or sets the Geometry property.
        /// </summary>
        public MeshGeometry3D Geometry
        {
            protected set { SetValue(GeometryKey, value); }
            get { return (MeshGeometry3D)GetValue(GeometryProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of MeshGeneratorBase.
        /// </summary>
        public MeshGeneratorBase()
        {
            Geometry = new MeshGeometry3D();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        protected static void PropertyChanged(DependencyObject obj,
                                              DependencyPropertyChangedEventArgs args)
        {
            (obj as MeshGeneratorBase).PropertyChanged(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected virtual void PropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            // Get the MeshGeometry3D for local convenience.
            MeshGeometry3D mesh = Geometry;

            // Obtain the four collection of the MeshGeometry3D.
            Point3DCollection vertices = mesh.Positions;
            Vector3DCollection normals = mesh.Normals;
            Int32Collection indices = mesh.TriangleIndices;
            PointCollection textures = mesh.TextureCoordinates;

            // Set the MeshGeometry3D collections to null while updating.
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TriangleIndices = null;
            mesh.TextureCoordinates = null;

            // Call the abstract method to fill the collections.
            Triangulate(args, vertices, normals, indices, textures);

            // Set the updated collections to the MeshGeometry3D.
            mesh.TextureCoordinates = textures;
            mesh.TriangleIndices = indices;
            mesh.Normals = normals;
            mesh.Positions = vertices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="indices"></param>
        /// <param name="textures"></param>
        protected abstract void Triangulate(DependencyPropertyChangedEventArgs args,
                                            Point3DCollection vertices,
                                            Vector3DCollection normals,
                                            Int32Collection indices,
                                            PointCollection textures);
    }
}
