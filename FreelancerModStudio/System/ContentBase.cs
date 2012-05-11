using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter
{
    public abstract class ContentBase : ModelVisual3D
    {
        public int ID { get; set; }

        protected abstract Model3D GetShapeModel();
        public abstract Vector3D GetBaseScale();
        public abstract bool IsEmissive();

        public void SetDisplay(Matrix3D matrix)
        {
            if (Content != null)
            {
                ContentAnimation animation = new ContentAnimation
                    {
                        OldMatrix = Transform.Value,
                        NewMatrix = matrix,
                    };
                Animator.Animate(this, animation);
            }
            else
            {
                Transform = new MatrixTransform3D(matrix);
            }
        }

        public void SetDisplay(Vector3D position, Vector3D rotation, Vector3D scale)
        {
            SetDisplay(GetMatrix(position, rotation, scale));
        }

        Matrix3D GetMatrix(Vector3D position, Vector3D rotation, Vector3D scale)
        {
            Matrix3D matrix = new Matrix3D();

            matrix.Scale(scale);

            matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), rotation.X));
            matrix.Rotate(new Quaternion(new Vector3D(0, 1, 0)*matrix, rotation.Y));
            matrix.Rotate(new Quaternion(new Vector3D(0, 0, 1)*matrix, rotation.Z));

            matrix.Translate(position);

            return matrix;
        }

        public void LoadModel()
        {
            Content = GetShapeModel();
        }

        public Vector3D GetPosition()
        {
            Matrix3D matrix = Transform.Value;
            return new Vector3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);
        }

        public Point3D GetPositionPoint()
        {
            Matrix3D matrix = Transform.Value;
            return new Point3D(matrix.OffsetX, matrix.OffsetY, matrix.OffsetZ);
        }
    }
}
