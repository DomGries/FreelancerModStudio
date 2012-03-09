using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// A flat pie slice defined by center, normal, up vectors, inner and outer radius, start and end angles
    /// </summary>
    public class PieSliceVisual3D : MeshElement3D
    {
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point3D), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(new Point3D()));

        public static readonly DependencyProperty DivisionsProperty =
            DependencyProperty.Register("Divisions", typeof(int), typeof(PieSliceVisual3D), new UIPropertyMetadata(20));

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(90.0));

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(0.5));


        public static readonly DependencyProperty NormalProperty =
            DependencyProperty.Register("Normal", typeof(Vector3D), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(new Vector3D(0, 0, 1)));

        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register("OuterRadius", typeof(double), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(1.0));

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(0.0));


        public static readonly DependencyProperty UpVectorProperty =
            DependencyProperty.Register("UpVector", typeof(Vector3D), typeof(PieSliceVisual3D),
                                        new UIPropertyMetadata(new Vector3D(0, 1, 0)));

        public Point3D Center
        {
            get { return (Point3D)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        public Vector3D Normal
        {
            get { return (Vector3D)GetValue(NormalProperty); }
            set { SetValue(NormalProperty, value); }
        }

        public Vector3D UpVector
        {
            get { return (Vector3D)GetValue(UpVectorProperty); }
            set { SetValue(UpVectorProperty, value); }
        }


        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }


        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }


        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }


        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }


        public int Divisions
        {
            get { return (int)GetValue(DivisionsProperty); }
            set { SetValue(DivisionsProperty, value); }
        }


        protected override MeshGeometry3D Tessellate()
        {
            var pc1 = new Point3DCollection();
            var pc2 = new Point3DCollection();
            var right = Vector3D.CrossProduct(UpVector, Normal);
            for (int i = 0; i < Divisions; i++)
            {
                double angle = StartAngle + (EndAngle - StartAngle) * i / (Divisions - 1);
                double angleRad = angle / 180 * Math.PI;
                Vector3D dir = right * Math.Cos(angleRad) + UpVector * Math.Sin(angleRad);
                pc1.Add(Center + dir * InnerRadius);
                pc2.Add(Center + dir * OuterRadius);
            }
            var mesh = new MeshGeometry3D();

            MeshGeometryHelper.AddTriangleStrip(mesh, pc1, pc2, null, null, null, null);
            return mesh;
        }
    }
}