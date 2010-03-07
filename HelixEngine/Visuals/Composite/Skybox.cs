using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HelixEngine.Visuals.Composite
{
    public class SkyBox : ModelVisual3D
    {
        ScaleTransform3D scale;

        public Vector3D Size
        {
            get { return (Vector3D)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(Vector3D),
            typeof(SkyBox),
            new FrameworkPropertyMetadata(OnSizeChanged)
            );

        private static void OnSizeChanged(DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
        {
            SkyBox sb = sender as SkyBox;
            sb.scale.ScaleX = sb.Size.X;
            sb.scale.ScaleY = sb.Size.Y;
            sb.scale.ScaleZ = sb.Size.Z;
        }

        public SkyBox()
        {
            Model3DGroup sides = new Model3DGroup();

            Point3D[] p = new Point3D[] {
                new Point3D(-1, 1, -1),
                new Point3D(-1, -1, -1),
                new Point3D(1, -1, -1),
                new Point3D(1, 1, -1),
                new Point3D(1, 1, 1),
                new Point3D(1, -1, 1),
                new Point3D(-1, -1, 1),
                new Point3D(-1, 1, 1)
                };

            Int32Collection triangleIndices = new Int32Collection(new int[] { 0, 1, 2, 2, 3, 0 });

            PointCollection textCoords = new PointCollection(new Point[] {
                new Point(0,0),
                new Point(0,1),
                new Point(1,1),
                new Point(1,0)
                });

            MeshGeometry3D quad = new MeshGeometry3D();
            quad.Positions.Add(p[0]);
            quad.Positions.Add(p[1]);
            quad.Positions.Add(p[2]);
            quad.Positions.Add(p[3]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("north")));

            quad = new MeshGeometry3D();
            quad.Positions.Add(p[4]);
            quad.Positions.Add(p[5]);
            quad.Positions.Add(p[6]);
            quad.Positions.Add(p[7]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("south")));

            quad = new MeshGeometry3D();
            quad.Positions.Add(p[1]);
            quad.Positions.Add(p[6]);
            quad.Positions.Add(p[5]);
            quad.Positions.Add(p[4]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("down")));

            quad = new MeshGeometry3D();
            quad.Positions.Add(p[7]);
            quad.Positions.Add(p[6]);
            quad.Positions.Add(p[1]);
            quad.Positions.Add(p[0]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("west")));

            quad = new MeshGeometry3D();
            quad.Positions.Add(p[3]);
            quad.Positions.Add(p[2]);
            quad.Positions.Add(p[5]);
            quad.Positions.Add(p[4]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("east")));

            quad = new MeshGeometry3D();
            quad.Positions.Add(p[7]);
            quad.Positions.Add(p[0]);
            quad.Positions.Add(p[3]);
            quad.Positions.Add(p[4]);
            quad.TriangleIndices = triangleIndices;
            quad.TextureCoordinates = textCoords;
            sides.Children.Add(new GeometryModel3D(quad, GetSideMaterial("up")));

            this.scale = new ScaleTransform3D(1, 1, 1);
            this.Transform = this.scale;
            this.Content = sides;
        }

        private Material GetSideMaterial(string file)
        {
            ImageBrush ib = new ImageBrush(new BitmapImage(new Uri(
                "pack://application:,,,/FreelancerModStudio;component/SkyBoxImages/" + file + ".jpg",
                UriKind.Absolute)));

            ib.ViewportUnits = BrushMappingMode.Absolute;
            ib.TileMode = TileMode.None;
            return new DiffuseMaterial(ib);
        }
    }
}
