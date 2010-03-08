using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media.Animation;
using HelixEngine;

namespace FreelancerModStudio.SystemPresenter
{
    public class ContentAnimation
    {
        public Vector3D OldPosition { get; set; }
        public Rotation3D OldRotation { get; set; }
        public Vector3D OldScale { get; set; }

        public Vector3D NewPosition { get; set; }
        public Rotation3D NewRotation { get; set; }
        public Vector3D NewScale { get; set; }
    }

    public static class Animator
    {
        public static Duration AnimationDuration = new Duration(TimeSpan.Zero);
        public static double AnimationAccelerationRatio = 0.3;
        public static double AnimationDecelerationRatio = 0.5;

        public static void Animate(ModelVisual3D model, ContentAnimation animation)
        {
            Transform3DGroup transform = new Transform3DGroup();
            if (AnimationDuration.TimeSpan == TimeSpan.Zero)
            {
                transform.Children.Add(new ScaleTransform3D(animation.NewScale));
                transform.Children.Add(new RotateTransform3D(animation.NewRotation));
                transform.Children.Add(new TranslateTransform3D(animation.NewPosition));
                model.Transform = new MatrixTransform3D(transform.Value);
            }
            else
            {
                transform.Children.Add(AnimateScale(animation.OldScale, animation.NewScale));
                transform.Children.Add(AnimateRotation(animation.OldRotation, animation.NewRotation));
                transform.Children.Add(AnimatePosition(animation.OldPosition, animation.NewPosition));

                model.Transform = transform;
            }
        }

        static TranslateTransform3D AnimatePosition(Vector3D oldPosition, Vector3D newPosition)
        {
            TranslateTransform3D transform = new TranslateTransform3D(oldPosition);

            if (newPosition.X != oldPosition.X)
            {
                DoubleAnimation animationX = new DoubleAnimation(oldPosition.X, newPosition.X, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(TranslateTransform3D.OffsetXProperty, animationX);
            }

            if (newPosition.Y != oldPosition.Y)
            {
                DoubleAnimation animationY = new DoubleAnimation(oldPosition.Y, newPosition.Y, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(TranslateTransform3D.OffsetYProperty, animationY);
            }

            if (newPosition.Z != oldPosition.Z)
            {
                DoubleAnimation animationZ = new DoubleAnimation(oldPosition.Z, newPosition.Z, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(TranslateTransform3D.OffsetZProperty, animationZ);
            }

            return transform;
        }

        static RotateTransform3D AnimateRotation(Rotation3D oldRotation, Rotation3D newRotation)
        {
            RotateTransform3D transform = new RotateTransform3D(oldRotation);

            Rotation3DAnimation animation = new Rotation3DAnimation(oldRotation, newRotation, AnimationDuration)
            {
                AccelerationRatio = AnimationAccelerationRatio,
                DecelerationRatio = AnimationDecelerationRatio,
                FillBehavior = FillBehavior.HoldEnd
            };

            transform.BeginAnimation(RotateTransform3D.RotationProperty, animation);

            return transform;
        }

        static ScaleTransform3D AnimateScale(Vector3D oldScale, Vector3D newScale)
        {
            ScaleTransform3D transform = new ScaleTransform3D(oldScale);

            if (newScale.X != oldScale.X)
            {
                DoubleAnimation animationX = new DoubleAnimation(oldScale.X, newScale.X, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(ScaleTransform3D.ScaleXProperty, animationX);
            }

            if (newScale.Y != oldScale.Y)
            {
                DoubleAnimation animationY = new DoubleAnimation(oldScale.Y, newScale.Y, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(ScaleTransform3D.ScaleYProperty, animationY);
            }

            if (newScale.Z != oldScale.Z)
            {
                DoubleAnimation animationZ = new DoubleAnimation(oldScale.Z, newScale.Z, AnimationDuration)
                {
                    AccelerationRatio = AnimationAccelerationRatio,
                    DecelerationRatio = AnimationDecelerationRatio,
                    FillBehavior = FillBehavior.HoldEnd
                };
                transform.BeginAnimation(ScaleTransform3D.ScaleZProperty, animationZ);
            }

            return transform;
        }
    }
}
