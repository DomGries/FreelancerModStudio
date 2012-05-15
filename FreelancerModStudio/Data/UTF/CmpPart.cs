using System.Windows.Media.Media3D;

namespace FreelancerModStudio.Data.UTF
{
    public class CmpPart
    {
        public string ParentName;
        public string ChildName;
        public Vector3D Origin;
        public Matrix3D Matrix;
        public Vector3D AxisRotation;
        public float Min;
        public float Max;
    }
}
