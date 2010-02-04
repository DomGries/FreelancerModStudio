using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// The <see cref="MeshElement3D"/> is a base class that implements a <see cref="Geometry3D"/> and front and back <see cref="Material"/>s.
    /// Derived classes should override the Tesselate() method.
    /// </summary>
    public abstract class MeshElement3D : ModelVisual3D
    {

       /* public Geometry3D Geometry
        {
            get { return (Geometry3D)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Geometry.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register("Geometry", typeof(Geometry3D), typeof(MeshElement3D), new UIPropertyMetadata(null, GeometryChanged));
        */



        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Fill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(MeshElement3D), new UIPropertyMetadata(null,FillChanged));

        private static void FillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var el = (MeshElement3D) d;
            el.Material = MaterialHelper.CreateMaterial(el.Fill);
            el.BackMaterial = el.Material;
        }

        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }

        public static readonly DependencyProperty MaterialProperty =
            DependencyProperty.Register("Material", typeof(Material), typeof(MeshElement3D), new UIPropertyMetadata(null, MaterialChanged));

        public Material BackMaterial
        {
            get { return (Material)GetValue(BackMaterialProperty); }
            set { SetValue(BackMaterialProperty, value); }
        }

        public static readonly DependencyProperty BackMaterialProperty =
            DependencyProperty.Register("BackMaterial", typeof(Material), typeof(MeshElement3D), new UIPropertyMetadata(null, MaterialChanged));


        protected static void GeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
//            Debug.WriteLine(String.Format("[{0}] GeometryChange: {1}", d.ToString(),e.Property));
            ((MeshElement3D)d).GeometryChanged();
            // todo: rather invalidate??
        }

        protected static void MaterialChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
  //          Debug.WriteLine(String.Format("[{0}] MaterialChange: {1}", d.ToString(), e.Property));
            ((MeshElement3D)d).MaterialChanged();
            // todo: rather invalidate??
        }

        public MeshElement3D()
        {
            Content = new GeometryModel3D();
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            InvalidateModel();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            lock (_invalidateLock)
            {
                if (_isInvalidated)
                {
                    _isInvalidated = false;
                    GeometryChanged();
                    MaterialChanged();
                }
            }
        }

        private bool _isInvalidated = false;
        private object _invalidateLock = "";

        private void InvalidateModel()
        {
            lock (_invalidateLock)
            {
                _isInvalidated = true;
            }
        }

        public GeometryModel3D Model
        {
            get { return Content as GeometryModel3D; }
        }

        /// <summary>
        /// Forces an update to the geometry model and materials
        /// </summary>
        public void UpdateModel()
        {
            GeometryChanged();
            MaterialChanged();
        }

        protected void MaterialChanged()
        {
            if (!_doUpdates)
                return;

            var model = Model;
            if (model == null)
                return;

            if (Material == null)
            {
                // use a default blue material
                model.Material = MaterialHelper.CreateMaterial(System.Windows.Media.Brushes.Blue);
            }
            else
                model.Material = Material;

            // the back material may be null (invisible)
            model.BackMaterial = BackMaterial;
        }

        protected void GeometryChanged()
        {
            if (!_doUpdates)
                return;
            Debug.WriteLine("  Tesselating");
            // create a new model incase the old model was frozen
            // todo...
            Model.Geometry = Tessellate();
        }

        private bool _doUpdates = true;
        public void DisableUpdates()
        {
            _doUpdates = false;
        }
        public void EnableUpdates()
        {
            _doUpdates = true;
        }


        /// <summary>
        /// Do the tesselation and return the <see cref="MeshGeometry3D"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract MeshGeometry3D Tessellate();

        // alternative:
        /*
        private MeshGeometry3D Tessellate()
        {
            var mesh = new MeshGeometry3D();
            var positions = mesh.Positions;
            var normals = mesh.Normals;
            var textureCoordinates = mesh.TextureCoordinates;
            var triangleIndices = mesh.TriangleIndices;
            mesh.Positions = null;
            mesh.Normals = null;
            mesh.TextureCoordinates = null;
            mesh.TriangleIndices = null;

            Tessellate(positions, normals, textureCoordinates, triangleIndices);

            mesh.Positions = positions;
            mesh.Normals = normals;
            mesh.TextureCoordinates = textureCoordinates;
            mesh.TriangleIndices = triangleIndices;
    
        }

        protected abstract void Tessellate(Point3DCollection points, Vector3DCollection normals, PointCollection textureCoordinates, Int32Collection triangleIndices);
        */

    }

}
