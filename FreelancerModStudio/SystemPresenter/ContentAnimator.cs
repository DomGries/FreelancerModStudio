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
    public static class ContentAnimator
    {
        public static Duration AnimationDuration = new Duration(TimeSpan.FromMilliseconds(2000));
        public static double AnimationAccelerationRatio = 0.3;
        public static double AnimationDecelerationRatio = 0.5;

        public static void SetPosition(ModelVisual3D model, Vector3D oldPosition, Vector3D newPosition, bool always)
        {
            if (always || newPosition != oldPosition)
            {
                if (AnimationDuration.TimeSpan == TimeSpan.Zero)
                    ContentAnimator.AddTransformation(model, new TranslateTransform3D(newPosition));
                else
                    AnimatePosition(model, oldPosition, newPosition);
            }
        }

        public static void SetRotation(ModelVisual3D model, Rotation3D oldRotation, Rotation3D newRotation, Vector3D center, bool always)
        {
            if (always || !RotationEquals(newRotation, oldRotation))
            {
                if (AnimationDuration.TimeSpan == TimeSpan.Zero)
                    ContentAnimator.AddTransformation(model, new RotateTransform3D(newRotation));
                else
                    AnimateRotation(model, oldRotation, newRotation, center);
            }
        }

        private static bool RotationEquals(Rotation3D rotationX, Rotation3D rotationY)
        {
            AxisAngleRotation3D x = (AxisAngleRotation3D)rotationX;
            AxisAngleRotation3D y = (AxisAngleRotation3D)rotationY;
            bool angleEqual = x.Angle == y.Angle;
            if (angleEqual)
                return x.Axis == y.Axis;

            return angleEqual;
        }

        public static void SetScale(ModelVisual3D model, Vector3D oldScale, Vector3D newScale, Vector3D center, bool always)
        {
            if (always || newScale != oldScale)
            {
                if (AnimationDuration.TimeSpan == TimeSpan.Zero)
                    ContentAnimator.AddTransformation(model, new ScaleTransform3D(newScale));
                else
                    AnimateScale(model, oldScale, newScale, center);
            }
        }

        public static void AddTransformation(ModelVisual3D model, Transform3D value)
        {
            Transform3DGroup group = new Transform3DGroup();
            group.Children.Add(model.Transform);

            //add new transform
            group.Children.Add(value);

            //add sum of all transforms
            model.Transform = new MatrixTransform3D(group.Value);
        }

        public static void AnimatePosition(ModelVisual3D model, Vector3D oldPosition, Vector3D newPosition)
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

            AddTransformationAnimation(model, transform);
        }

        public static void AnimateRotation(ModelVisual3D model, Rotation3D oldRotation, Rotation3D newRotation, Vector3D center)
        {
            RotateTransform3D transform = new RotateTransform3D(oldRotation, center.ToPoint3D());

            Rotation3DAnimation animation = new Rotation3DAnimation(oldRotation, newRotation, AnimationDuration)
            {
                AccelerationRatio = AnimationAccelerationRatio,
                DecelerationRatio = AnimationDecelerationRatio,
                FillBehavior = FillBehavior.HoldEnd
            };

            transform.BeginAnimation(RotateTransform3D.RotationProperty, animation);

            AddTransformationAnimation(model, transform);
        }

        public static void AnimateScale(ModelVisual3D model, Vector3D oldScale, Vector3D newScale, Vector3D center)
        {
            ScaleTransform3D transform = new ScaleTransform3D(oldScale, center.ToPoint3D());

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

            AddTransformationAnimation(model, transform);
        }

        public static void AddTransformationAnimation(ModelVisual3D model, Transform3D value)
        {
            if (model.Transform is Transform3DGroup == false)
                model.Transform = new Transform3DGroup();

            Type type = value.GetType();
            Transform3DGroup transformGroup = (Transform3DGroup)model.Transform;
            for (int i = 0; i < transformGroup.Children.Count; i++)
            {
                if (transformGroup.Children[i].GetType() == type)
                {
                    transformGroup.Children[i] = value;
                    return;
                }
            }
            transformGroup.Children.Add(value);
        }
    }
}
