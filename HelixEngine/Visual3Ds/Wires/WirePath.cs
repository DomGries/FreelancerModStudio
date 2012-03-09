//-----------------------------------------
// WirePath.cs (c) 2007 by Charles Petzold
//-----------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine.Paths;

namespace HelixEngine.Wires
{
    /// <summary>
    ///     Draws a series of straight lines and curves of perceived uniform
    ///     width in 3D space.
    /// </summary>
    public class WirePath : WireBase
    {
        /// <summary>
        ///     Initializes a new instance of the <c>WirePath</c> class.
        /// </summary>
        public WirePath()
        {
            Data = (PathGeometry3D) Data.Clone();
        }

        /// <summary>
        ///     Identifies the <c>Data</c> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data",
            typeof(PathGeometry3D),
            typeof(WirePath),
            new PropertyMetadata(new PathGeometry3D(), PropertyChanged));

        /// <summary>
        ///     Gets or sets a <c>PathGeometry3D</c> that specifies the 
        ///     shape to be drawn. 
        /// </summary>
        public PathGeometry3D Data
        {
            set { SetValue(DataProperty, value); }
            get { return (PathGeometry3D)GetValue(DataProperty); }
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
        /// </remarks>
        protected override void Generate(DependencyPropertyChangedEventArgs args,
                                         Point3DCollection lines)
        {
            lines.Clear();

            if (Data == null)
                return;

            Transform3D xform = Data.Transform;

            foreach (PathFigure3D fig in Data.Figures)
            {
                PathFigure3D figFlattened = fig.GetFlattenedPathFigure();
                Point3D pointStart = xform.Transform(figFlattened.StartPoint);

                foreach (PathSegment3D seg in figFlattened.Segments)
                {
                    PolyLineSegment3D segPoly = seg as PolyLineSegment3D;

                    for (int i = 0; i < segPoly.Points.Count; i++)
                    {
                        lines.Add(pointStart);
                        Point3D point = xform.Transform(segPoly.Points[i]);
                        lines.Add(point);
                        pointStart = point;
                    }
                }
            }
        }
    }
}


 

