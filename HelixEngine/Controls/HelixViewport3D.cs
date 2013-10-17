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
    /// A control that contains a <see cref="Viewport3D"/> and a <see cref="CameraController"/> .
    /// </summary>
    /// <example>
    /// The following XAML code shows how to create a 3D view 
    /// <code>
    /// <Window x:Class="..." xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:h="clr-namespace:HelixToolkit;assembly=HelixToolkit.Wpf">
    ///                                                             <h:HelixViewport3D></h:HelixViewport3D>
    ///                                                           </Window>
    ///                                                         </code>
    /// </example>
    [ContentProperty("Children")]
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class HelixViewport3D : Control
    {
        /// <summary>
        ///   The camera changed event.
        /// </summary>
        public static readonly RoutedEvent CameraChangedEvent = EventManager.RegisterRoutedEvent(
            "CameraChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HelixViewport3D));

        /// <summary>
        /// Identifies the <see cref="CameraMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraModeProperty = DependencyProperty.Register(
            "CameraMode", typeof(CameraMode), typeof(HelixViewport3D), new UIPropertyMetadata(CameraMode.Inspect));

        /// <summary>
        /// Identifies the <see cref="CameraRotationMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraRotationModeProperty =
            DependencyProperty.Register(
                "CameraRotationMode", 
                typeof(CameraRotationMode), 
                typeof(HelixViewport3D), 
                new UIPropertyMetadata(CameraRotationMode.Turntable));

        /// <summary>
        /// <summary>
        ///   The show view cube property.
        /// </summary>
        public static readonly DependencyProperty ShowViewCubeProperty = DependencyProperty.Register(
            "ShowViewCube", typeof(bool), typeof(HelixViewport3D), new UIPropertyMetadata(true));

        ///   The title property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(HelixViewport3D), new UIPropertyMetadata(null));

        /// <summary>
        ///   The adorner layer name.
        /// </summary>
        private const string PartAdornerLayer = "PART_AdornerLayer";

        /// <summary>
        ///   The camera controller name.
        /// </summary>
        private const string PartCameraController = "PART_CameraController";

        /// <summary>
        ///   The view cube name.
        /// </summary>
        private const string PartViewCube = "PART_ViewCube";

        /// <summary>
        ///   The view cube viewport name.
        /// </summary>
        private const string PartViewCubeViewport = "PART_ViewCubeViewport";

        /// <summary>
        ///   The viewport.
        /// </summary>
        private readonly Viewport3D viewport;

        /// <summary>
        ///   The adorner layer.
        /// </summary>
        private AdornerDecorator adornerLayer;

        /// <summary>
        ///   The camera controller.
        /// </summary>
        private CameraController cameraController;

        /// <summary>
        ///   The view cube.
        /// </summary>
        private ViewCubeVisual3D viewCube;

        /// <summary>
        ///   The view cube lights.
        /// </summary>
        private Model3DGroup viewCubeLights;

        /// <summary>
        ///   The view cube view.
        /// </summary>
        private Viewport3D viewCubeViewport;

        /// <summary>
        ///   Initializes static members of the <see cref="HelixViewport3D" /> class.
        /// </summary>
        static HelixViewport3D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HelixViewport3D), new FrameworkPropertyMetadata(typeof(HelixViewport3D)));
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HelixViewport3D" /> class.
        /// </summary>
        public HelixViewport3D()
        {
            // The Viewport3D must be created here since the Children collection is attached directly
            this.viewport = new Viewport3D
                                {
                                    IsHitTestVisible = false,
                                    Camera = CameraHelper.CreateDefaultCamera()
                                };
            this.Viewport.Camera.Changed += this.CameraPropertyChanged;
        }

        /// <summary>
        /// Event when a property has been changed
        /// </summary>
        public event RoutedEventHandler CameraChanged
        {
            add
            {
                this.AddHandler(CameraChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(CameraChangedEvent, value);
            }
        }

        /// <summary>
        ///   Gets or sets the camera.
        /// </summary>
        /// <value> The camera. </value>
        public PerspectiveCamera Camera
        {
            get
            {
                return this.Viewport.Camera as PerspectiveCamera;
            }

            set
            {
                if (this.Viewport == null)
                    return;

                if (this.Viewport.Camera != null)
                {
                    this.Viewport.Camera.Changed -= this.CameraPropertyChanged;
                }

                this.Viewport.Camera = value;
                this.Viewport.Camera.Changed += this.CameraPropertyChanged;
            }
        }

        /// <summary>
        ///   Gets the camera controller
        /// </summary>
        public CameraController CameraController
        {
            get
            {
                return this.cameraController;
            }
        }

        /// <summary>
        ///   Gets or sets the <see cref="CameraMode" />
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

        /// <summary>
        ///   Gets or sets the camera rotation mode.
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
        ///   Gets the children.
        /// </summary>
        /// <value> The children. </value>
        public Visual3DCollection Children
        {
            get
            {
                return this.viewport.Children;
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether [show view cube].
        /// </summary>
        /// <value> <c>true</c> if [show view cube]; otherwise, <c>false</c> . </value>
        public bool ShowViewCube
        {
            get
            {
                return (bool)this.GetValue(ShowViewCubeProperty);
            }

            set
            {
                this.SetValue(ShowViewCubeProperty, value);
            }
        }

        ///   Gets or sets the title.
        /// </summary>
        /// <value> The title. </value>
        public string Title
        {
            get
            {
                return (string)this.GetValue(TitleProperty);
            }

            set
            {
                this.SetValue(TitleProperty, value);
            }
        }

        /// <summary>
        ///   Gets the viewport.
        /// </summary>
        /// <value> The viewport. </value>
        public Viewport3D Viewport
        {
            get
            {
                return this.viewport;
            }
        }

        /// <summary>
        /// Copies the view to the clipboard.
        /// </summary>
        public void Copy()
        {
            Viewport3DHelper.Copy(
                this.Viewport, this.Viewport.ActualWidth * 2, this.Viewport.ActualHeight * 2, Brushes.White);
        }

        /// <summary>
        /// Finds the nearest object.
        /// </summary>
        /// <param name="pt">
        /// The pt. 
        /// </param>
        /// <param name="pos">
        /// The pos. 
        /// </param>
        /// <param name="normal">
        /// The normal. 
        /// </param>
        /// <param name="obj">
        /// The obj. 
        /// </param>
        /// <returns>
        /// The find nearest. 
        /// </returns>
        public bool Find(Point pt, out Point3D pos, bool farest, out Vector3D normal, out DependencyObject obj)
        {
            return Viewport3DHelper.Find(this.Viewport, pt, farest, out pos, out normal, out obj);
        }

        /// <summary>
        /// Finds the nearest point.
        /// </summary>
        /// <param name="pt">
        /// The pt. 
        /// </param>
        /// <returns>
        /// A point. 
        /// </returns>
        public Point3D? FindPoint(Point pt, bool farest)
        {
            return Viewport3DHelper.FindPoint(this.Viewport, pt, farest);
        }

        /// <summary>
        /// Finds the nearest visual.
        /// </summary>
        /// <param name="pt">
        /// The pt. 
        /// </param>
        /// <returns>
        /// A visual. 
        /// </returns>
        public Visual3D FindNearestVisual(Point pt, bool farest)
        {
            return Viewport3DHelper.FindVisual(this.Viewport, pt, farest);
        }

        /// <summary>
        /// Change the camera to look at the specified point.
        /// </summary>
        /// <param name="p">
        /// The point. 
        /// </param>
        public void LookAt(Point3D p)
        {
            this.LookAt(p, 0);
        }

        /// <summary>
        /// Change the camera to look at the specified point.
        /// </summary>
        /// <param name="p">
        /// The point. 
        /// </param>
        /// <param name="animationTime">
        /// The animation time. 
        /// </param>
        public void LookAt(Point3D p, double animationTime)
        {
            CameraHelper.LookAt(this.Camera, p, animationTime);
        }

        /// <summary>
        /// Change the camera to look at the specified point.
        /// </summary>
        /// <param name="p">
        /// The point. 
        /// </param>
        /// <param name="distance">
        /// The distance. 
        /// </param>
        /// <param name="animationTime">
        /// The animation time. 
        /// </param>
        public void LookAt(Point3D p, double distance, double animationTime)
        {
            CameraHelper.LookAt(this.Camera, p, distance, animationTime);
        }

        /// <summary>
        /// Change the camera to look at the specified point.
        /// </summary>
        /// <param name="p">
        /// The point. 
        /// </param>
        /// <param name="direction">
        /// The direction. 
        /// </param>
        /// <param name="animationTime">
        /// The animation time. 
        /// </param>
        public void LookAt(Point3D p, Vector3D direction, double animationTime)
        {
            CameraHelper.LookAt(this.Camera, p, direction, animationTime);
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate"/> .
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.adornerLayer == null)
            {
                this.adornerLayer = this.Template.FindName(PartAdornerLayer, this) as AdornerDecorator;
                if (this.adornerLayer != null)
                {
                    this.adornerLayer.Child = this.viewport;
                }
            }

            Debug.Assert(
                this.adornerLayer != null, string.Format("{0} is missing from the template.", PartAdornerLayer));

            if (this.cameraController == null)
            {
                this.cameraController = this.Template.FindName(PartCameraController, this) as CameraController;
                if (this.cameraController != null)
                {
                    this.cameraController.Viewport = this.Viewport;
                    this.cameraController.SelectionChanged += CameraControllerSelectionChanged;
                }
            }

            Debug.Assert(
                this.cameraController != null, string.Format("{0} is missing from the template.", PartCameraController));

            if (this.viewCubeViewport == null)
            {
                this.viewCubeViewport = this.Template.FindName(PartViewCubeViewport, this) as Viewport3D;

                this.viewCubeLights = new Model3DGroup();
                this.viewCubeLights.Children.Add(new AmbientLight(Colors.White));
                if (this.viewCubeViewport != null)
                {
                    this.viewCubeViewport.Camera = new PerspectiveCamera();
                    this.viewCubeViewport.Children.Add(new ModelVisual3D { Content = this.viewCubeLights });
                    this.viewCubeViewport.MouseEnter += this.ViewCubeViewportMouseEnter;
                    this.viewCubeViewport.MouseLeave += this.ViewCubeViewportMouseLeave;
                }

                this.viewCube = this.Template.FindName(PartViewCube, this) as ViewCubeVisual3D;
                if (this.viewCube != null)
                {
                    this.viewCube.Viewport = this.Viewport;
                }
            }

            Debug.Assert(
                this.viewCube != null, string.Format("{0} is missing from the template.", PartViewCubeViewport));

            // update the coordinateview camera
            this.OnCameraChanged();

            // add the default headlight
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Change the camera position and directions.
        /// </summary>
        /// <param name="newPosition">
        /// The new camera position. 
        /// </param>
        /// <param name="newDirection">
        /// The new camera look direction. 
        /// </param>
        /// <param name="newUpDirection">
        /// The new camera up direction. 
        /// </param>
        /// <param name="animationTime">
        /// The animation time. 
        /// </param>
        public void SetView(Point3D newPosition, Vector3D newDirection, Vector3D newUpDirection, double animationTime)
        {
            CameraHelper.AnimateTo(this.Camera, newPosition, newDirection, newUpDirection, animationTime);
        }

        /// <summary>
        /// Zooms to the extents of the sceen.
        /// </summary>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public void ZoomExtents(double animationTime)
        {
            CameraHelper.ZoomExtents(this.Camera, this.Viewport, animationTime);
        }

        /// <summary>
        /// Zooms to the extents of the specified bounding box.
        /// </summary>
        /// <param name="bounds">
        /// The bounding box.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public void ZoomExtents(Rect3D bounds, double animationTime)
        {
            CameraHelper.ZoomExtents(this.Camera, this.Viewport, bounds, animationTime);
        }

        /// <summary>
        /// Zooms to fit the specified sphere.
        /// </summary>
        /// <param name="center">
        /// The center of the sphere.
        /// </param>
        /// <param name="radius">
        /// The radius of the sphere.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public void ZoomExtents(Point3D center, double radius, double animationTime)
        {
            CameraHelper.ZoomExtents(this.Camera, this.Viewport, center, radius, animationTime);
        }

        public delegate void SelectionChangedType(DependencyObject visual, bool toggle);
        public SelectionChangedType SelectionChanged;

        private void OnSelectionChanged(DependencyObject visual, bool toggle)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(visual, toggle);
        }

        private void CameraControllerSelectionChanged(DependencyObject visual, bool toggle)
        {
            OnSelectionChanged(visual, toggle);
        }

        /// <summary>
        /// Called when the camera is changed.
        /// </summary>
        protected virtual void OnCameraChanged()
        {
            // update the camera of the view cube
            if (this.viewCubeViewport != null)
            {
                CameraHelper.CopyDirectionOnly(this.Camera, this.viewCubeViewport.Camera as PerspectiveCamera, 20);
            }
        }

        /// <summary>
        /// Raises the camera changed event.
        /// </summary>
        protected virtual void RaiseCameraChangedEvent()
        {
            // e.Handled = true;
            var args = new RoutedEventArgs(CameraChangedEvent);
            this.RaiseEvent(args);
        }

        /// <summary>
        /// The animate opacity.
        /// </summary>
        /// <param name="obj">
        /// The obj. 
        /// </param>
        /// <param name="toOpacity">
        /// The to opacity. 
        /// </param>
        /// <param name="animationTime">
        /// The animation time. 
        /// </param>
        private static void AnimateOpacity(UIElement obj, double toOpacity, double animationTime)
        {
            var a = new DoubleAnimation(toOpacity, new Duration(TimeSpan.FromMilliseconds(animationTime)))
                {
                   AccelerationRatio = 0.3, DecelerationRatio = 0.5 
                };
            obj.BeginAnimation(OpacityProperty, a);
        }

        /// <summary>
        /// The camera_ changed.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        private void CameraPropertyChanged(object sender, EventArgs e)
        {
            // Raise notification
            this.RaiseCameraChangedEvent();

            // Update the CoordinateView camera and the headlight direction
            this.OnCameraChanged();
        }

        /// <summary>
        /// Called when the mouse enters the view cube.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        private void ViewCubeViewportMouseEnter(object sender, MouseEventArgs e)
        {
            AnimateOpacity(this.viewCubeViewport, 1.0, 200);
        }

        /// <summary>
        /// Called when the mouse leaves the view cube.
        /// </summary>
        /// <param name="sender">
        /// The sender. 
        /// </param>
        /// <param name="e">
        /// The event arguments. 
        /// </param>
        private void ViewCubeViewportMouseLeave(object sender, MouseEventArgs e)
        {
            AnimateOpacity(this.viewCubeViewport, 0.5, 200);
        }
    }
}