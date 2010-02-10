using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    public class CameraHelper
    {
        public static PerspectiveCamera CreateDefaultCamera()
        {
            PerspectiveCamera cam = new PerspectiveCamera();
            Reset(cam);
            return cam;
        }

        public static void Reset(PerspectiveCamera camera)
        {
            if (camera == null)
                return;

            camera.UpDirection = new Vector3D(0, 1, 0);
            camera.Position = new Point3D(0, 0, 350);

            var target = new Point3D(0, 0, 0);
            camera.LookDirection = target - camera.Position;
        }

        /// <summary>
        /// Copies all members of the source <see cref="Camera"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void Copy(PerspectiveCamera source, PerspectiveCamera dest)
        {
            if (source == null || dest == null)
                return;

            /// todo: could also use Camera.Clone?

            dest.LookDirection = source.LookDirection;
            dest.Position = source.Position;
            dest.UpDirection = source.UpDirection;
            dest.FieldOfView = source.FieldOfView;
            dest.NearPlaneDistance = source.NearPlaneDistance;
            dest.FarPlaneDistance = source.FarPlaneDistance;
        }

        /// <summary>
        /// Copy the direction of the source <see cref="Camera"/> only. Used for the CoordinateSystem view.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="distance"></param>
        public static void CopyDirectionOnly(ProjectionCamera source, ProjectionCamera dest, double distance)
        {
            if (source == null || dest == null)
                return;

            Vector3D dir = source.LookDirection;
            dir.Normalize();
            dir *= distance;

            dest.LookDirection = dir;
            dest.Position = new Point3D(-dest.LookDirection.X, -dest.LookDirection.Y, -dest.LookDirection.Z);
            dest.UpDirection = source.UpDirection;
        }

        /// <summary>
        /// Animates the camera position and direction
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="newPosition">The position to animate to</param>
        /// <param name="newDirection">The direction to animate to</param>
        /// <param name="newUpDirection">The up direction to animate to</param>
        /// <param name="animationTime">Animation time in milliseconds</param>
        public static void AnimateTo(PerspectiveCamera camera, Point3D newPosition, Vector3D newDirection, Vector3D newUpDirection, double animationTime)
        {
            var fromPosition = camera.Position;
            var fromDirection = camera.LookDirection;
            var fromUpDirection = camera.UpDirection;

            camera.Position = newPosition;
            camera.LookDirection = newDirection;
            camera.UpDirection = newUpDirection;

            if (animationTime > 0)
            {
                var a1 = new Point3DAnimation(fromPosition, newPosition,
                    new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5, FillBehavior = FillBehavior.Stop };
                camera.BeginAnimation(ProjectionCamera.PositionProperty, a1);

                var a2 = new Vector3DAnimation(fromDirection, newDirection,
                                               new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5, FillBehavior = FillBehavior.Stop };
                camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, a2);

                var a3 = new Vector3DAnimation(fromUpDirection, newUpDirection,
                                               new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5, FillBehavior = FillBehavior.Stop };
                camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, a3);
            }
        }
    }
}
