using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter
{
    public class ContentAnimation
    {
        public Matrix3D OldMatrix { get; set; }
        public Matrix3D NewMatrix { get; set; }
    }

    public static class Animator
    {
        public static Duration AnimationDuration = new Duration(TimeSpan.Zero);
        public static double AnimationAccelerationRatio = 0.3;
        public static double AnimationDecelerationRatio = 0.5;

        public static void Animate(ModelVisual3D model, ContentAnimation animation)
        {
            if (AnimationDuration.TimeSpan == TimeSpan.Zero)
                model.Transform = new MatrixTransform3D(animation.NewMatrix);
            else
                model.Transform = AnimateMatrix(animation.OldMatrix, animation.NewMatrix);
        }

        static MatrixTransform3D AnimateMatrix(Matrix3D oldMatrix, Matrix3D newMatrix)
        {
            Matrix3DAnimation animationMatrix = new Matrix3DAnimation(oldMatrix, newMatrix, AnimationDuration)
            {
                AccelerationRatio = AnimationAccelerationRatio,
                DecelerationRatio = AnimationDecelerationRatio,
                FillBehavior = FillBehavior.HoldEnd
            };

            MatrixTransform3D transform = new MatrixTransform3D(oldMatrix);
            transform.BeginAnimation(MatrixTransform3D.MatrixProperty, animationMatrix);

            return transform;
        }
    }
}
