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
using Path=System.IO.Path;

namespace HelixEngine
{
    public static class Viewport3DHelper
    {
        #region From Charles Petzold "3D programming for Windows"

        #region CameraInfo

        /// <summary>
        /// Obtains the view transform matrix for a camera. (see page 327)
        /// </summary>
        /// <param name="camera">Camera to obtain the ViewMatrix for</param>
        /// <returns>A Matrix3D object with the camera view transform matrix, or a Matrix3D with all zeros if the "camera" is null.</returns>
        /// <exception cref="ApplicationException">if the 'camera' is neither of type MatrixCamera nor ProjectionCamera. </exception>
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

                Vector3D zAxis = -projcam.LookDirection;
                zAxis.Normalize();

                Vector3D xAxis = Vector3D.CrossProduct(projcam.UpDirection, zAxis);
                xAxis.Normalize();

                Vector3D yAxis = Vector3D.CrossProduct(zAxis, xAxis);
                var pos = (Vector3D) projcam.Position;

                return new Matrix3D(
                    xAxis.X, yAxis.X, zAxis.X, 0,
                    xAxis.Y, yAxis.Y, zAxis.Y, 0,
                    xAxis.Z, yAxis.Z, zAxis.Z, 0,
                    -Vector3D.DotProduct(xAxis, pos),
                    -Vector3D.DotProduct(yAxis, pos),
                    -Vector3D.DotProduct(zAxis, pos), 1);
            }

