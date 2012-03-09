using System.Windows.Media.Media3D;
using System;

namespace HelixEngine
{
    /// <summary>
    /// Extension methods for <see cref="Vector3D"/>.
    /// </summary>
    public static class Vector3DExtensions
    {
        #region Public Methods

        /// <summary>
        /// Find a <see cref="Vector3D"/> that is perpendicular to the given <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="n">
        /// The input vector.
        /// </param>
        /// <returns>
        /// A perpendicular vector.
        /// </returns>
        public static Vector3D FindAnyPerpendicular(this Vector3D n)
        {
            n.Normalize();
            Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), n);
            if (u.LengthSquared < 1e-3)
                u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), n);

            return u;
        }

        /// <summary>
        /// Convert a <see cref="Vector3D"/> to a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="n">
        /// The input vector.
        /// </param>
        /// <returns>
        /// A point. 
        /// </returns>
        public static Point3D ToPoint3D(this Vector3D n)
        {
            return new Point3D(n.X, n.Y, n.Z);
        }

        #endregion
    }
}
