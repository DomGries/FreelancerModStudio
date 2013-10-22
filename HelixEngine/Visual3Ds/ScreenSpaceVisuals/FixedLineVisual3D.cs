namespace HelixEngine
{
    using System.Windows;

    /// <summary>
    /// A visual element that shows a wireframe for the specified bounding box.
    /// </summary>
    public class FixedLineVisual3D : LineVisual3D
    {
        /// <summary>
        /// Identifies the <see cref="FixedLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FixedLengthProperty = DependencyProperty.Register(
            "FixedLength", typeof(double), typeof(FixedLineVisual3D), new UIPropertyMetadata(new double(), PointChanged));

        /// <summary>
        /// Gets or sets the point 1.
        /// </summary>
        /// <value> The point 1. </value>
        public double FixedLength
        {
            get
            {
                return (double)this.GetValue(FixedLengthProperty);
            }

            set
            {
                this.SetValue(FixedLengthProperty, value);
            }
        }

        /// <summary>
        /// Updates the geometry.
        /// </summary>
        protected override void UpdateGeometry()
        {
            this.Mesh.Positions = null;
            if (this.Points != null)
            {
                int n = this.Points.Count;
                if (n > 0)
                {
                    if (this.Mesh.TriangleIndices.Count != n * 3)
                    {
                        this.Mesh.TriangleIndices = this.Builder.CreateIndices(n);
                    }

                    this.Mesh.Positions = this.Builder.CreatePositions(this.Points, this.Thickness, this.DepthOffset, FixedLength, null);
                }
            }
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
            ((FixedLineVisual3D)d).OnPointChanged();
        }
    }
}