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
    public class WireBoundingBox : WireBase
    {
        /// <summary>
        /// Identifies the <see cref="BoundingBox"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BoundingBoxProperty =
            DependencyProperty.Register("BoundingBox",
            typeof(Rect3D),
            typeof(WireBoundingBox),
            new PropertyMetadata(new Rect3D(), PropertyChanged));

        /// <summary>
        /// Gets or sets the bounding box.
        /// </summary>
        /// <value> The bounding box. </value>
        public Rect3D BoundingBox
        {
            set { SetValue(BoundingBoxProperty, value); }
            get { return (Rect3D)GetValue(BoundingBoxProperty); }
        }

        /// <summary>
        /// Updates the box.
        /// </summary>
        protected override void Generate(DependencyPropertyChangedEventArgs args, 
                                         Point3DCollection lines)
        {
            lines.Clear();
            if (this.BoundingBox.IsEmpty)
            {
                return;
            }

            Rect3D bb = this.BoundingBox;

            var p0 = new Point3D(bb.X, bb.Y, bb.Z);
            var p1 = new Point3D(bb.X, bb.Y + bb.SizeY, bb.Z);
            var p2 = new Point3D(bb.X + bb.SizeX, bb.Y + bb.SizeY, bb.Z);
            var p3 = new Point3D(bb.X + bb.SizeX, bb.Y, bb.Z);
            var p4 = new Point3D(bb.X, bb.Y, bb.Z + bb.SizeZ);
            var p5 = new Point3D(bb.X, bb.Y + bb.SizeY, bb.Z + bb.SizeZ);
            var p6 = new Point3D(bb.X + bb.SizeX, bb.Y + bb.SizeY, bb.Z + bb.SizeZ);
            var p7 = new Point3D(bb.X + bb.SizeX, bb.Y, bb.Z + bb.SizeZ);

            Action<Point3D, Point3D> AddEdge = (p, q) =>
            {
                lines.Add(p);
                lines.Add(q);
            };

            AddEdge(p0, p1);
            AddEdge(p1, p2);
            AddEdge(p2, p3);
            AddEdge(p3, p0);

            AddEdge(p4, p5);
            AddEdge(p5, p6);
            AddEdge(p6, p7);
            AddEdge(p7, p4);

            AddEdge(p0, p4);
            AddEdge(p1, p5);
            AddEdge(p2, p6);
            AddEdge(p3, p7);
        }
    }
}