            throw new ApplicationException("unknown camera type");
        }

        /// <summary>
        /// Projection matrix, page 327-331
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="aspectRatio"></param>
        /// <returns></returns>
        public static Matrix3D GetProjectionMatrix(Camera camera, double aspectRatio)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            if (camera is MatrixCamera)
            {
                return (camera as MatrixCamera).ProjectionMatrix;
            }

            if (camera is OrthographicCamera)
            {
                var orthocam = camera as OrthographicCamera;

                double xScale = 2/orthocam.Width;
                double yScale = xScale*aspectRatio;
                double zNear = orthocam.NearPlaneDistance;
                double zFar = orthocam.FarPlaneDistance;

                // Hey, check this out!
                if (Double.IsPositiveInfinity(zFar))
                    zFar = 1E10;

                return new Matrix3D(xScale, 0, 0, 0,
                                    0, yScale, 0, 0,
                                    0, 0, 1/(zNear - zFar), 0,
                                    0, 0, zNear/(zNear - zFar), 1);
            }

            if (camera is PerspectiveCamera)
            {
                var perscam = camera as PerspectiveCamera;

                // The angle-to-radian formula is a little off because only
                // half the angle enters the calculation.
                double xScale = 1/Math.Tan(Math.PI*perscam.FieldOfView/360);
                double yScale = xScale*aspectRatio;
                double zNear = perscam.NearPlaneDistance;
                double zFar = perscam.FarPlaneDistance;
                double zScale = (zFar == double.PositiveInfinity ? -1 : (zFar/(zNear - zFar)));
                double zOffset = zNear*zScale;

                return new Matrix3D(xScale, 0, 0, 0,
                                    0, yScale, 0, 0,
                                    0, 0, zScale, -1,
                                    0, 0, zOffset, 0);
            }

            throw new ApplicationException("unknown camera type");
        }

        /// <summary>
        /// Get the combined view and projection transform
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="aspectRatio"></param>
        /// <returns></returns>
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


        public static Matrix3D GetInverseTransform(Camera cam, double aspectRatio)
        {
            var m = GetTotalTransform(cam, aspectRatio);

            if (!m.HasInverse)
            {
                throw new ApplicationException("camera transform has no inverse");
            }

            m.Invert();
            return m;
        }

        #endregion

        #region ViewportInfo

        public static Matrix3D GetTotalTransform(Viewport3DVisual vis)
        {
            var m = GetCameraTransform(vis);
            m.Append(GetViewportTransform(vis));
            return m;
        }

        public static Matrix3D GetTotalTransform(Viewport3D viewport)
        {
            Matrix3D matx = GetCameraTransform(viewport);
            matx.Append(GetViewportTransform(viewport));
            return matx;
        }

        public static Matrix3D GetCameraTransform(Viewport3DVisual vis)
        {
            return GetTotalTransform(vis.Camera,
                                     vis.Viewport.Size.Width/vis.Viewport.Size.Height);
        }

        public static Matrix3D GetCameraTransform(Viewport3D viewport)
        {
            return GetTotalTransform(viewport.Camera,
                                     viewport.ActualWidth/viewport.ActualHeight);
        }

        public static Matrix3D GetViewportTransform(Viewport3DVisual vis)
        {
            return new Matrix3D(vis.Viewport.Width/2, 0, 0, 0,
                                0, -vis.Viewport.Height/2, 0, 0,
                                0, 0, 1, 0,
                                vis.Viewport.X + vis.Viewport.Width/2,
                                vis.Viewport.Y + vis.Viewport.Height/2, 0, 1);
        }

        public static Matrix3D GetViewportTransform(Viewport3D viewport)
        {
            return new Matrix3D(viewport.ActualWidth/2, 0, 0, 0,
                                0, -viewport.ActualHeight/2, 0, 0,
                                0, 0, 1, 0,
                                viewport.ActualWidth/2,
                                viewport.ActualHeight/2, 0, 1);
        }


        public static Point Point3DtoPoint2D(Viewport3D viewport, Point3D point)
        {
            var matrix = GetTotalTransform(viewport);
            var pointTransformed = matrix.Transform(point);
            var pt = new Point(pointTransformed.X, pointTransformed.Y);
            return pt;
        }

        public static bool Point2DtoPoint3D(Viewport3D viewport, Point ptIn, out Point3D pointNear, out Point3D pointFar)
        {
            pointNear = new Point3D();
            pointFar = new Point3D();

            var pointIn = new Point3D(ptIn.X, ptIn.Y, 0);
            var matrixViewport = GetViewportTransform(viewport);
            var matrixCamera = GetCameraTransform(viewport);

            if (!matrixViewport.HasInverse)
                return false;

            if (!matrixCamera.HasInverse)
                return false;

            matrixViewport.Invert();
            matrixCamera.Invert();

            Point3D pointNormalized = matrixViewport.Transform(pointIn);
            pointNormalized.Z = 0.01;
            pointNear = matrixCamera.Transform(pointNormalized);
            pointNormalized.Z = 0.99;
            pointFar = matrixCamera.Transform(pointNormalized);

            return true;
        }

        #endregion

        #endregion

        #region Helper methods based on Eric Sink's twelve days of WPF

        // The Twelve Days of WPF 3D
        // http://www.ericsink.com/wpf3d/index.html

        public static Rect3D GetBoundingRect(MeshGeometry3D mg3d, Transform3D transform)
        {
            var r = new Rect3D();

            foreach (Point3D p3d in mg3d.Positions)
            {
                Point3D pb = transform.Transform(p3d);
                r.Union(pb);
            }
            throw new NotImplementedException();
        }

        public static Rect3D GetBoundingRect(Visual3D mg3d, Transform3D transform)
        {
            throw new NotImplementedException();
        }

        // todo: move from viewport3dextensions

        public static Transform3D GetTotalTransform(Visual3D visual)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(visual);
            throw new NotImplementedException();
        }

        private static Transform3D CombinedTransform(Transform3D t1, Transform3D t2)
        {
            var tg = new Transform3DGroup();
            tg.Children.Add(t1);
            tg.Children.Add(t2);
            return tg;
        }

        public static void TraverseModel3D(DependencyObject parent, Transform3D transform,
                                           Action<Model3D, Transform3D> action)
        {
            int n = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < n; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                var visual = child as Visual3D;
                Transform3D childTransform = visual != null ? CombinedTransform(transform, visual.Transform) : transform;

                Debug.WriteLine(child.GetType());
                Debug.Indent();
                TraverseModel3D(child, childTransform, action);
                Debug.Unindent();
            }
            var m = parent as MyModelUIElement3D;
            if (m != null)
            {
                action(m.Model, CombinedTransform(transform, m.Transform));
            }
        }

        // http://www.ericsink.com/wpf3d/3_Bitmap.html
        public static RenderTargetBitmap RenderBitmap(Viewport3D view, Brush background)
        {
            var bmp = new RenderTargetBitmap(
                (int) view.ActualWidth, (int) view.ActualHeight, 96, 96,
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

        public static RenderTargetBitmap RenderBitmap(Viewport3D view, double width, double height, Brush background)
        {
            double w = view.Width;
            double h = view.Height;
            ResizeAndArrange(view, width, height);
            RenderTargetBitmap rtb = RenderBitmap(view,background);
            ResizeAndArrange(view, w, h);
            return rtb;
        }

        public static void Copy(Viewport3D view)
        {
            Clipboard.SetImage(RenderBitmap(view,Brushes.White));
        }

        public static void Copy(Viewport3D view, double width, double height,Brush background)
        {
            Clipboard.SetImage(RenderBitmap(view, width, height,background));
        }

        public static void Save(Viewport3D view, string fileName)
        {
            RenderTargetBitmap bmp = RenderBitmap(view,Brushes.White);
            string ext = Path.GetExtension(fileName).ToLower();
            BitmapEncoder enc;
            switch (ext)
            {
                case ".jpg":
                    var jpg = new JpegBitmapEncoder();
                    jpg.Frames.Add(BitmapFrame.Create(bmp));
                    enc = jpg;
                    break;
                default:
                    var png = new PngBitmapEncoder();
                    png.Frames.Add(BitmapFrame.Create(bmp));
                    enc = png;
                    break;
            }
            using (Stream stm = File.Create(fileName))
            {
                enc.Save(stm);
            }
        }

        public static void ResizeAndArrange(Viewport3D view, double width, double height)
        {
            view.Width = width;
            view.Height = height;
            if (double.IsNaN(width) || double.IsNaN(height))
                return;
            view.Measure(new Size(width, height));
            view.Arrange(new Rect(0, 0, width, height));
        }

        // http://www.ericsink.com/wpf3d/7_XAML.html
        public static void CopyXaml(Viewport3D view)
        {
            Clipboard.SetText(XamlWriter.Save(view));
        }

        public static string GetXaml(Viewport3D view)
        {
            var sb = new StringBuilder();
            var tw = new StringWriter(sb);
            var xw = new XmlTextWriter(tw) {Formatting = Formatting.Indented};
            XamlWriter.Save(view, xw);
            xw.Close();
            string xaml = sb.ToString();

            xaml = xaml.Replace(string.Format(
                                    "<Viewport3D Height=\"{0}\" Width=\"{1}\" ",
                                    view.ActualHeight, view.ActualWidth),
                                "<Viewport3D ");

            return xaml;
        }

        // http://www.ericsink.com/wpf3d/A_AutoZoom.html
        /*        public static Rect Get2DBoundingBox(Viewport3D vp)
                {
                    bool bOK;

                    Viewport3DVisual vpv =
                        VisualTreeHelper.GetParent(
                            vp.Children[0]) as Viewport3DVisual;

                    Matrix3D m = _3DTools.MathUtils.TryWorldToViewportTransform(vpv, out bOK);

                    bool bFirst = true;
                    Rect r = new Rect();

                    foreach (Visual3D v3d in vp.Children)
                    {
                        if (v3d is ModelVisual3D)
                        {
                            ModelVisual3D mv3d = (ModelVisual3D)v3d;
                            if (mv3d.Content is GeometryModel3D)
                            {
                                GeometryModel3D gm3d =
                                    (GeometryModel3D)mv3d.Content;

                                if (gm3d.Geometry is MeshGeometry3D)
                                {
                                    MeshGeometry3D mg3d =
                                        (MeshGeometry3D)gm3d.Geometry;

                                    foreach (Point3D p3d in mg3d.Positions)
                                    {
                                        Point3D pb = m.Transform(p3d);
                                        Point p2d = new Point(pb.X, pb.Y);
                                        if (bFirst)
                                        {
                                            r = new Rect(p2d, new Size(1, 1));
                                            bFirst = false;
                                        }
                                        else
                                        {
                                            r.Union(p2d);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return r;
                }

                private static bool TooBig(Viewport3D vp)
                {
                    Rect r = Get2DBoundingBox(vp);

                    if (r.Left < 0)
                    {
                        return true;
                    }
                    if (r.Right > vp.ActualWidth)
                    {
                        return true;
                    }
                    if (r.Top < 0)
                    {
                        return true;
                    }
                    if (r.Bottom > vp.ActualHeight)
                    {
                        return true;
                    }
                    return false;
                }
                */

        // todo...
        public static void Fit(Viewport3D vp)
        {
            /*if (vp.TooBig())
            {
                while (vp.TooBig())
                {
                    slider_zoom.Value -= 0.1;
                }
                while (!TooBig())
                {
                    slider_zoom.Value += 0.01;
                }
                slider_zoom.Value -= 0.01;
            }
            else
            {
                while (!TooBig())
                {
                    slider_zoom.Value += 0.1;
                }
                while (TooBig())
                {
                    slider_zoom.Value -= 0.01;
                }
            }
             */
        }

        public static void Print(Viewport3D vp, string description)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                dlg.PrintVisual(vp, description);
            }
        }

        #endregion

        /// <summary>
        /// Get all lights in the Viewport3D
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public static Light[] GetLights(Viewport3D viewport)
        {
            List<Model3D> models = SearchFor<Light>(viewport.Children);
            var lights = new List<Light>();
            foreach (Model3D m in models)
                lights.Add(m as Light);
            return lights.ToArray();
        }

        /// <summary>
        /// Recursive search in a collection for objects of given type T 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static List<Model3D> SearchFor<T>(IEnumerable<Visual3D> collection)
        {
            var output = new List<Model3D>();
            SearchFor(collection, typeof (T), output);
            return output;
        }

        /// <summary>
        /// Recursive search for an object of a given type
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="type"></param>
        /// <param name="output"></param>
        private static void SearchFor(IEnumerable<Visual3D> collection, Type type, ICollection<Model3D> output)
        {
            foreach (Visual3D visual in collection)
            {
                var mv3D = visual as ModelVisual3D;
                if (mv3D != null)
                {
                    Model3D m3D = mv3D.Content;
                    if (m3D != null)
                    {
                        if (Inherits(m3D.GetType(), type))
                            output.Add(m3D);
                        // recursive
                        SearchFor(mv3D.Children, type, output);
                    }
                }
            }
        }

        // todo: how to do this properly??
        private static bool Inherits(Type inheritedType, Type type)
        {
            while (inheritedType != null)
            {
                if (inheritedType == type)
                    return true;
                inheritedType = inheritedType.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Find the Visual3D that is nearest given a 2D position in the viewport
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Visual3D FindNearestVisual(Viewport3D viewport, Point position)
        {
            Point3D p;
            Vector3D n;
            DependencyObject obj;
            if (FindNearest(viewport, position, out p, out n, out obj))
                return obj as Visual3D;

            return null;
        }

        /// <summary>
        /// Find the coordinates of the nearest point given a 2D position in the viewport
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point3D? FindNearestPoint(Viewport3D viewport, Point position)
        {
            Point3D p;
            Vector3D n;
            DependencyObject obj;
            if (FindNearest(viewport, position, out p, out n, out obj))
                return p;

            return null;
        }

        public static bool FindNearest(Viewport3D viewport, Point position, out Point3D point, out Vector3D normal,
                                       out DependencyObject visual)
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

            VisualTreeHelper.HitTest(viewport, null, delegate(HitTestResult hit)
                                                         {
                                                             var rayHit = hit as RayMeshGeometry3DHitTestResult;
                                                             if (rayHit != null)
                                                             {
                                                                 MeshGeometry3D mesh = rayHit.MeshHit;
                                                                 if (mesh != null)
                                                                 {
                                                                     Point3D p1 = mesh.Positions[rayHit.VertexIndex1];
                                                                     Point3D p2 = mesh.Positions[rayHit.VertexIndex2];
                                                                     Point3D p3 = mesh.Positions[rayHit.VertexIndex3];
                                                                     double x = p1.X*rayHit.VertexWeight1 +
                                                                                p2.X*rayHit.VertexWeight2 +
                                                                                p3.X*rayHit.VertexWeight3;
                                                                     double y = p1.Y*rayHit.VertexWeight1 +
                                                                                p2.Y*rayHit.VertexWeight2 +
                                                                                p3.Y*rayHit.VertexWeight3;
                                                                     double z = p1.Z*rayHit.VertexWeight1 +
                                                                                p2.Z*rayHit.VertexWeight2 +
                                                                                p3.Z*rayHit.VertexWeight3;

                                                                     // point in local coordinates
                                                                     var p = new Point3D(x, y, z);
                                                                     // transform to global coordinates
                                                                     GeneralTransform3D t = GetTransform(viewport,
                                                                                                         hit.VisualHit
                                                                                                         as Visual3D);
                                                                     if (t != null)
                                                                         p = t.Transform(p);

                                                                     double distance =
                                                                         (camera.Position - p).LengthSquared;
                                                                     if (distance < minimumDistance)
                                                                     {
                                                                         minimumDistance = distance;
                                                                         nearestPoint = p;
                                                                         nearestNormal = Vector3D.CrossProduct(p2 - p1,
                                                                                                               p3 - p1);
                                                                         nearestObject = hit.VisualHit;
                                                                     }
                                                                 }
                                                             }
                                                             return HitTestResultBehavior.Continue;
                                                         }, hitParams);

            point = nearestPoint;
            visual = nearestObject;
            normal = nearestNormal;

            if (minimumDistance == double.MaxValue)
                return false;

            normal.Normalize();
            return true;
        }

        /// <summary>
        /// Get the total transform of a Visual3D
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static GeneralTransform3D GetTransform(Viewport3D viewport, Visual3D visual)
        {
            if (visual == null)
                return null;

            foreach (Visual3D ancestor in viewport.Children)
            {
                if (visual.IsDescendantOf(ancestor))
                {
                    var g = new GeneralTransform3DGroup();
                    g.Children.Add(ancestor.Transform);
                    GeneralTransform3D dt = visual.TransformToAncestor(ancestor);
                    if (dt != null)
                        g.Children.Add(dt);
                    return g;
                }
            }
            return visual.Transform;
        }
    }
}