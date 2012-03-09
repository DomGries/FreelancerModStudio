//
//
//
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Wires
{
    /// <summary>
    ///     Draws a series of successive straight line of constant perceived 
    ///     width in 3D space between two points. 
    /// </summary>
    public class WireLines : WireBase
    {
        /// <summary>
        ///     Identifies the Lines dependency property.
        /// </summary>
        public static readonly DependencyProperty LinesProperty =
            DependencyProperty.Register("Lines",
            typeof(Point3DCollection),
            typeof(WireLines),
            new PropertyMetadata(null, PropertyChanged));

        /// <summary>
        ///     Gets or sets a collection that contains the start and end
        ///     points of each individual line.
        /// </summary>
        /// <remarks>
        ///     This collection normally contains an even number of points.
        ///     Each pair of points specifies one line.
        /// </remarks>
        public Point3DCollection Lines
        {
            set { SetValue(LinesProperty, value); }
            get { return (Point3DCollection)GetValue(LinesProperty); }
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
        ///         The <c>WireLines</c> class implements this method by 
        ///         clearing the <c>lines</c> collection and then copying
        ///         its own <c>Lines</c> collection to it.
        ///     </para>
        /// </remarks>
        protected override void Generate(DependencyPropertyChangedEventArgs args, 
                                         Point3DCollection lines)
        {
            lines.Clear();

            foreach (Point3D point in Lines)
                lines.Add(point);
        }
    }
}
