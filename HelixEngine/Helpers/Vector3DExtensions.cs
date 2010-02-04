using System.Windows.Media.Media3D;
using System;

namespace HelixEngine
{
    public static class Vector3DExtensions
    {
        /// <summary>
        /// Find a <see cref="Vector3D"/> that is perpendicular to the given <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector3D FindAnyPerpendicular(this Vector3D n)
        {
            Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), n);
            if (u.LengthSquared < 1e-3)
                u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), n);
            return u;
        }

        /// <summary>
        /// Convert a <see cref="Point3D"/> to a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector3D ToVector3D(this Point3D n)
        {
            return new Vector3D(n.X, n.Y, n.Z);
        }

        /// <summary>
        /// Convert a <see cref="Vector3D"/> to a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Point3D ToPoint3D(this Vector3D n)
        {
            return new Point3D(n.X, n.Y, n.Z);
        }

        /// <summary>
        /// Returns the highest number.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Max(this Vector3D n)
        {
            return Math.Max(Math.Max(n.X, n.Y), n.Z);
        }

    }
}
