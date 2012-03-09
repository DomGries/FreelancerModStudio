

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Paths
{
    /// <summary>
    ///     Draws a line between two points in a <c>PathFigure3D</c>.
    /// </summary>
    /// <remarks>
    ///     A <c>LineSegment3D</c> is always part of a <c>PathFigure3D</c>. 
    ///     The starting point of the line is given by the <c>StartPoint</c> property of
    ///     <c>PathFigure3D</c> or the end point of the previous <c>PathSegment3D</c> object in 
    ///     the <c>PathFigure3D</c>.
    /// </remarks>
    public class LineSegment3D : PathSegment3D
    {
        // ---------------------------------------
        // Point dependency property and property.
        // ---------------------------------------

        /// <summary>
        ///     Identifies the <c>Point</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty PointProperty = 
            DependencyProperty.Register("Point", typeof(Point3D),
            typeof(LineSegment3D),
            new PropertyMetadata(new Point3D(0, 0, 0)));

        /// <summary>
        ///     Gets or sets the end point of the line segment. This is a dependency property.
        /// </summary>
        public Point3D Point
        {
            set { SetValue(PointProperty, value); }
            get { return (Point3D) GetValue(PointProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "L" + Point;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new LineSegment3D();
        }
    }
}
