using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml;
using Path = System.IO.Path;

namespace HelixEngine
{
    /// <summary>
    /// Helper methods for Viewport3D.
    /// </summary>
    /// <remarks>
    /// See Charles Petzold's book "3D programming for Windows" and Eric Sink's "Twelve Days of WPF 3D"
    ///   http://www.ericsink.com/wpf3d/index.html
    /// </remarks>
    public static class Viewport3DHelper
    {
        /// <summary>
        /// Copies the specified viewport to the clipboard.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="m">The oversampling multiplier.</param>
        public static void Copy(Viewport3D view)
        {
            Clipboard.SetImage(RenderBitmap(view, Brushes.White));
        }

        /// <summary>
        /// Copies the specified viewport to the clipboard.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        public static void Copy(Viewport3D view, double width, double height, Brush background)
        {
            Clipboard.SetImage(RenderBitmap(view, width, height, background));
        }

        /// <summary>
        /// Copies the viewport as xaml to the clipboard.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        public static void CopyXaml(Viewport3D view)
        {
            Clipboard.SetText(XamlWriter.Save(view));
        }

        /// <summary>
        /// Finds the nearest point and its normal.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <param name="position">
        /// The position.
        /// </param>
        /// <param name="point">
        /// The point.
        /// </param>
        /// <param name="normal">
        /// The normal.
        /// </param>
        /// <param name="visual">
        /// The visual.
        /// </param>
        /// <returns>
        /// The find nearest.
        /// </returns>
        public static bool FindNearest(
            Viewport3D viewport, Point position, out Point3D point, out Vector3D normal, out DependencyObject visual)
        {
            var camera = viewport.Camera as PerspectiveCamera;
            if (camera == null)
            {
                point = new Point3D();
                normal = new Vector3D();
                visual = null;
                return false;
            }

            var hitParams = new PointHitTestParameters(position);

            double minimumDistance = double.MaxValue;
            var nearestPoint = new Point3D();
            var nearestNormal = new Vector3D();
            DependencyObject nearestObject = null;

            VisualTreeHelper.HitTest(
                viewport,
                null,
                delegate(HitTestResult hit)
                {
                    var rayHit = hit as RayMeshGeometry3DHitTestResult;
                    if (rayHit != null)
                    {
                        var mesh = rayHit.MeshHit;
                        if (mesh != null)
                        {
                            var p1 = mesh.Positions[rayHit.VertexIndex1];
                            var p2 = mesh.Positions[rayHit.VertexIndex2];
                            var p3 = mesh.Positions[rayHit.VertexIndex3];
                            double x = p1.X * rayHit.VertexWeight1 + p2.X * rayHit.VertexWeight2
                                       + p3.X * rayHit.VertexWeight3;
                            double y = p1.Y * rayHit.VertexWeight1 + p2.Y * rayHit.VertexWeight2
                                       + p3.Y * rayHit.VertexWeight3;
                            double z = p1.Z * rayHit.VertexWeight1 + p2.Z * rayHit.VertexWeight2
                                       + p3.Z * rayHit.VertexWeight3;

                            // point in local coordinates
                            var p = new Point3D(x, y, z);

                            // transform to global coordinates

                            // first transform the Model3D hierarchy
                            var t2 = GetTransform(rayHit.VisualHit, rayHit.ModelHit);
                            if (t2 != null)
                            {
                                p = t2.Transform(p);
                            }

                            // then transform the Visual3D hierarchy up to the Viewport3D ancestor
                            var t = GetTransform(viewport, rayHit.VisualHit);
                            if (t != null)
                            {
                                p = t.Transform(p);
                            }

                            double distance = (camera.Position - p).LengthSquared;
                            if (distance < minimumDistance)
                            {
                                minimumDistance = distance;
                                nearestPoint = p;
                                nearestNormal = Vector3D.CrossProduct(p2 - p1, p3 - p1);
                                nearestObject = hit.VisualHit;
                            }
                        }
                    }

                    return HitTestResultBehavior.Continue;
                },
                hitParams);

            point = nearestPoint;
            visual = nearestObject;
            normal = nearestNormal;

            if (minimumDistance >= double.MaxValue)
            {
                return false;
            }

            normal.Normalize();
            return true;
        }

