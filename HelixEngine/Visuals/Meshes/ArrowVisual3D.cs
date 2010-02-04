using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    public class ArrowVisual3D : MeshElement3D
    {
        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof (double), typeof (ArrowVisual3D),
                                        new UIPropertyMetadata(1.0, GeometryChanged));

        public static readonly DependencyProperty HeadLengthProperty =
            DependencyProperty.Register("HeadLength", typeof (double), typeof (ArrowVisual3D),
                                        new UIPropertyMetadata(3.0, GeometryChanged));

        public static readonly DependencyProperty Point1Property =
            DependencyProperty.Register("Point1", typeof (Point3D), typeof (ArrowVisual3D),
                                        new UIPropertyMetadata(new Point3D(0, 0, 0), GeometryChanged));

        public static readonly DependencyProperty Point2Property =
            DependencyProperty.Register("Point2", typeof (Point3D), typeof (ArrowVisual3D),
                                        new UIPropertyMetadata(new Point3D(0, 0, 10), GeometryChanged));

        public static readonly DependencyProperty SidesProperty =
            DependencyProperty.Register("Sides", typeof (int), typeof (ArrowVisual3D),
                                        new UIPropertyMetadata(36, GeometryChanged));

        public double Diameter
        {
            get { return (double) GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }
        public double HeadLength
        {
            get { return (double) GetValue(HeadLengthProperty); }
            set { SetValue(HeadLengthProperty, value); }
        }

        public int Sides
        {
            get { return (int) GetValue(SidesProperty); }
            set { SetValue(SidesProperty, value); }
        }

        public Point3D Point1
        {
            get { return (Point3D) GetValue(Point1Property); }
            set { SetValue(Point1Property, value); }
        }

        public Point3D Point2
        {
            get { return (Point3D) GetValue(Point2Property); }
            set { SetValue(Point2Property, value); }
        }

        public Point3D Origin
        {
            get { return Point1; }
            set { Point1 = value; }
        }

        public Vector3D Direction
        {
            get { return Point2 - Point1; }
            set { Point2 = Point1 + value; }
        }

        protected override MeshGeometry3D Tessellate()
        {
            Vector3D dir = Point2 - Point1;
            double length = dir.Length;
            double r = Diameter/2;

            var pc = new PointCollection();
            pc.Add(new Point(0, 0));
            pc.Add(new Point(0, r));
            pc.Add(new Point(length - HeadLength, r));
            pc.Add(new Point(length - HeadLength, r*2));
            pc.Add(new Point(length, 0));

            return MeshGeometryHelper.CreateRevolvedGeometry(pc, Point1, dir, Sides);
        }
    }
}