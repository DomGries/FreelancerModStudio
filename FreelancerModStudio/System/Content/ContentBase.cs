using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public abstract class ContentBase : ModelVisual3D
    {
        public TableBlock Block;
        public Vector3D Position;

        protected abstract Model3D GetShapeModel();
        public abstract Rect3D GetShapeBounds();
        public abstract bool IsEmissive();

        void SetTransform(Matrix3D matrix, bool animate)
        {
            if (Content != null && animate)
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

        public void SetTransform(Vector3D position, Vector3D rotation, Vector3D scale, bool animate)
        {
            Position = position;
            SetTransform(GetMatrix(position, rotation, scale), animate);
        }

        static Matrix3D CreateRotationMatrix(Quaternion value)
        {
            Matrix3D matrix = Matrix3D.Identity;
            matrix.Rotate(value);
            return matrix;
        }

        static Matrix3D GetMatrix(Vector3D position, Vector3D rotation, Vector3D scale)
        {
            Matrix3D matrix = new Matrix3D();

            matrix.Scale(scale);

            matrix *= CreateRotationMatrix(new Quaternion(new Vector3D(1, 0, 0), rotation.X)) *
                      CreateRotationMatrix(new Quaternion(new Vector3D(0, 0, 1), rotation.Z)) *
                      CreateRotationMatrix(new Quaternion(new Vector3D(0, 1, 0), rotation.Y));

            matrix.Translate(position);

            return matrix;
        }

        public void LoadModel()
        {
            Content = GetShapeModel();
        }

        public Point3D GetPositionPoint()
        {
            return new Point3D(Position.X, Position.Y, Position.Z);
        }
    }
}
