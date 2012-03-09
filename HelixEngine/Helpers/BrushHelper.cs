using System.Windows;
using System.Windows.Media;

namespace HelixEngine
{
    /// <summary>
    /// Provides methods that creates brushes.
    /// </summary>
    public static class BrushHelper
    {
        #region Public Methods

        /// <summary>
        /// Creates a copy of a brush with the specified opacity.
        /// </summary>
        /// <param name="brush">
        /// The brush to copy.
        /// </param>
        /// <param name="opacity">
        /// The opacity.
        /// </param>
        /// <returns>
        /// </returns>
        public static Brush ChangeOpacity(Brush brush, double opacity)
        {
            brush = brush.Clone();
            brush.Opacity = opacity;
            return brush;
        }

        // http://en.wikipedia.org/wiki/HSL_and_HSV 
        /// <summary>
        /// Creates a HSV brush.
        /// </summary>
        /// <param name="alpha">
        /// The opacity (0-1).
        /// </param>
        /// <returns>
        /// </returns>
        public static LinearGradientBrush CreateHsvBrush(double alpha)
        {
            var a = (byte)(alpha * 255);
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0xff, 0x00, 0x00), 0.00));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0xff, 0xff, 0x00), 0.17));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0x00, 0xff, 0x00), 0.33));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0x00, 0xff, 0xff), 0.50));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0x00, 0x00, 0xff), 0.67));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0xff, 0x00, 0xff), 0.84));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(a, 0xff, 0x00, 0x00), 1.00));
            return brush;
        }

        /// <summary>
        /// Creates a rainbow brush.
        /// </summary>
        /// <returns>
        /// A rainbow brush.
        /// </returns>
        public static LinearGradientBrush CreateRainbowBrush()
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            brush.GradientStops.Add(new GradientStop(Colors.Red, 0.00));
            brush.GradientStops.Add(new GradientStop(Colors.Orange, 0.17));
            brush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.33));
            brush.GradientStops.Add(new GradientStop(Colors.Green, 0.50));
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 0.67));
            brush.GradientStops.Add(new GradientStop(Colors.Indigo, 0.84));
            brush.GradientStops.Add(new GradientStop(Colors.Violet, 1.00));
            return brush;
        }

        #endregion
    }
}