        /// <summary>
        /// Find the coordinates of the nearest point given a 2D position in the viewport
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="position">The position.</param>
        /// <returns>The nearest point, or null if no point was found.</returns>
        public static Point3D? FindNearestPoint(Viewport3D viewport, Point position)
        {
            Point3D p;
            Vector3D n;
            DependencyObject obj;
            if (FindNearest(viewport, position, out p, out n, out obj))
            {
                return p;
            }

            return null;
        }

        /// <summary>
        /// Find the Visual3D that is nearest given a 2D position in the viewport
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="position">The position.</param>
        /// <returns>
        /// The nearest visual, or null if no visual was found.
        /// </returns>
        public static Visual3D FindNearestVisual(Viewport3D viewport, Point position)
        {
            Point3D p;
            Vector3D n;
            DependencyObject obj;
            if (FindNearest(viewport, position, out p, out n, out obj))
            {
                return obj as Visual3D;
            }

            return null;
        }

        /// <summary>
        /// Gets the camera transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport visual.</param>
        /// <returns>The camera transform.</returns>
        public static Matrix3D GetCameraTransform(Viewport3DVisual viewport3DVisual)
        {
            return GetTotalTransform(viewport3DVisual.Camera, viewport3DVisual.Viewport.Size.Width / viewport3DVisual.Viewport.Size.Height);
        }

        /// <summary>
        /// Gets the camera transform.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>
        /// The camera transform.
        /// </returns>
        public static Matrix3D GetCameraTransform(Viewport3D viewport)
        {
            return GetTotalTransform(viewport.Camera, viewport.ActualWidth / viewport.ActualHeight);
        }

        /// <summary>
        /// Gets the inverse camera transform.
        /// </summary>
        /// <param name="camera">
        /// The camera.
        /// </param>
        /// <param name="aspectRatio">
        /// The aspect ratio.
        /// </param>
        /// <returns>
        /// The inverse transform.
        /// </returns>
        public static Matrix3D GetInverseTransform(Camera camera, double aspectRatio)
        {
            var m = GetTotalTransform(camera, aspectRatio);

            if (!m.HasInverse)
            {
                throw new ApplicationException("camera transform has no inverse");
            }

            m.Invert();
            return m;
        }

        /// <summary>
        /// Get all lights in the Viewport3D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>The lights.</returns>
        public static IEnumerable<Light> GetLights(Viewport3D viewport)
        {
            var models = SearchFor<Light>(viewport.Children);
            var lights = new List<Light>();
            foreach (Model3D m in models)
                lights.Add(m as Light);
            return lights.ToArray();
        }

        /// <summary>
        /// Gets the projection matrix for the specified camera.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <returns>The projection matrix.</returns>
        public static Matrix3D GetProjectionMatrix(Camera camera, double aspectRatio)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            var perspectiveCamera = camera as PerspectiveCamera;
            if (perspectiveCamera != null)
            {
                // The angle-to-radian formula is a little off because only
                // half the angle enters the calculation.
                double xscale = 1 / Math.Tan(Math.PI * perspectiveCamera.FieldOfView / 360);
                double yscale = xscale * aspectRatio;
                double znear = perspectiveCamera.NearPlaneDistance;
                double zfar = perspectiveCamera.FarPlaneDistance;
                double zscale = double.IsPositiveInfinity(zfar) ? -1 : (zfar / (znear - zfar));
                double zoffset = znear * zscale;

                return new Matrix3D(xscale, 0, 0, 0, 0, yscale, 0, 0, 0, 0, zscale, -1, 0, 0, zoffset, 0);
            }

            var orthographicCamera = camera as OrthographicCamera;
            if (orthographicCamera != null)
            {
                double xscale = 2.0 / orthographicCamera.Width;
                double yscale = xscale * aspectRatio;
                double znear = orthographicCamera.NearPlaneDistance;
                double zfar = orthographicCamera.FarPlaneDistance;

                if (double.IsPositiveInfinity(zfar))
                {
                    zfar = znear * 1e5;
                }

                double dzinv = 1.0 / (znear - zfar);

                var m = new Matrix3D(xscale, 0, 0, 0, 0, yscale, 0, 0, 0, 0, dzinv, 0, 0, 0, znear * dzinv, 1);
                return m;
            }

