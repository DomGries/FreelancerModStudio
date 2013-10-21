// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BoundingBoxWireFrameVisual3D.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixEngine
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// A visual element that shows a wireframe for the specified bounding box.
    /// </summary>
    public class LineVisual3D : LinesVisual3D
    {
        /// <summary>
        /// Identifies the <see cref="Point1"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Point1Property = DependencyProperty.Register(
            "Point1", typeof(Point3D), typeof(LineVisual3D), new UIPropertyMetadata(new Point3D(), PointChanged));

        /// <summary>
        /// Gets or sets the point 1.
        /// </summary>
        /// <value> The point 1. </value>
        public Point3D Point1
        {
            get
            {
                return (Point3D)this.GetValue(Point1Property);
            }

            set
            {
                this.SetValue(Point1Property, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Point2"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Point2Property = DependencyProperty.Register(
            "Point2", typeof(Point3D), typeof(LineVisual3D), new UIPropertyMetadata(new Point3D(), PointChanged));

        /// <summary>
        /// Gets or sets the point 1.
        /// </summary>
        /// <value> The point 1. </value>
        public Point3D Point2
        {
            get
            {
                return (Point3D)this.GetValue(Point2Property);
            }

            set
            {
                this.SetValue(Point2Property, value);
            }
        }

        /// <summary>
        /// Updates the box.
        /// </summary>
        protected virtual void OnPointChanged()
        {
            this.Points.Clear();
            this.Points.Add(Point1);
            this.Points.Add(Point2);

            UpdateGeometry();
        }

        /// <summary>
        /// Called when the points were changed.
        /// </summary>
        /// <param name="d">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void PointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LineVisual3D)d).OnPointChanged();
        }
    }
}