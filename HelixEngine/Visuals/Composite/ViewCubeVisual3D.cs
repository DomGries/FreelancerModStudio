using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;

namespace HelixEngine
{
    public class ViewCubeVisual3D : ModelVisual3D
    {
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point3D), typeof(ViewCubeVisual3D),
                                        new UIPropertyMetadata(new Point3D(0, 0, 0)));

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(double), typeof(ViewCubeVisual3D), new UIPropertyMetadata(5.0));

        public static readonly DependencyProperty ViewportProperty =
            DependencyProperty.Register("Viewport", typeof(Viewport3D), typeof(ViewCubeVisual3D),
                                        new PropertyMetadata(null, ViewportChanged));

        private Dictionary<object, Vector3D> _normal = new Dictionary<object, Vector3D>();
        private Dictionary<object, Vector3D> _up = new Dictionary<object, Vector3D>();

        public ViewCubeVisual3D()
        {
            UpdateVisuals();
        }

        public Point3D Center
        {
            get { return (Point3D)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public Viewport3D Viewport
        {
            get { return (Viewport3D)GetValue(ViewportProperty); }
            set { SetValue(ViewportProperty, value); }
        }

        private static void ViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ViewCubeVisual3D)d).OnViewportChanged();
        }

        private void OnViewportChanged()
        {
            //  if (Camera == null && Viewport != null)
            //      Camera = Viewport.Camera as PerspectiveCamera;
        }


        private void UpdateVisuals()
        {
            Children.Clear();
            addFace(new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), Brushes.Red, "F");
            addFace(new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1), Brushes.Red, "B");
            addFace(new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), Brushes.Green, "R");
            addFace(new Vector3D(0, -1, 0), new Vector3D(0, 0, 1), Brushes.Green, "L");
            addFace(new Vector3D(0, 0, 1), new Vector3D(0, 1, 0), Brushes.Blue, "U");
            addFace(new Vector3D(0, 0, -1), new Vector3D(0, -1, 0), Brushes.Blue, "D");
            var circle = new PieSliceVisual3D()
                            {
                                Center = new Point3D(0, 0, -Size / 2),
                                InnerRadius = Size,
                                OuterRadius = Size * 1.3,
                                StartAngle = 0,
                                EndAngle = 360,
                                Divisions = 9,
                                Fill = Brushes.Gray
                            };
            Children.Add(circle);
        }

        private void addFace(Vector3D normal, Vector3D up, Brush b, string text)
        {
            var grid = new Grid { Width = 20, Height = 20 };
            grid.Background = b;
            grid.Children.Add(new TextBlock
                                  {
                                      Text = text,
                                      VerticalAlignment = VerticalAlignment.Center,
                                      HorizontalAlignment = HorizontalAlignment.Center,
                                      FontSize = 15,
                                      Foreground = Brushes.White
                                  });
            grid.Arrange(new Rect(new Point(0, 0), new Size(20, 20)));

            var bmp = new RenderTargetBitmap((int)grid.Width, (int)grid.Height, 96, 96, PixelFormats.Default);
            bmp.Render(grid);

            Material material = MaterialHelper.CreateMaterial(new ImageBrush(bmp));

            double a = Size;
            var geometry = new MeshGeometry3D();
            geometry.AddCubeFace(Center, normal, up, a, a, a);
            geometry.Freeze();

            var model = new GeometryModel3D() { Geometry = geometry, Material = material };
            var element = new ModelUIElement3D() { Model = model };
            element.MouseLeftButtonDown += face_MouseLeftButtonDown;
            //element.MouseEnter += face_MouseEnter;
            //element.MouseLeave += face_MouseLeave;

            _normal.Add(element, normal);
            _up.Add(element, up);

            Children.Add(element);
        }

        /*private void face_MouseLeave(object sender, MouseEventArgs e)
        {
                        var el = (ModelUIElement3D) sender;
                        var model = el.Model as GeometryModel3D;
                        var mg = model.Material as MaterialGroup;
                        var dm = mg.Children[0] as DiffuseMaterial;
                        AnimateOpacity(dm.Brush, 0.8, 200);
        }

        private void face_MouseEnter(object sender, MouseEventArgs e)
        {
            var el = (ModelUIElement3D) sender;
                var model = el.Model as GeometryModel3D;
                var mg=model.Material as MaterialGroup;
                var dm=mg.Children[0] as DiffuseMaterial;
                AnimateOpacity(dm.Brush, 1.0, 200);
        }*/


        private void AnimateOpacity(Animatable obj, double toOpacity, double animationTime)
        {
            var a = new DoubleAnimation(toOpacity,
                                        new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5 };
            a.Completed += new EventHandler(a_Completed);
            obj.BeginAnimation(UIElement.OpacityProperty, a);
        }

        void a_Completed(object sender, EventArgs e)
        {

        }


        private void face_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Vector3D faceNormal = _normal[sender];
            Vector3D faceUp = _up[sender];

            var camera = Viewport.Camera as PerspectiveCamera;
            Point3D target = camera.Position + camera.LookDirection;
            double dist = camera.LookDirection.Length;

            Vector3D lookdir = -faceNormal;
            lookdir.Normalize();
            lookdir = lookdir * dist;

            Point3D pos = target - lookdir;
            Vector3D updir = faceUp;
            updir.Normalize();

            CameraHelper.AnimateTo(camera, pos, lookdir, updir, 500);
        }
    }

    [ContentProperty("Children")]
    public class MyContainerUIElement3D : UIElement3D
    {
        // Fields
        private readonly ObservableCollection<Visual3D> _children;

        // Methods
        public MyContainerUIElement3D()
        {
            _children = new ObservableCollection<Visual3D>();
        }

        public ObservableCollection<Visual3D> Children
        {
            get { return _children; }
        }

        /*        protected override AutomationPeer OnCreateAutomationPeer()
                {
                    return new UIElement3DAutomationPeer(this);
                }

                // Properties
                [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
                public Visual3DCollection Children
                {
                    get
                    {
                        base.VerifyAPIReadOnly();
                        return this._children;
                    }
                }
                */

        protected override int Visual3DChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual3D GetVisual3DChild(int index)
        {
            return _children[index];
        }
    }
}