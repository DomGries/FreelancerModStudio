using System;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    public class Ray3D
    {
        public Point3D Origin { get; set; }
        public Vector3D Direction { get; set; }

        public Point3D PlaneIntersection(Point3D position, Vector3D normal)
        {
            var r0 = new Vector3D(Origin.X, Origin.Y, Origin.Z);
            var p0 = new Vector3D(position.X, position.Y, position.Z);
            double d = Vector3D.DotProduct(p0, normal);
            double ddN = Vector3D.DotProduct(Direction, normal);

            if (Math.Abs(ddN) < double.Epsilon)
                return new Point3D(double.NaN, double.NaN, double.NaN);

            double t = -(Vector3D.DotProduct(r0, normal) + d) / ddN;
            return Origin + t * Direction;
        }
    }
}
