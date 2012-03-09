//---------------------------------------------
// WirePolyline.cs (c) 2007 by Charles Petzold
//---------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Wires
{
    /// <summary>
    ///     Draws a polyline of constant perceived width in 3D space
    ///     between two points. 
    /// </summary>
    public class WirePolyline : WireBase
    {
        /// <summary>
        ///     Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
            typeof(Point3DCollection),
            typeof(WirePolyline),
            new PropertyMetadata(null, PropertyChanged));

        /// <summary>
        ///     Gets or sets a collection that contains the points of 
        ///     the polyline.
        /// </summary>
        public Point3DCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (Point3DCollection)GetValue(PointsProperty); }
        }

        /// <summary>
        ///     Sets the coordinates of all the individual lines in the visual.
        /// </summary>
        /// <param name="args">
        ///     The <c>DependencyPropertyChangedEventArgs</c> object associated 
        ///     with the property-changed event that resulted in this method 
        ///     being called.
        /// </param>
        /// <param name="lines">
        ///     The <c>Point3DCollection</c> to be filled.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         Classes that derive from <c>WireBase</c> override this
        ///         method to fill the <c>lines</c> collection.
        ///         It is custmary for implementations of this method to clear
        ///         the <c>lines</c> collection first before filling it. 
        ///         Each pair of successive members of the <c>lines</c>
        ///         collection indicate one straight line.
        ///     </para>
        ///     <para>
        ///         The <c>WirePolyline</c> class implements this method by 
        ///         clearing the <c>lines</c> collection and then breaking
        ///         down its <c>Points</c> collection into individual lines
        ///         and then adding the start and end points to the collection.
        ///     </para>
        /// </remarks>
        protected override void Generate(DependencyPropertyChangedEventArgs args,
                                         Point3DCollection lines)
        {
            Point3DCollection points = Points;
            lines.Clear();

            for (int i = 0; i < points.Count - 1; i++)
            {
                lines.Add(points[i]);
                lines.Add(points[i + 1]);
            }
        }
    }
}