            var matrixCamera = camera as MatrixCamera;
            if (matrixCamera != null)
            {
                return matrixCamera.ProjectionMatrix;
            }

            throw new ApplicationException("unknown camera type");
        }

        /// <summary>
        /// Get the combined view and projection transform
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="aspectRatio">The aspect ratio.</param>
        /// <returns>The total view and projection transform.</returns>
        public static Matrix3D GetTotalTransform(Camera camera, double aspectRatio)
        {
            var m = Matrix3D.Identity;

            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (camera.Transform != null)
            {
                var cameraTransform = camera.Transform.Value;

                if (!cameraTransform.HasInverse)
                {
                    throw new ApplicationException("camera transform has no inverse");
                }

                cameraTransform.Invert();
                m.Append(cameraTransform);
            }

            m.Append(GetViewMatrix(camera));
            m.Append(GetProjectionMatrix(camera, aspectRatio));
            return m;
        }

        /// <summary>
        /// Gets the total transform.
        /// </summary>
        /// <param name="viewport3DVisual">The viewport3DVisual.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(Viewport3DVisual viewport3DVisual)
        {
            var m = GetCameraTransform(viewport3DVisual);
            m.Append(GetViewportTransform(viewport3DVisual));
            return m;
        }

        /// <summary>
        /// Gets the total transform for a Viewport3D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <returns>The total transform.</returns>
        public static Matrix3D GetTotalTransform(Viewport3D viewport)
        {
            var matx = GetCameraTransform(viewport);
            matx.Append(GetViewportTransform(viewport));
            return matx;
        }

        /// <summary>
        /// Get the total transform of a Visual3D
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="visual">The visual.</param>
        /// <returns>The transform.</returns>
        public static GeneralTransform3D GetTransform(Viewport3D viewport, Visual3D visual)
        {
            if (visual == null)
            {
                return null;
            }

            foreach (var ancestor in viewport.Children)
            {
                if (visual.IsDescendantOf(ancestor))
                {
                    var g = new GeneralTransform3DGroup();

                    // this includes the visual.Transform
                    var ta = visual.TransformToAncestor(ancestor);
                    if (ta != null)
                    {
                        g.Children.Add(ta);
                    }

                    // add the transform of the top-level ancestor
                    g.Children.Add(ancestor.Transform);

                    return g;
                }
            }

            return visual.Transform;
        }

        /// <summary>
        /// Gets the transform from the specified Visual3D to the Model3D.
        /// </summary>
        /// <param name="visual">The source visual.</param>
        /// <param name="model">The target model.</param>
        /// <returns>The transform.</returns>
        public static GeneralTransform3D GetTransform(Visual3D visual, Model3D model)
        {
            var mv = visual as ModelVisual3D;
            if (mv != null)
            {
                return GetTransform(mv.Content, model, Transform3D.Identity);
            }

            return null;
        }

