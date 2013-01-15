using System;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// Represents a 3D ray.
    /// </summary>
    public class Ray3D
    {
        /// <summary>
        ///   Gets or sets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public Vector3D Direction { get; set; }

        /// <summary>
        ///   Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Point3D Origin { get; set; }

        /// <summary>
        /// Finds the intersection with a plane.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="normal">The normal.</param>
        /// <returns>The intersection point.</returns>
        public Point3D? PlaneIntersection(Point3D position, Vector3D normal)
        {
            // http://paulbourke.net/geometry/planeline/
            double dn = Vector3D.DotProduct(normal, this.Direction);
            if (dn == 0)
            {
                return null;
            }

            double u = Vector3D.DotProduct(normal, position - this.Origin) / dn;
            return this.Origin + u * this.Direction;
        }
    }
}
