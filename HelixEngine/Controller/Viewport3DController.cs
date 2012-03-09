using System.Windows;
using System.Windows.Controls;

namespace HelixEngine
{
    public class Viewport3DController : ContentControl
    {
        public CameraRotationMode CameraRotationMode
        {
            get { return (CameraRotationMode)GetValue(CameraRotationModeProperty); }
            set { SetValue(CameraRotationModeProperty, value); }
        }

        public static readonly DependencyProperty CameraRotationModeProperty =
            DependencyProperty.Register("CameraRotationMode", typeof(CameraRotationMode), typeof(Viewport3DController), new UIPropertyMetadata(CameraRotationMode.Turntable));

        /// <summary>
        /// Select the mode 
        /// - inspect/examine (moves closer to target when zooming)
        /// - walkaround (moves camera when zooming)
        /// - fixedposition (changes fov when zooming)
        /// </summary>
        public CameraMode CameraMode
        {
            get { return (CameraMode)GetValue(CameraModeProperty); }
            set { SetValue(CameraModeProperty, value); }
        }

        public static readonly DependencyProperty CameraModeProperty =
            DependencyProperty.Register("CameraMode", typeof(CameraMode), typeof(Viewport3DController), new UIPropertyMetadata(CameraMode.Inspect));



        public bool InfiniteSpin
        {
            get { return (bool)GetValue(InfiniteSpinProperty); }
            set { SetValue(InfiniteSpinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InfiniteSpin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfiniteSpinProperty =
            DependencyProperty.Register("InfiniteSpin", typeof(bool), typeof(Viewport3DController), new UIPropertyMetadata(false));

        private CameraController _cameraController;

        private const string partCameraControl = "PART_CameraControl";

        static Viewport3DController()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Viewport3DController), new FrameworkPropertyMetadata(typeof(Viewport3DController)));
        }

        public override void OnApplyTemplate()
        {
            if (_cameraController == null)
            {
                _cameraController = Template.FindName(partCameraControl, this) as CameraController;
                if (_cameraController != null)
                    _cameraController.Viewport = this.Content as Viewport3D;
            }
            base.OnApplyTemplate();
        }
    }
}
