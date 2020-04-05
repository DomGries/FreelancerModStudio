namespace FreelancerModStudio.SystemDesigner.Content
{
    using global::System;
    using global::System.Windows.Media.Media3D;
    using FreelancerModStudio.Data;

    public abstract class ContentBase : ModelVisual3D
    {
        public TableBlock Block;
        public Vector3D Position;
        public Vector3D Rotation;
        public Vector3D Scale;

        public abstract Rect3D GetShapeBounds();
        public abstract bool IsEmissive();
        protected abstract Model3D GetShapeModel();

        public void UpdateTransform(Matrix3D matrix, bool animate)
        {
            if (this.Content != null && animate)
            {
                ContentAnimation animation = new ContentAnimation
                    {
                        OldMatrix = this.Transform.Value,
                        NewMatrix = matrix,
                    };
                Animator.Animate(this, animation);
            }

            else
                this.Transform = new MatrixTransform3D(matrix);
        }

        public void UpdateTransform(bool animate) => this.UpdateTransform(this.GetMatrix(), animate);

        public static Matrix3D RotationMatrix(Vector3D rotation)
        {
            return CreateRotationMatrix(new Quaternion(new Vector3D(1, 0, 0), rotation.X)) *
                   CreateRotationMatrix(new Quaternion(new Vector3D(0, 0, 1), rotation.Z)) *
                   CreateRotationMatrix(new Quaternion(new Vector3D(0, 1, 0), rotation.Y));
        }

        public static Vector3D GetRotation(Matrix3D matrix)
        {
            const double RadToDeg = 180 / Math.PI;

            if (matrix.M12 >= 1)
            {
                // Not a unique solution: thetaY - thetaX = atan2(M31, M33)
                return new Vector3D(
                    Math.Atan2(matrix.M31, matrix.M33) * RadToDeg,
                    0,
                    0.5 * Math.PI * RadToDeg);
            }

            if (matrix.M12 <= -1)
            {
                // Not a unique solution: thetaY + thetaX = atan2(M31, M33)
                return new Vector3D(
                    Math.Atan2(matrix.M31, matrix.M33) * RadToDeg,
                    0,
                    -0.5 * Math.PI * RadToDeg);
            }

            return new Vector3D(
                Math.Atan2(-matrix.M32, matrix.M22) * RadToDeg,
                Math.Atan2(-matrix.M13, matrix.M11) * RadToDeg,
                Math.Asin(matrix.M12) * RadToDeg);
        }

        public void LoadModel() => this.Content = this.GetShapeModel();
        public Point3D GetPositionPoint() => new Point3D(this.Position.X, this.Position.Y, this.Position.Z);

        protected virtual Matrix3D GetMatrix()
        {
            Matrix3D matrix = new Matrix3D();

            matrix.Scale(this.Scale);
            matrix *= RotationMatrix(this.Rotation);
            matrix.Translate(this.Position);

            return matrix;
        }

        private static Matrix3D CreateRotationMatrix(Quaternion value)
        {
            Matrix3D matrix = Matrix3D.Identity;
            matrix.Rotate(value);
            return matrix;
        }
    }
}
