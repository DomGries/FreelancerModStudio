using System;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;

namespace HelixEngine
{
    /// <summary>
    /// A Target symbol adorner. This is shown when manipulating the camera with the mouse.
    /// Inspired by Google Earth...
    /// </summary>
    public class TargetSymbolAdorner : Adorner
    {
        public Point Position { get; set; }

        public TargetSymbolAdorner(UIElement adornedElement, Point position)
            : base(adornedElement)
        {
            Position = position;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var lightBrush = new SolidColorBrush(Colors.LightGray);
            var darkBrush = new SolidColorBrush(Colors.Black);
            lightBrush.Opacity = 0.4;
            darkBrush.Opacity = 0.1;

            double t1 = 6; // thickness of dark circle pen
            double t2 = 2; // thickness of light pen (circle, arcs, segments)
            double d = 0; // distance from light circle to segments
            double l = 10; // length of segments
            double r = 20.0; // radius of light circle

            double r1 = r - (t1 + t2) / 2;
            double r2 = r + l;
            double r3 = r + t2 / 2 + d;
            double r4 = (r + r2) / 2;

            var darkPen = new Pen(darkBrush, t1);
            var lightPen = new Pen(lightBrush, t2);

            dc.DrawEllipse(null, lightPen, Position, r, r);
            dc.DrawEllipse(null, darkPen, Position, r1, r1);
            dc.DrawArc(null, lightPen, Position, 10, 80, r4, r4);
            dc.DrawArc(null, lightPen, Position, 100, 170, r4, r4);
            dc.DrawArc(null, lightPen, Position, 190, 260, r4, r4);
            dc.DrawArc(null, lightPen, Position, 280, 350, r4, r4);

            dc.DrawLine(lightPen, new Point(Position.X, Position.Y - r2), new Point(Position.X, Position.Y - r3));
            dc.DrawLine(lightPen, new Point(Position.X, Position.Y + r2), new Point(Position.X, Position.Y + r3));
            dc.DrawLine(lightPen, new Point(Position.X - r2, Position.Y), new Point(Position.X - r3, Position.Y));
            dc.DrawLine(lightPen, new Point(Position.X + r2, Position.Y), new Point(Position.X + r3, Position.Y));
        }
    }

    public static class DrawingContextExtensions
    {
        // http://blogs.vertigo.com/personal/ralph/Blog/archive/2007/02/09/wpf-drawing-arcs.aspx
        public static void DrawArc(this DrawingContext dc, Brush brush, Pen pen, Point start, Point end, SweepDirection direction, double radiusX, double radiusY)
        {
            // setup the geometry object
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            geometry.Figures.Add(figure);
            figure.StartPoint = start;

            // add the arc to the geometry
            figure.Segments.Add(new ArcSegment(end, new Size(radiusX, radiusY),
                0, false, direction, true));

            // draw the arc
            dc.DrawGeometry(brush, pen, geometry);
        }

        public static void DrawArc(this DrawingContext dc, Brush brush, Pen pen, Point position, double startAngle, double endAngle, SweepDirection direction, double radiusX, double radiusY)
        {
            double startRadians = startAngle / 180 * Math.PI;
            double endRadians = endAngle / 180 * Math.PI;
            var start = position + new Vector(Math.Cos(startRadians) * radiusX, -Math.Sin(startRadians) * radiusY);
            var end = position + new Vector(Math.Cos(endRadians) * radiusX, -Math.Sin(endRadians) * radiusY);
            dc.DrawArc(brush, pen, start, end, direction, radiusX, radiusY);
        }
        public static void DrawArc(this DrawingContext dc, Brush brush, Pen pen, Point position, double startAngle, double endAngle, double radiusX, double radiusY)
        {
            DrawArc(dc, brush, pen, position, startAngle, endAngle, SweepDirection.Counterclockwise, radiusX, radiusY);
        }
    }
}