        /// <summary>
        /// Obtains the view transform matrix for a camera. (see page 327)
        /// </summary>
        /// <param name="camera">
        /// Camera to obtain the ViewMatrix for
        /// </param>
        /// <returns>
        /// A Matrix3D object with the camera view transform matrix, or a Matrix3D with all zeros if the "camera" is null.
        /// </returns>
        public static Matrix3D GetViewMatrix(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (camera is MatrixCamera)
            {
                return (camera as MatrixCamera).ViewMatrix;
            }

            if (camera is ProjectionCamera)
            {
                // Reflector on: ProjectionCamera.CreateViewMatrix
                var projcam = camera as ProjectionCamera;

                var zaxis = -projcam.LookDirection;
                zaxis.Normalize();

                var xaxis = Vector3D.CrossProduct(projcam.UpDirection, zaxis);
                xaxis.Normalize();

                var yaxis = Vector3D.CrossProduct(zaxis, xaxis);
                var pos = (Vector3D)projcam.Position;

                return new Matrix3D(
                    xaxis.X,
                    yaxis.X,
                    zaxis.X,
                    0,
                    xaxis.Y,
                    yaxis.Y,
                    zaxis.Y,
                    0,
                    xaxis.Z,
                    yaxis.Z,
                    zaxis.Z,
                    0,
                    -Vector3D.DotProduct(xaxis, pos),
                    -Vector3D.DotProduct(yaxis, pos),
                    -Vector3D.DotProduct(zaxis, pos),
                    1);
            }

            throw new ApplicationException("unknown camera type");
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="vis">The viewport3DVisual.</param>
        /// <returns>The transform.</returns>
        public static Matrix3D GetViewportTransform(Viewport3DVisual vis)
        {
            return new Matrix3D(
                vis.Viewport.Width / 2,
                0,
                0,
                0,
                0,
                -vis.Viewport.Height / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                vis.Viewport.X + vis.Viewport.Width / 2,
                vis.Viewport.Y + vis.Viewport.Height / 2,
                0,
                1);
        }

        /// <summary>
        /// Gets the viewport transform.
        /// </summary>
        /// <param name="viewport">
        /// The viewport.
        /// </param>
        /// <returns>The transform.
        /// </returns>
        public static Matrix3D GetViewportTransform(Viewport3D viewport)
        {
            return new Matrix3D(
                viewport.ActualWidth / 2,
                0,
                0,
                0,
                0,
                -viewport.ActualHeight / 2,
                0,
                0,
                0,
                0,
                1,
                0,
                viewport.ActualWidth / 2,
                viewport.ActualHeight / 2,
                0,
                1);
        }

        /// <summary>
        /// Transforms a Point2D to a Point3D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="pointIn">The pt in.</param>
        /// <param name="pointNear">The point near.</param>
        /// <param name="pointFar">The point far.</param>
        /// <returns>The point 2 dto point 3 d.</returns>
        public static bool Point2DtoPoint3D(Viewport3D viewport, Point pointIn, out Point3D pointNear, out Point3D pointFar)
        {
            pointNear = new Point3D();
            pointFar = new Point3D();

            var pointIn3D = new Point3D(pointIn.X, pointIn.Y, 0);
            var matrixViewport = GetViewportTransform(viewport);
            var matrixCamera = GetCameraTransform(viewport);

            if (!matrixViewport.HasInverse)
            {
                return false;
            }

            if (!matrixCamera.HasInverse)
            {
                return false;
            }

            matrixViewport.Invert();
            matrixCamera.Invert();

            var pointNormalized = matrixViewport.Transform(pointIn3D);
            pointNormalized.Z = 0.01;
            pointNear = matrixCamera.Transform(pointNormalized);
            pointNormalized.Z = 0.99;
            pointFar = matrixCamera.Transform(pointNormalized);

            return true;
        }

        /// <summary>
        /// Transforms the Point3D to a Point2D.
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        /// <param name="point">The 3D point.</param>
        /// <returns>The point.</returns>
        public static Point Point3DtoPoint2D(Viewport3D viewport, Point3D point)
        {
            var matrix = GetTotalTransform(viewport);
            var pointTransformed = matrix.Transform(point);
            var pt = new Point(pointTransformed.X, pointTransformed.Y);
            return pt;
        }

        /// <summary>
        /// Prints the specified viewport.
        /// </summary>
        /// <param name="vp">
        /// The viewport.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public static void Print(Viewport3D vp, string description)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                dlg.PrintVisual(vp, description);
            }
        }

        /// <summary>
        /// Renders the viewport to a bitmap.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        /// <returns>A bitmap.</returns>
        public static RenderTargetBitmap RenderBitmap(Viewport3D view, Brush background)
        {
            var bmp = new RenderTargetBitmap(
                (int)view.ActualWidth, (int)view.ActualHeight, 96, 96,
                PixelFormats.Pbgra32);

            // erase background
            var vRect = new Rectangle
                            {
                                Width = view.ActualWidth,
                                Height = view.ActualHeight,
                                Fill = background
                            };
            vRect.Arrange(new Rect(0, 0, vRect.Width, vRect.Height));
            bmp.Render(vRect);

            bmp.Render(view);
            return bmp;
        }

        /// <summary>
        /// Renders the viewport to a bitmap.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="background">The background.</param>
        /// <param name="m">The oversampling multiplier.</param>
        /// <returns>A bitmap.</returns>
        public static BitmapSource RenderBitmap(Viewport3D view, double width, double height, Brush background)
        {
            double w = view.Width;
            double h = view.Height;
            ResizeAndArrange(view, width, height);
            var rtb = RenderBitmap(view, background);
            ResizeAndArrange(view, w, h);
            return rtb;
        }

        /// <summary>
        /// Resizes and arranges the viewport.
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        public static void ResizeAndArrange(Viewport3D view, double width, double height)
        {
            view.Width = width;
            view.Height = height;
            if (double.IsNaN(width) || double.IsNaN(height))
            {
                return;
            }

            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
        }

        /// <summary>
        /// Saves the viewport to a file.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="background">The background brush.</param>
        /// <param name="m">The oversampling multiplier.</param>
        public static void SaveBitmap(Viewport3D view, string fileName, Brush background)
        {

            var bmp = RenderBitmap(view, background);
            BitmapEncoder encoder;
            string ext = System.IO.Path.GetExtension(fileName);
            if (ext != null)
            {
                ext = ext.ToLower();
            }

            switch (ext)
            {
                case ".jpg":
                    var jpg = new JpegBitmapEncoder();
                    jpg.Frames.Add(BitmapFrame.Create(bmp));
                    encoder = jpg;
                    break;
                case ".png":
                    var png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(bmp));
                    encoder = png;
                    break;
                default:
                    throw new InvalidOperationException("Not supported file format.");
            }

            using (Stream stm = File.Create(fileName))
            {
                encoder.Save(stm);
            }
        }

        /// <summary>
        /// Recursive search in a Visual3D collection for objects of given type T
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <returns>A list of models.</returns>
        public static IList<Model3D> SearchFor<T>(IEnumerable<Visual3D> collection)
        {
            var output = new List<Model3D>();
            SearchFor(collection, typeof(T), output);
            return output;
        }

        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <param name="current">
        /// The current.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="parentTransform">
        /// The parent transform.
        /// </param>
        /// <returns>
        /// The transform.
        /// </returns>
        private static GeneralTransform3D GetTransform(Model3D current, Model3D model, Transform3D parentTransform)
        {
            var currentTransform = Transform3DHelper.CombineTransform(current.Transform, parentTransform);
            if (current == model)
            {
                return currentTransform;
            }

            var mg = current as Model3DGroup;
            if (mg != null)
            {
                foreach (var m in mg.Children)
                {
                    var result = GetTransform(m, model, currentTransform);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursive search for an object of a given type
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="type">The type.</param>
        /// <param name="output">The output.</param>
        private static void SearchFor(IEnumerable<Visual3D> collection, Type type, IList<Model3D> output)
        {
            foreach (var visual in collection)
            {
                var modelVisual = visual as ModelVisual3D;
                if (modelVisual != null)
                {
                    var model = modelVisual.Content;
                    if (model != null)
                    {
                        if (type.IsInstanceOfType(model))
                        {
                            output.Add(model);
                        }

                        // recursive
                        SearchFor(modelVisual.Children, type, output);
                    }

                    var modelGroup = model as Model3DGroup;
                    if (modelGroup != null)
                    {
                        SearchFor(modelGroup.Children, type, output);
                    }
                }
            }
        }

        /// <summary>
        /// Searches for models of the specified type.
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="output">
        /// The output.
        /// </param>
        private static void SearchFor(IEnumerable<Model3D> collection, Type type, IList<Model3D> output)
        {
            foreach (var model in collection)
            {
                if (type.IsInstanceOfType(model))
                {
                    output.Add(model);
                }

                var group = model as Model3DGroup;
                if (group != null)
                {
                    SearchFor(group.Children, type, output);
                }
            }
        }
    }
}