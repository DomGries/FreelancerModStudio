using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using HelixEngine.Meshes;

namespace HelixEngine
{
    /// <summary>
    /// The HelixView3D contains a camera controller, coordinate view and a view cube
    /// </summary>
    [ContentProperty("Children"), Localizability(LocalizationCategory.NeverLocalize)]
    public class HelixView3D : Control
    {
        static HelixView3D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HelixView3D),
                new FrameworkPropertyMetadata(typeof(HelixView3D)));
        }

        public HelixView3D()
        {
            _viewport = new Viewport3D { IsHitTestVisible = false };
            _viewport.Camera = CameraHelper.CreateDefaultCamera();
            _viewport.Camera.Changed += Camera_Changed;
        }

        #region Viewport/Children/Camera access properties

        public Viewport3D Viewport { get { return _viewport; } }

        public Visual3DCollection Children
        {
            get { return _viewport.Children; }
        }

        /// <summary>
        /// Access to the Camera
        /// </summary>
        public PerspectiveCamera Camera
        {
            get { return Viewport.Camera as PerspectiveCamera; }
        }

        /// <summary>
        /// Access to the camera controller
        /// </summary>
        public CameraController CameraController
        {
            get { return _cameraController; }
        }

        #endregion

        #region Custom control PARTs
        private CameraController _cameraController;
        private Viewport3D _viewport;
        private Viewport3D _viewCubeView;
        private Model3DGroup _viewCubeLights;
        private ViewCubeVisual3D _viewCube;
        public AdornerDecorator AdornerLayer;

        private const string PartAdornerLayer = "PART_AdornerLayer";
        private const string PartViewCubeView = "PART_ViewCubeView";
        private const string PartViewCube = "PART_ViewCube";
        private const string PartCameraController = "PART_CameraController";
        #endregion

        public delegate void SelectionChangedType(DependencyObject visual);
        public SelectionChangedType SelectionChanged;

        private void OnSelectionChanged(DependencyObject visual)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(visual);
        }

        private void cameraController_SelectionChanged(DependencyObject visual)
        {
            OnSelectionChanged(visual);
        }

        public override void OnApplyTemplate()
        {
            if (AdornerLayer == null)
            {
                AdornerLayer = Template.FindName(PartAdornerLayer, this) as AdornerDecorator;
                if (AdornerLayer != null)
                    AdornerLayer.Child = _viewport;
            }
            Debug.Assert(AdornerLayer != null, String.Format("{0} is missing from the template.", PartAdornerLayer));

            if (_cameraController == null)
            {
                _cameraController = Template.FindName(PartCameraController, this) as CameraController;
                if (_cameraController != null)
                {
                    _cameraController.Viewport = Viewport;
                    _cameraController.SelectionChanged += cameraController_SelectionChanged;
                }
            }
            Debug.Assert(_cameraController != null, String.Format("{0} is missing from the template.", PartCameraController));

            if (_viewCubeView == null)
            {
                _viewCubeView = Template.FindName(PartViewCubeView, this) as Viewport3D;

                _viewCubeLights = new Model3DGroup();
                _viewCubeLights.Children.Add(new AmbientLight(Colors.White));
                if (_viewCubeView != null)
                {
                    _viewCubeView.Camera = new PerspectiveCamera();
                    _viewCubeView.Children.Add(new ModelVisual3D() { Content = _viewCubeLights });
                    _viewCubeView.MouseEnter += _viewCubeView_MouseEnter;
                    _viewCubeView.MouseLeave += _viewCubeView_MouseLeave;
                }
                _viewCube = Template.FindName(PartViewCube, this) as ViewCubeVisual3D;
                if (_viewCube != null)
                    _viewCube.Viewport = Viewport;
            }

            // update the coordinateview camera
            OnCameraChanged();

            // add the default headlight
            base.OnApplyTemplate();
        }

        #region Annotation properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(HelixView3D), new UIPropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public bool ShowViewCube
        {
            get { return (bool)GetValue(ShowViewCubeProperty); }
            set { SetValue(ShowViewCubeProperty, value); }
        }

        public static readonly DependencyProperty ShowViewCubeProperty =
            DependencyProperty.Register("ShowViewCube", typeof(bool), typeof(HelixView3D), new UIPropertyMetadata(false));

        #endregion

        #region Camera properties

        public static readonly DependencyProperty CameraModeProperty =
            DependencyProperty.Register("CameraMode", typeof(CameraMode), typeof(HelixView3D),
                                        new UIPropertyMetadata(CameraMode.Inspect));

        public static readonly DependencyProperty CameraRotationModeProperty =
            DependencyProperty.Register("CameraRotationMode", typeof(CameraRotationMode), typeof(HelixView3D),
                                        new UIPropertyMetadata(CameraRotationMode.TwoAxis));

        public bool InfiniteSpin
        {
            get { return (bool)GetValue(InfiniteSpinProperty); }
            set { SetValue(InfiniteSpinProperty, value); }
        }

        public static readonly DependencyProperty InfiniteSpinProperty =
            DependencyProperty.Register("InfiniteSpin", typeof(bool), typeof(HelixView3D), new UIPropertyMetadata(false));

        /// <summary>
        /// Select rotation by two-axis or virtual trackball
        /// </summary>
        public CameraRotationMode CameraRotationMode
        {
            get { return (CameraRotationMode)GetValue(CameraRotationModeProperty); }
            set { SetValue(CameraRotationModeProperty, value); }
        }

        /// <summary>
        /// Selected <see cref="CameraMode"/>
        /// </summary>
        public CameraMode CameraMode
        {
            get { return (CameraMode)GetValue(CameraModeProperty); }
            set { SetValue(CameraModeProperty, value); }
        }

        #endregion

        #region Camera changed event

        public static readonly RoutedEvent CameraChangedEvent =
            EventManager.RegisterRoutedEvent("CameraChanged",
                                             RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HelixView3D));

        /// <summary>
        /// Event when a property has been changed
        /// </summary>
        public event RoutedEventHandler CameraChanged
        {
            add { AddHandler(CameraChangedEvent, value); }
            remove { RemoveHandler(CameraChangedEvent, value); }
        }

        protected virtual void RaiseCameraChangedEvent()
        {
            // e.Handled = true;
            var args = new RoutedEventArgs(CameraChangedEvent);
            RaiseEvent(args);
        }

        #endregion

        private void Camera_Changed(object sender, EventArgs e)
        {
            // Raise notification from the SpaceView
            RaiseCameraChangedEvent();
            // Update the CoordinateView camera and the headlight direction
            OnCameraChanged();
        }

        public virtual void OnCameraChanged()
        {
            // update the camera of the view cube
            if (_viewCubeView != null)
                CameraHelper.CopyDirectionOnly(Camera, _viewCubeView.Camera as PerspectiveCamera, 20);
        }

        #region View cube mouse enter/leave opacity animation
        void _viewCubeView_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateOpacity(_viewCubeView, 0.5, 200);
        }

        private void AnimateOpacity(UIElement obj, double toOpacity, double animationTime)
        {
            var a = new DoubleAnimation(toOpacity,
                                            new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5 };
            obj.BeginAnimation(OpacityProperty, a);
        }

        void _viewCubeView_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateOpacity(_viewCubeView, 1.0, 200);
        }
        #endregion

        public void LookAt(Point3D p)
        {
            LookAt(p, 0);
        }

        public void LookAt(Point3D p, double animationTime)
        {
            Debug.Assert(CameraController != null, "CameraController not defined");
            if (CameraController != null)
                CameraController.LookAt(p, animationTime);
        }

        public void LookAt(Point3D p, double distance, double animationTime)
        {
            Debug.Assert(CameraController != null, "CameraController not defined");
            if (CameraController != null)
                CameraController.LookAt(p, distance, animationTime);
        }

        public void LookAt(Point3D p, Vector3D direction, double animationTime)
        {
            Debug.Assert(CameraController != null, "CameraController not defined");
            if (CameraController != null)
                CameraController.LookAt(p, direction, animationTime);
        }

        public void SetView(Point3D newPosition, Vector3D newDirection, Vector3D newUpDirection, double animationTime)
        {
            CameraHelper.AnimateTo(Camera, newPosition, newDirection, newUpDirection, animationTime);
        }

        public void Add(Visual3D v)
        {
            if (!Viewport.Children.Contains(v))
                Viewport.Children.Add(v);
        }

        public void Remove(Visual3D v)
        {
            if (Viewport.Children.Contains(v))
                Viewport.Children.Remove(v);
        }

        public void Save(string fileName)
        {
            Viewport3DHelper.Save(Viewport, fileName);
        }

        public void Copy()
        {
            Viewport3DHelper.Copy(Viewport, Viewport.ActualWidth * 2, Viewport.ActualHeight * 2, Brushes.White);
        }

        public void CopyXaml()
        {
            Clipboard.SetText(Viewport3DHelper.GetXaml(Viewport));
        }

        public void ZoomToFit()
        {
            // Viewport3DHelper.ZoomToFit(this);
            // CameraController.ZoomToFit();
        }

        public Visual3D FindNearestVisual(Point pt)
        {
            return Viewport3DHelper.FindNearestVisual(Viewport, pt);
        }

        public Point3D? FindNearestPoint(Point pt)
        {
            return Viewport3DHelper.FindNearestPoint(Viewport, pt);
        }

        public bool FindNearest(Point pt, out Point3D pos, out Vector3D normal, out DependencyObject obj)
        {
            return Viewport3DHelper.FindNearest(Viewport, pt, out pos, out normal, out obj);
        }

    }
}