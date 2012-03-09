using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    public enum MouseAction
    {
        None,
        Pan,
        Zoom,
        Rotate,
        ShowContextMenu,
        ResetCamera,
        ChangeLookAt,
        Select,
        SelectFarest
    }

    public class CameraController : Border
    {
        /// <summary>
        ///   The camera property.
        /// </summary>
        public static readonly DependencyProperty CameraProperty = DependencyProperty.Register(
            "Camera",
            typeof(PerspectiveCamera),
            typeof(CameraController),
            new UIPropertyMetadata(null));

        public static readonly DependencyProperty FixedMouseDownPointProperty =
            DependencyProperty.Register("FixedMouseDownPoint", typeof(bool), typeof(CameraController),
                                        new UIPropertyMetadata(false));

        /// <summary>
        ///   The inertia factor property.
        /// </summary>
        public static readonly DependencyProperty InertiaFactorProperty = DependencyProperty.Register(
            "InertiaFactor", typeof(double), typeof(CameraController), new UIPropertyMetadata(0.9));

        /// <summary>
        ///   The infinite spin property.
        /// </summary>
        public static readonly DependencyProperty InfiniteSpinProperty = DependencyProperty.Register(
            "InfiniteSpin", typeof(bool), typeof(CameraController), new UIPropertyMetadata(false));

        /// <summary>
        ///   The is pan enabled property.
        /// </summary>
        public static readonly DependencyProperty IsPanEnabledProperty = DependencyProperty.Register(
            "IsPanEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        ///   The is zoom enabled property.
        /// </summary>
        public static readonly DependencyProperty IsZoomEnabledProperty = DependencyProperty.Register(
            "IsZoomEnabled", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        ///   The show camera target property.
        /// </summary>
        public static readonly DependencyProperty ShowCameraTargetProperty =
            DependencyProperty.Register(
                "ShowCameraTarget", typeof(bool), typeof(CameraController), new UIPropertyMetadata(true));

        /// <summary>
        ///   The spin release time property.
        /// </summary>
        public static readonly DependencyProperty SpinReleaseTimeProperty =
            DependencyProperty.Register(
                "SpinReleaseTime", typeof(int), typeof(CameraController), new UIPropertyMetadata(200));

        /// <summary>
        ///   The camera mode property.
        /// </summary>
        public static readonly DependencyProperty CameraModeProperty = DependencyProperty.Register(
            "CameraMode", typeof(CameraMode), typeof(CameraController), new UIPropertyMetadata(CameraMode.Inspect));

        /// <summary>
        ///   The camera rotation mode property.
        /// </summary>
        public static readonly DependencyProperty CameraRotationModeProperty =
            DependencyProperty.Register(
                "CameraRotationMode",
                typeof(CameraRotationMode),
                typeof(CameraController),
                new UIPropertyMetadata(CameraRotationMode.Turntable));

        public CameraController()
        {
            LeftButtonAction = MouseAction.Select;
            LeftDoubleClickAction = MouseAction.Select;
            ShiftLeftButtonAction = MouseAction.SelectFarest;
            ControlLeftButtonAction = MouseAction.Select;

            RightButtonAction = MouseAction.Rotate;
            RightDoubleClickAction = MouseAction.ChangeLookAt;
            ShiftRightButtonAction = MouseAction.Pan;
            ControlRightButtonAction = MouseAction.Zoom;

            MiddleButtonAction = MouseAction.Pan;
            MiddleDoubleClickAction = MouseAction.ResetCamera;
            ShiftMiddleButtonAction = MouseAction.Pan;
            ControlMiddleButtonAction = MouseAction.Pan;

            Background = Brushes.Transparent;
            EventSurface = this;
            SubscribeEvents();

            _watch.Start();
            lastTick = _watch.ElapsedTicks;
        }

        /// <summary>
        ///   Gets or sets InertiaFactor.
        /// </summary>
        public double InertiaFactor
        {
            get
            {
                return (double)this.GetValue(InertiaFactorProperty);
            }

            set
            {
                this.SetValue(InertiaFactorProperty, value);
            }
        }

        /// <summary>
        ///   Max duration of mouse drag to activate spin
        /// </summary>
        /// <remarks>
        ///   If the time between mouse down and mouse up is less than this value, spin is activated.
        /// </remarks>
        public int SpinReleaseTime
        {
            get
            {
                return (int)this.GetValue(SpinReleaseTimeProperty);
            }

            set
            {
                this.SetValue(SpinReleaseTimeProperty, value);
            }
        }


        /// <summary>
        ///   Gets or sets a value indicating whether InfiniteSpin.
        /// </summary>
        public bool InfiniteSpin
        {
            get
            {
                return (bool)this.GetValue(InfiniteSpinProperty);
            }

            set
            {
                this.SetValue(InfiniteSpinProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether IsPanEnabled.
        /// </summary>
        public bool IsPanEnabled
        {
            get
            {
                return (bool)this.GetValue(IsPanEnabledProperty);
            }

            set
            {
                this.SetValue(IsPanEnabledProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether IsZoomEnabled.
        /// </summary>
        public bool IsZoomEnabled
        {
            get
            {
                return (bool)this.GetValue(IsZoomEnabledProperty);
            }

            set
            {
                this.SetValue(IsZoomEnabledProperty, value);
            }
        }

        /// <summary>
        ///   The is spinning flag.
        /// </summary>
        private bool isSpinning;

        /// <summary>
        ///   The last tick.
        /// </summary>
        private long lastTick;

        /// <summary>
        ///   The pan speed.
        /// </summary>
        private Vector3D panSpeed;

        /// <summary>
        ///   The rotation speed.
        /// </summary>
        private Vector rotationSpeed;

        /// <summary>
        ///   The spinning speed.
        /// </summary>
        private Vector spinningSpeed;

        /// <summary>
        ///   The target adorner.
        /// </summary>
        private Adorner targetAdorner;

        /// <summary>
        ///   The zoom speed.
        /// </summary>
        private double zoomSpeed;

        private readonly Stopwatch _spinWatch = new Stopwatch();
        private readonly Stopwatch _watch = new Stopwatch();
        private bool _isFixed;
        private Point3D? _lastPoint3D;
        private Point _lastPosition;
        private Point3D? _mouseDownPoint3D;
        private Point _mouseDownPosition;

        private bool _panning;

        private bool _rotating;
        private bool _zooming;

        /// <summary>
        ///   Show a target adorner when manipulating the camera.
        /// </summary>
        public bool ShowCameraTarget
        {
            get
            {
                return (bool)this.GetValue(ShowCameraTargetProperty);
            }

            set
            {
                this.SetValue(ShowCameraTargetProperty, value);
            }
        }

        /// <summary>
        /// Keep the point (3D) where rotation/zoom started at the same screen position(2D)
        /// </summary>
        public bool FixedMouseDownPoint
        {
            get { return (bool)GetValue(FixedMouseDownPointProperty); }
            set { SetValue(FixedMouseDownPointProperty, value); }
        }

        public PerspectiveCamera Camera
        {
            get { return (PerspectiveCamera)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Camera.  This enables animation, styling, binding, etc...

        public MouseAction LeftDoubleClickAction { get; set; }
        public MouseAction MiddleDoubleClickAction { get; set; }
        public MouseAction RightDoubleClickAction { get; set; }

        public MouseAction LeftButtonAction { get; set; }
        public MouseAction ShiftLeftButtonAction { get; set; }
        public MouseAction ControlLeftButtonAction { get; set; }

        public MouseAction MiddleButtonAction { get; set; }
        public MouseAction ShiftMiddleButtonAction { get; set; }
        public MouseAction ControlMiddleButtonAction { get; set; }

        public MouseAction RightButtonAction { get; set; }
        public MouseAction ShiftRightButtonAction { get; set; }
        public MouseAction ControlRightButtonAction { get; set; }

        public double ZoomSensitivity { get; set; }
        public double RotateSensitivity { get; set; }
        public double PanSensitivity { get; set; }

        public delegate void SelectionChangedType(DependencyObject visual);
        public SelectionChangedType SelectionChanged;

        private void OnSelectionChanged(DependencyObject visual)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(visual);
        }

        /// <summary>
        ///   Gets or sets CameraRotationMode.
        /// </summary>
        public CameraRotationMode CameraRotationMode
        {
            get
            {
                return (CameraRotationMode)this.GetValue(CameraRotationModeProperty);
            }

            set
            {
                this.SetValue(CameraRotationModeProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets CameraMode.
        /// </summary>
        public CameraMode CameraMode
        {
            get
            {
                return (CameraMode)this.GetValue(CameraModeProperty);
            }

            set
            {
                this.SetValue(CameraModeProperty, value);
            }
        }

        #region General properties

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CameraController),
                                        new UIPropertyMetadata(true));

        public static readonly DependencyProperty EventSurfaceProperty =
            DependencyProperty.Register("EventSurface", typeof(FrameworkElement), typeof(CameraController),
                                        new UIPropertyMetadata(null));

        public static readonly DependencyProperty ViewportProperty =
            DependencyProperty.Register("Viewport", typeof(Viewport3D), typeof(CameraController),
                                        new PropertyMetadata(null, ViewportChanged));

        /// <summary>
        /// The element that receives mouse events
        /// </summary>
        public FrameworkElement EventSurface
        {
            get { return (FrameworkElement)GetValue(EventSurfaceProperty); }
            set { SetValue(EventSurfaceProperty, value); }
        }

        /// <summary>
        ///   Gets or sets Viewport.
        /// </summary>
        public Viewport3D Viewport
        {
            get
            {
                return (Viewport3D)this.GetValue(ViewportProperty);
            }

            set
            {
                this.SetValue(ViewportProperty, value);
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether Enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (bool)this.GetValue(EnabledProperty);
            }

            set
            {
                this.SetValue(EnabledProperty, value);
            }
        }

        private static void ViewportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CameraController)d).OnViewportChanged();
        }

        private void OnViewportChanged()
        {
            if (Camera == null && Viewport != null)
                Camera = Viewport.Camera as PerspectiveCamera;
        }

        #endregion

        private void SubscribeEvents()
        {
            EventSurface.MouseMove += MouseMoveHandler;
            EventSurface.MouseDown += MouseDownHandler;
            EventSurface.MouseUp += MouseUpHandler;
            EventSurface.MouseWheel += OnMouseWheel;
            //EventSurface.KeyDown += OnKeyDown;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // todo
        private void UnSubscribeEvents()
        {
            EventSurface.MouseMove -= MouseMoveHandler;
            EventSurface.MouseDown -= MouseDownHandler;
            EventSurface.MouseUp -= MouseUpHandler;
            EventSurface.MouseWheel -= OnMouseWheel;
            //EventSurface.KeyDown -= OnKeyDown;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            // Time in seconds
            double time = 1.0 * (_watch.ElapsedTicks - lastTick) / Stopwatch.Frequency;
            lastTick = _watch.ElapsedTicks;
            OnTimeStep(time);
        }

        #region Spinning / inertia handling

        private void OnTimeStep(double time)
        {
            // should be independent of time
            double factor = Math.Pow(InertiaFactor, time / 0.012);
            // factor = InertiaFactor;

            if (isSpinning && spinningSpeed.LengthSquared > 0)
            {
                Rotate(spinningSpeed.X * time, spinningSpeed.Y * time);

                if (!InfiniteSpin)
                    spinningSpeed *= factor;
                _spinWatch.Reset();
                _spinWatch.Start();
            }

            if (rotationSpeed.LengthSquared > 0.1)
            {
                Rotate(rotationSpeed.X * time, rotationSpeed.Y * time);
                rotationSpeed *= factor;
            }

            if (Math.Abs(panSpeed.LengthSquared) > 0.0001)
            {
                Pan(panSpeed * time);
                panSpeed *= factor;
            }
            if (Math.Abs(zoomSpeed) > 0.1)
            {
                Zoom(zoomSpeed * time);
                zoomSpeed *= factor;
            }
        }

        #endregion

        #region Keyboard handlers

        // todo
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.Key)
            {
                case Key.Left:
                    AddRotateForce(-50, 0);
                    break;
                case Key.Right:
                    AddRotateForce(50, 0);
                    break;
            }
        }

        #endregion

        #region Mouse handlers

        private Vector3D _fixRelative;

        public bool IsActive
        {
            get { return Enabled && Viewport != null && Camera != null; }
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled) return;
            if (Viewport == null)
                throw new NullReferenceException("Viewport");

            _mouseDownPosition = e.GetPosition(this);
            _fixRelative = new Vector3D();

            // reset camera
            if (CheckButton(e, MouseAction.ResetCamera))
                ResetCamera();

            Point3D point;
            Vector3D normal;
            DependencyObject visual;
            bool farest = CheckButton(e, MouseAction.SelectFarest);
            if (Viewport3DHelper.Find(Viewport, _mouseDownPosition, farest, out point, out normal, out visual))
                _mouseDownPoint3D = point;
            else
                _mouseDownPoint3D = null;

            _lastPoint3D = UnProject(_mouseDownPosition, CameraTarget(), Camera.LookDirection);

            // select object
            if ((CheckButton(e, MouseAction.Select) || farest) && visual != null)
                OnSelectionChanged(visual);

            // change the 'lookat' point
            if (_mouseDownPoint3D != null && CheckButton(e, MouseAction.ChangeLookAt))
                LookAt(_mouseDownPoint3D.Value, 0);

            _zooming = CheckButton(e, MouseAction.Zoom);
            _panning = CheckButton(e, MouseAction.Pan);
            _rotating = CheckButton(e, MouseAction.Rotate);
            _isFixed = false;

            if (_zooming || _panning || _rotating)
            {
                bool rightWinKey = (Keyboard.IsKeyDown(Key.RWin));
                if (FixedMouseDownPoint || rightWinKey)
                {
                    if (!_panning && _mouseDownPoint3D != null)
                    {
                        _fixRelative = _mouseDownPoint3D.Value - CameraTarget();
                        ShowTargetAdorner(_mouseDownPosition);
                        _isFixed = true;
                    }
                }
                else
                {
                    // show the adorner in the middle
                    ShowTargetAdorner(new Point(Viewport.ActualWidth / 2, Viewport.ActualHeight / 2));
                }

                if (_zooming || _panning || _rotating)
                {
                    e.Handled = true;
                    ((UIElement)sender).CaptureMouse();
                }

                _spinWatch.Reset();
                _spinWatch.Start();

                // ProjectToTrackball(EventSurface.ActualWidth, EventSurface.ActualHeight, _mouseDownPosition);
                _lastPosition = _mouseDownPosition;
            }

            isSpinning = false;
        }

        private bool CheckButton(MouseButtonEventArgs e, MouseAction a)
        {
            bool control = (Keyboard.IsKeyDown(Key.LeftCtrl));
            bool shift = (Keyboard.IsKeyDown(Key.LeftShift));
            bool doubleClick = e.ClickCount == 2;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (control)
                    return a == ControlLeftButtonAction;
                if (shift)
                    return a == ShiftLeftButtonAction;
                if (doubleClick)
                    return a == LeftDoubleClickAction;
                return a == LeftButtonAction;
            }

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                if (control)
                    return a == ControlMiddleButtonAction;
                if (shift)
                    return a == ShiftMiddleButtonAction;
                if (doubleClick)
                    return a == MiddleDoubleClickAction;
                return a == MiddleButtonAction;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (control)
                    return a == ControlRightButtonAction;
                if (shift)
                    return a == ShiftRightButtonAction;
                if (doubleClick)
                    return a == RightDoubleClickAction;
                return a == RightButtonAction;
            }

            return false;
        }

        public Point3D CameraTarget()
        {
            return Camera.Position + Camera.LookDirection;
        }

        // From 3dtools
        private static Vector3D ProjectToTrackball(double width, double height, Point point)
        {
            double x = point.X / (width / 2); // Scale so bounds map to [0,0] - [2,2]
            double y = point.Y / (height / 2);

            x = x - 1; // Translate 0,0 to the center
            y = 1 - y; // Flip so +Y is up instead of down

            double z2 = 1 - x * x - y * y; // z^2 = 1 - x^2 - y^2
            double z = z2 > 0 ? Math.Sqrt(z2) : 0;

            return new Vector3D(x, z, y);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (!IsActive) return;

            var element = (UIElement)sender;
            if (element.IsMouseCaptured)
            {
                Point point = e.MouseDevice.GetPosition(element);

                // move target point to mouse down point (3D)
                // camera will be positioned back later
                Camera.Position += _fixRelative;

                Point3D? thisPoint3D = UnProject(point, CameraTarget(), Camera.LookDirection);
                Vector3D delta3D = _lastPoint3D.Value - thisPoint3D.Value;

                Vector delta = point - _lastPosition;
                _lastPosition = point;

                // var thisTrack3D = ProjectToTrackball(EventSurface.ActualWidth, EventSurface.ActualHeight, point);


                if (_rotating)
                {
                    Rotate(delta.X, delta.Y);
                }

                if (_zooming)
                    Zoom(delta.Y * 0.01);
                if (_panning)
                    Pan(delta3D);

                _lastPoint3D = UnProject(point, CameraTarget(), Camera.LookDirection);


                Camera.Position -= _fixRelative;

                if (_isFixed)
                {
                    // todo:
                    // reposition the camera so mouse down point (3D) matches the mousedown position (2D)
                    Pan(_mouseDownPoint3D.Value, _mouseDownPosition);
                }

                e.Handled = true;
            }
        }

        private void Pan(Point3D point3D, Point position)
        {
            Point3D? nowPoint3D = UnProject(position, point3D, Camera.LookDirection);
            Pan(point3D - nowPoint3D.Value);
            Point newPosition = Project(point3D);
            Debug.Assert(newPosition == position);
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled) return;

            var element = (UIElement)sender;

            if (_spinWatch.ElapsedMilliseconds < SpinReleaseTime)
            {
                if (_rotating)
                {
                    spinningSpeed = 4 * (_lastPosition - _mouseDownPosition)
                                     * ((double)SpinReleaseTime / _spinWatch.ElapsedMilliseconds);
                    _spinWatch.Reset();
                    _spinWatch.Start();
                    isSpinning = true;
                }
            }
            _rotating = false;
            _zooming = false;
            _panning = false;

            if (element.IsMouseCaptured)
            {
                e.Handled = true;
                HideTargetAdorner();
                element.ReleaseMouseCapture();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            //Zoom(-e.Delta * 0.001);
            AddZoomForce(-e.Delta * 0.001);
        }

        #endregion

        #region Camera operations

        public void ResetCamera()
        {
            CameraHelper.Reset(Camera);
        }

        public void Zoom(double delta)
        {
            if (!IsZoomEnabled)
                return;

            bool alt = (Keyboard.IsKeyDown(Key.LeftAlt));
            CameraMode cm = CameraMode;
            if (alt)
                cm = (CameraMode)(((int)CameraMode + 1) % 2);

            switch (cm)
            {
                case CameraMode.Inspect:
                    Point3D target = Camera.Position + Camera.LookDirection;
                    Vector3D lookDirection = Camera.LookDirection * (1 + delta);
                    LookAt(target, lookDirection, 0);
                    //Point3D target = Camera.Position + Camera.LookDirection;
                    //Camera.LookDirection *= (1 + delta);
                    //Camera.Position = target - Camera.LookDirection;
                    break;
                case CameraMode.WalkAround:
                    Camera.Position -= Camera.LookDirection * delta;
                    break;
                case CameraMode.FixedPosition:
                    double fov = Camera.FieldOfView;
                    fov *= (1 + delta);
                    if (fov < 3) fov = 3;
                    if (fov > 160) fov = 160;
                    Camera.FieldOfView = fov;
                    break;
            }
        }

        public void Pan(Vector3D delta)
        {
            if (!IsPanEnabled)
                return;
            if (CameraMode == CameraMode.FixedPosition)
                return;
            Camera.Position += delta;
        }


        public void Rotate(double dx, double dy)
        {
            PerspectiveCamera c = Camera;

            Point3D target = c.Position + c.LookDirection;

            // toggle rotation mode if the user presses alt
            bool alt = (Keyboard.IsKeyDown(Key.LeftAlt));

            if ((CameraRotationMode == CameraRotationMode.VirtualTrackball) != alt)
            {
                RotateRoam(dx, dy);
                //    Track(thisTrack3D, lastTrack3D);
                //    lastTrack3D = thisTrack3D;
            }
            else
            {
                RotateTwoAxes(dx, dy);
            }

            if (Math.Abs(c.UpDirection.Length - 1) > 1e-8)
                c.UpDirection.Normalize();

            if (IsFixedPosition())
                c.Position = target - c.LookDirection;
        }

        public bool IsFixedPosition()
        {
            bool leftWinKey = Keyboard.IsKeyDown(Key.LWin);

            // fix the camera position if user presses left Windows key
            if (leftWinKey)
                return CameraMode != CameraMode.Inspect;

            return CameraMode == CameraMode.Inspect;
        }

        // http://www.codeplex.com/3DTools/Thread/View.aspx?ThreadId=22310
        private void RotateRoam(double dX, double dY)
        {
            PerspectiveCamera c = Camera;
            double dist = c.LookDirection.Length;

            Vector3D camZ = c.LookDirection;
            camZ.Normalize();
            Vector3D camX = -Vector3D.CrossProduct(camZ, c.UpDirection);
            camX.Normalize();
            Vector3D camY = Vector3D.CrossProduct(camZ, camX);
            camY.Normalize();

            var aarY = new AxisAngleRotation3D(camY, -dX * 0.5);
            var aarX = new AxisAngleRotation3D(camX, dY * 0.5);

            var rotY = new RotateTransform3D(aarY);
            var rotX = new RotateTransform3D(aarX);

            camZ = camZ * rotY.Value * rotX.Value;
            camZ.Normalize();
            camY = camY * rotX.Value * rotY.Value;
            camY.Normalize();

            Vector3D newLookDir = camZ * dist;
            Vector3D newUpDir = camY;

            Vector3D right = Vector3D.CrossProduct(newLookDir, newUpDir);
            right.Normalize();
            Vector3D modUpDir = Vector3D.CrossProduct(right, newLookDir);
            modUpDir.Normalize();
            if ((newUpDir - modUpDir).Length > 1e-8)
                newUpDir = modUpDir;

            c.LookDirection = newLookDir;
            c.UpDirection = newUpDir;
        }

        public void RotateTwoAxes(double dx, double dy)
        {
            PerspectiveCamera c = Camera;

            var up = new Vector3D(0, 0, 1);
            Vector3D dir = c.LookDirection;
            dir.Normalize();

            Vector3D right = Vector3D.CrossProduct(dir, c.UpDirection);
            right.Normalize();

            double d = -0.5;
            if (CameraMode == CameraMode.WalkAround)
                d = 0.1;

            var q1 = new Quaternion(up, d * dx);
            var q2 = new Quaternion(right, d * dy);
            Quaternion q = q1 * q2;

            var m = new Matrix3D();
            m.Rotate(q);

            Vector3D newLookDir = m.Transform(c.LookDirection);
            Vector3D newUpDir = m.Transform(c.UpDirection);

            right = Vector3D.CrossProduct(newLookDir, newUpDir);
            right.Normalize();
            Vector3D modUpDir = Vector3D.CrossProduct(right, newLookDir);
            modUpDir.Normalize();
            if ((newUpDir - modUpDir).Length > 1e-8)
                newUpDir = modUpDir;

            c.LookDirection = newLookDir;
            c.UpDirection = newUpDir;
        }

        public void AddPanForce(double dx, double dy)
        {
            AddPanForce(FindPanVector(dx, dy));
        }

        private Vector3D FindPanVector(double dx, double dy)
        {
            PerspectiveCamera pc = Camera;
            Vector3D axis1 = Vector3D.CrossProduct(pc.LookDirection, pc.UpDirection);
            Vector3D axis2 = Vector3D.CrossProduct(axis1, pc.LookDirection);
            axis1.Normalize();
            axis2.Normalize();
            double l = pc.LookDirection.Length;
            double f = l * 0.001;
            Vector3D move = -axis1 * f * dx + axis2 * f * dy; // this should be dependent on distance to target?           
            return move;
        }

        public void AddPanForce(Vector3D pan)
        {
            panSpeed += pan * 40;
        }

        public void AddRotateForce(double dx, double dy)
        {
            rotationSpeed.X += dx * 40;
            rotationSpeed.Y += dy * 40;
        }

        public void AddZoomForce(double dx)
        {
            zoomSpeed += dx * 8;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the ray into the view volume given by the position in 2D (screen coordinates)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Ray3D GetRay(Point position)
        {
            Point3D point1, point2;
            bool ok = Viewport3DHelper.Point2DtoPoint3D(Viewport, position, out point1, out point2);
            if (!ok)
                return null;

            return new Ray3D { Origin = point1, Direction = point2 - point1 };
        }

        /// <summary>
        /// Unproject a point from the screen (2D) to a point on plane (3D)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="position">plane position</param>
        /// <param name="normal">plane normal</param>
        /// <returns></returns>
        public Point3D? UnProject(Point p, Point3D position, Vector3D normal)
        {
            Ray3D ray = GetRay(p);
            if (ray == null)
                return null;
            return ray.PlaneIntersection(position, normal);
        }

        /// <summary>
        /// Calculate the screen position of a 3D point
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Point Project(Point3D p)
        {
            return Viewport3DHelper.Point3DtoPoint2D(Viewport, p);
        }

        public void Select(Point target, double animationTime)
        {
            PerspectiveCamera camera = Camera;
            if (camera == null) return;

            //Point ptMouse = args.GetPosition(viewport);
            HitTestResult result = VisualTreeHelper.HitTest(Viewport, target);

            //// We're only interested in 3D hits.
            //RayMeshGeometry3DHitTestResult result3d =
            //                    result as RayMeshGeometry3DHitTestResult;
        }

        /// <summary>
        /// Set the camera target point
        /// </summary>
        /// <param name="target"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public void LookAt(Point3D target, double animationTime)
        {
            PerspectiveCamera camera = Camera;
            if (camera == null) return;

            LookAt(target, camera.LookDirection, animationTime);
        }

        public void LookAt(Point3D target, double distance, double animationTime)
        {
            var d = Camera.LookDirection;
            d.Normalize();
            LookAt(target, d * distance, animationTime);
        }

        public void LookAt(Point3D target, Vector3D newDirection, double animationTime)
        {
            PerspectiveCamera camera = Camera;
            if (camera == null) return;

            Point3D newPosition = target - newDirection;

            CameraHelper.AnimateTo(camera, newPosition, newDirection, camera.UpDirection, animationTime);
            /*
            Point3D newPosition = point3D - newDirection;
            if (animationTime == 0)
            {
                camera.LookDirection = newDirection;
                camera.Position = newPosition;
            }
            else
            {
                var a = new Point3DAnimation(newPosition,
                                             new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5 };
                camera.BeginAnimation(ProjectionCamera.PositionProperty, a);

                var a2 = new Vector3DAnimation(newDirection,
                                               new Duration(TimeSpan.FromMilliseconds(animationTime))) { AccelerationRatio = 0.3, DecelerationRatio = 0.5 };
                camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, a2);
            }
            return newPosition;*/
        }

        #endregion

        /// <summary>
        ///   Shows the target adorner.
        /// </summary>
        /// <param name="position"> The position. </param>
        public void ShowTargetAdorner(Point position)
        {
            if (!this.ShowCameraTarget)
            {
                return;
            }

            if (this.targetAdorner != null)
            {
                return;
            }

            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            this.targetAdorner = new TargetSymbolAdorner(this.Viewport, position);
            myAdornerLayer.Add(this.targetAdorner);
        }

        /// <summary>
        ///   Hides the target adorner.
        /// </summary>
        public void HideTargetAdorner()
        {
            AdornerLayer myAdornerLayer = AdornerLayer.GetAdornerLayer(this.Viewport);
            if (this.targetAdorner != null)
            {
                myAdornerLayer.Remove(this.targetAdorner);
            }

            this.targetAdorner = null;

            // the adorner sometimes leaves some 'dust', so refresh the viewport
            this.RefreshViewport();
        }

        /// <summary>
        ///   The refresh viewport.
        /// </summary>
        private void RefreshViewport()
        {
            // todo: this is a hack, should be improved

            // var mg = new ModelVisual3D { Content = new AmbientLight(Colors.White) };
            // Viewport.Children.Add(mg);
            // Viewport.Children.Remove(mg);
            Camera c = this.Viewport.Camera;
            this.Viewport.Camera = null;
            this.Viewport.Camera = c;

            // var w = Viewport.Width;
            // Viewport.Width = w-1;
            // Viewport.Width = w;

            // Viewport.InvalidateVisual();
        }
    }
}