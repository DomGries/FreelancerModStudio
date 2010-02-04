//------------------------------------------------
// BezierSegment3D.cs (c) 2007 by Charles Petzold
//------------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Paths
{
    public class BezierSegment3D : PathSegment3D
    {
        /// <summary>
        ///     Identifies the Point1 dependency property.
        /// </summary>
        public static readonly DependencyProperty Point1Property = 
            DependencyProperty.Register("Point1", 
            typeof(Point3D),
            typeof(BezierSegment3D),
            new PropertyMetadata(new Point3D()));

        /// <summary>
        ///     Gets or sets the first control point of the curve. 
        /// </summary>
        public Point3D Point1
        {
            set { SetValue(Point1Property, value); }
            get { return (Point3D) GetValue(Point1Property); }
        }

        /// <summary>
        ///     Identifies the Point2 dependency property. 
        /// </summary>
        public static readonly DependencyProperty Point2Property =
            DependencyProperty.Register("Point2", 
            typeof(Point3D),
            typeof(BezierSegment3D),
            new PropertyMetadata(new Point3D()));

        /// <summary>
        ///     Gets or sets the second control point of the curve. 
        /// </summary>
        public Point3D Point2
        {
            set { SetValue(Point2Property, value); }
            get { return (Point3D)GetValue(Point2Property); }
        }

        /// <summary>
        ///     Identifies the Point3 dependency property. 
        /// </summary>
        public static readonly DependencyProperty Point3Property =
            DependencyProperty.Register("Point3", 
            typeof(Point3D),
            typeof(BezierSegment3D),
            new PropertyMetadata(new Point3D()));

        /// <summary>
        ///     Gets or sets the final point of the curve. 
        /// </summary>
        public Point3D Point3
        {
            set { SetValue(Point3Property, value); }
            get { return (Point3D)GetValue(Point3Property); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("C{0} {1} {2}", Point1, Point2, Point3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BezierSegment3D();
        }
    }
}
