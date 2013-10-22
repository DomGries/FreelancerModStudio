// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LineGeometryBuilder.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixEngine
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;

    /// <summary>
    /// Builds a mesh geometry for a collection of line segments.
    /// </summary>
    public class LineGeometryBuilder : ScreenGeometryBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineGeometryBuilder"/> class.
        /// </summary>
        /// <param name="visual">
        /// The visual parent of the geometry (the transform is calculated from this object).
        /// </param>
        public LineGeometryBuilder(Visual3D visual)
            : base(visual)
        {
        }

        /// <summary>
        /// Creates the triangle indices.
        /// </summary>
        /// <param name="n">
        /// The number of points.
        /// </param>
        /// <returns>
        /// Triangle indices.
        /// </returns>
        public Int32Collection CreateIndices(int n)
        {
            var indices = new Int32Collection(n * 3);

            for (int i = 0; i < n / 2; i++)
            {
                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 1);
                indices.Add(i * 4 + 0);

                indices.Add(i * 4 + 2);
                indices.Add(i * 4 + 3);
                indices.Add(i * 4 + 1);
            }

            indices.Freeze();
            return indices;
        }

        /// <summary>
        /// Creates the positions for the specified line segments.
        /// </summary>
        /// <param name="points">
        /// The points of the line segments.
        /// </param>
        /// <param name="thickness">
        /// The thickness of the line.
        /// </param>
        /// <param name="depthOffset">
        /// The depth offset. A positive number (e.g. 0.0001) moves the point towards the camera.
        /// </param>
        /// <param name="clipping">
        /// The clipping.
        /// </param>
        /// <returns>
        /// The positions collection.
        /// </returns>
        public Point3DCollection CreatePositions(
            IList<Point3D> points,
            double thickness,
            double depthOffset,
            double fixedLength,
            CohenSutherlandClipping clipping)
        {
            double halfThickness = thickness * 0.5;
            int segmentCount = points.Count / 2;

            var positions = new Point3DCollection(segmentCount * 4);

            for (int i = 0; i < segmentCount; i++)
            {
                int startIndex = i * 2;

                Point3D startPoint = points[startIndex];
                Point3D endPoint = points[startIndex + 1];

                var screenStartPoint = (Point4D)startPoint * this.visualToScreen;
                var screenEndPoint = (Point4D)endPoint * this.visualToScreen;

                if (fixedLength > 0)
                {
                    Point3D screenStartPoint3D = startPoint * this.visualToScreen;
                    Point3D screenEndPoint3D = endPoint * this.visualToScreen;
                    double deltaScreenPointX = screenEndPoint3D.X - screenStartPoint3D.X;
                    double deltaScreenPointY = screenEndPoint3D.Y - screenStartPoint3D.Y;
                    double magnitudeScreenPoint = Math.Sqrt(deltaScreenPointX * deltaScreenPointX + deltaScreenPointY * deltaScreenPointY);
                    deltaScreenPointX *= fixedLength * screenEndPoint.W / magnitudeScreenPoint;
                    deltaScreenPointY *= fixedLength * screenEndPoint.W / magnitudeScreenPoint;

                    screenEndPoint = screenStartPoint;
                    screenEndPoint.X += deltaScreenPointX;
                    screenEndPoint.Y += deltaScreenPointY;
                }

                if (clipping != null)
                {
                    double x0 = screenStartPoint.X / screenStartPoint.W;
                    double y0 = screenStartPoint.Y / screenStartPoint.W;
                    double x1 = screenEndPoint.X / screenEndPoint.W;
                    double y1 = screenEndPoint.Y / screenEndPoint.W;

                    if (!clipping.ClipLine(ref x0, ref y0, ref x1, ref y1))
                    {
                        continue;
                    }

                    screenStartPoint.X = x0 * screenStartPoint.W;
                    screenStartPoint.Y = y0 * screenStartPoint.W;
                    screenEndPoint.X = x1 * screenEndPoint.W;
                    screenEndPoint.Y = y1 * screenEndPoint.W;
                }

                double lx = screenEndPoint.X / screenEndPoint.W - screenStartPoint.X / screenStartPoint.W;
                double ly = screenEndPoint.Y / screenEndPoint.W - screenStartPoint.Y / screenStartPoint.W;
                double m = halfThickness / Math.Sqrt(lx * lx + ly * ly);

                double deltaX = -ly * m;
                double deltaY = lx * m;

                positions.Add(Widen(screenStartPoint, deltaX, deltaY, depthOffset));
                positions.Add(Widen(screenStartPoint, -deltaX, -deltaY, depthOffset));
                positions.Add(Widen(screenEndPoint, deltaX, deltaY, depthOffset));
                positions.Add(Widen(screenEndPoint, -deltaX, -deltaY, depthOffset));
            }

            positions.Freeze();
            return positions;
        }

        Point3D Widen(Point4D point, double deltaX, double deltaY, double depthOffset)
        {
            point.X += deltaX * point.W;
            point.Y += deltaY * point.W;

            if (depthOffset != 0)
            {
                point.Z -= depthOffset;
                point *= this.screenToVisual;
                return new Point3D(point.X / point.W, point.Y / point.W, point.Z / point.W);
            }

            point *= this.screenToVisual;
            return new Point3D(point.X, point.Y, point.Z);
        }
    }
}