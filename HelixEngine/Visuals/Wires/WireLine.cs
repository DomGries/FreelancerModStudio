//-----------------------------------------
// WireLine.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Wires
{
    /// <summary>
    ///     Draws a straight line of constant perceived width in 3D space
    ///     between two points. 
    /// </summary>
    public class WireLine : WireBase
    {
        /// <summary>
        ///     Identifies the Point1 dependency property.
        /// </summary>
        public static readonly DependencyProperty Point1Property =
            DependencyProperty.Register("Point1",
            typeof(Point3D),
            typeof(WireLine),
            new PropertyMetadata(new Point3D(), PropertyChanged));

        /// <summary>
        ///     Gets or sets the Line start point.
        /// </summary>
        public Point3D Point1
        {
            set { SetValue(Point1Property, value); }
            get { return (Point3D)GetValue(Point1Property); }
        }

        /// <summary>
        ///     Identifies the Point2 dependency property.
        /// </summary>
        public static readonly DependencyProperty Point2Property =
            DependencyProperty.Register("Point2",
            typeof(Point3D),
            typeof(WireLine),
            new PropertyMetadata(new Point3D(), PropertyChanged));

        /// <summary>
        ///     Gets or sets the Line end point.
        /// </summary>
        public Point3D Point2
        {
            set { SetValue(Point2Property, value); }
            get { return (Point3D)GetValue(Point2Property); }
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
        ///         The <c>WireLine</c> class implements this method by 
        ///         clearing the <c>lines</c> collection and then adding 
        ///         <c>Point1</c> and <c>Point2</c> to the collection.
        ///     </para>
        /// </remarks>
        protected override void Generate(DependencyPropertyChangedEventArgs args, 
                                         Point3DCollection lines)
        {
            lines.Clear();
            lines.Add(Point1);
            lines.Add(Point2);
        }
    }
}
