using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;


namespace HelixEngine.Paths
{
    [System.Windows.Markup.ContentProperty("Segments")] 
    public class PathFigure3D : Animatable
    {
        // ----------------------------------------------------
        // TODO: IsClosed, IsFilled, May have curves, etc, etc
        // ----------------------------------------------------

        public static int Approximation = 100;

        // Filled when dependency properties change.
        PathFigure3D figFlattened;

        /// <summary>
        /// 
        /// </summary>
        public PathFigure3D()
        {
            figFlattened = new PathFigure3D(0);
            Segments = new PathSegment3DCollection();
        }

        private PathFigure3D(int unused)
        {
            Segments = new PathSegment3DCollection();
            Segments.Add(new PolyLineSegment3D());
        }

        /// <summary>
        ///     Identifies the StartPoint dependency property.
        /// </summary>
        public static readonly DependencyProperty StartPointProperty =
            DependencyProperty.Register("StartPoint", 
                typeof(Point3D),
                typeof(PathFigure3D),
                new PropertyMetadata(new Point3D(), StartPointPropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Point3D StartPoint
        {
            set { SetValue(StartPointProperty, value); }
            get { return (Point3D)GetValue(StartPointProperty); }
        }
        
        static void StartPointPropertyChanged(DependencyObject obj,
                                              DependencyPropertyChangedEventArgs args)
        {
            PathFigure3D fig = obj as PathFigure3D;

            if (fig.figFlattened != null)
            {
                fig.figFlattened.StartPoint = (Point3D)args.NewValue;
                SegmentsPropertyChanged(obj, args);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SegmentsProperty =
            DependencyProperty.Register("Segments", 
            typeof(PathSegment3DCollection),
            typeof(PathFigure3D),
            new PropertyMetadata(null, SegmentsPropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public PathSegment3DCollection Segments
        {
            set { SetValue(SegmentsProperty, value); }
            get { return (PathSegment3DCollection)GetValue(SegmentsProperty); }
        }

        static void SegmentsPropertyChanged(DependencyObject obj,
                                DependencyPropertyChangedEventArgs args)
        {
            PathFigure3D fig = obj as PathFigure3D;

            if (fig.figFlattened != null)
                fig.SegmentsPropertyChanged();
        }

        void SegmentsPropertyChanged()
        {
            PolyLineSegment3D polyseg = figFlattened.Segments[0] as PolyLineSegment3D;
            Point3DCollection points = polyseg.Points;
            polyseg.Points = null;

            points.Clear();
            Point3D ptStart = StartPoint;

            foreach (PathSegment3D seg in Segments)
            {
                if (seg is LineSegment3D)
                {
                    LineSegment3D segLine = seg as LineSegment3D;
                    points.Add(segLine.Point);
                    ptStart = segLine.Point;
                }
                else if (seg is PolyLineSegment3D)
                {
                    PolyLineSegment3D segPoly = seg as PolyLineSegment3D;

                    foreach (Point3D pt in segPoly.Points)
                    {
                        points.Add(pt);
                        ptStart = pt;
                    }
                }

                else if (seg is BezierSegment3D)
                {
                    BezierSegment3D segBez = seg as BezierSegment3D;
                    ConvertBezier(points, ptStart, segBez.Point1, segBez.Point2, segBez.Point3);
                    ptStart = segBez.Point3;
                }

                else if (seg is PolyBezierSegment3D)
                {
                    PolyBezierSegment3D segPoly = seg as PolyBezierSegment3D;

                    for (int i = 0; i < segPoly.Points.Count; i += 3)
                    {
                        ConvertBezier(points, ptStart, segPoly.Points[i], segPoly.Points[i + 1], segPoly.Points[i + 2]);
                        ptStart = segPoly.Points[i + 2];
                    }
                }
            }
            polyseg.Points = points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public PathFigure3D GetFlattenedPathFigure()
        {
            return figFlattened;
        }

        void ConvertBezier(Point3DCollection points, Point3D point0, Point3D point1, Point3D point2, Point3D point3)
        {

            for (int i = 0; i < Approximation; i++)
            {
                double t = (double) (i + 1) / Approximation;

                double x = (1 - t) * (1 - t) * (1 - t) * point0.X +
                           3 * t * (1 - t) * (1 - t) * point1.X +
                           3 * t * t * (1 - t) * point2.X +
                           t * t * t * point3.X;

                double y = (1 - t) * (1 - t) * (1 - t) * point0.Y +
                           3 * t * (1 - t) * (1 - t) * point1.Y +
                           3 * t * t * (1 - t) * point2.Y +
                           t * t * t * point3.Y;

                double z = (1 - t) * (1 - t) * (1 - t) * point0.Z +
                           3 * t * (1 - t) * (1 - t) * point1.Z +
                           3 * t * t * (1 - t) * point2.Z +
                           t * t * t * point3.Z;

                points.Add(new Point3D(x, y, z));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "M" + StartPoint + Segments;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new PathFigure3D();
        }
    }
}
