//using System.Windows.Media;
//using System.Windows.Media.Media3D;

//namespace HelixEngine
//{
//    public enum ManipulatorType
//    {
//        TranslateX,
//        TranslateY,
//        TranslateZ,
//        TranslateXY,
//        TranslateXZ,
//        TranslateYZ
//    } ;

//    // todo: use a UIElement3D?

//    public class ManipulatorControl : ArrowVisual3D
//    {
//        private readonly Visual3D targetVisual;
//        private Point3D _lastMousePoint;
//        private Point3D _mouseDownPoint;
//        private bool isMoving;
//        private Material originalMaterial;

//        public ManipulatorControl()
//        {
//            Transform = new TranslateTransform3D();
//        }

//        public ManipulatorControl(ManipulatorType type, Visual3D visual)
//        {
//            targetVisual = visual;
//            Type = type;
//            Transform = targetVisual.Transform;
//            Diameter = 0.5;
//            HeadLength = Diameter*3;
//            double l = 5;
//            switch (type)
//            {
//                case ManipulatorType.TranslateX:
//                    Normal = new Vector3D(0, 0, 1);
//                    Direction = new Vector3D(l, 0, 0);
//                    Fill = Brushes.Red;
//                    break;
//                case ManipulatorType.TranslateY:
//                    Normal = new Vector3D(0, 0, 1);
//                    Direction = new Vector3D(0, l, 0);
//                    Fill = Brushes.Green;
//                    break;
//                case ManipulatorType.TranslateZ:
//                    Normal = new Vector3D(0, 1, 0);
//                    Direction = new Vector3D(0, 0, l);
//                    Fill = Brushes.Blue;
//                    break;
//            }
//        }

//        public ManipulatorType Type { get; set; }
//        public Vector3D Normal { get; set; }

//        public void InitMove(Ray3D ray)
//        {
//            Vector3D t = Vector3D.CrossProduct(ray.Direction, Direction);
//            Vector3D n = Vector3D.CrossProduct(Direction, t);
//            _mouseDownPoint = ray.PlaneIntersection(Origin, n);
//            _lastMousePoint = _mouseDownPoint;
//            originalMaterial = Material;
//            Fill = Brushes.Yellow;
//            isMoving = true;
//        }

//        public Vector3D Move(Ray3D ray)
//        {
//            if (!isMoving) return new Vector3D();
//            Vector3D t = Vector3D.CrossProduct(ray.Direction, Direction);
//            Vector3D n = Vector3D.CrossProduct(Direction, t);
//            Point3D mousePoint = ray.PlaneIntersection(Origin, n);
//            double x = 0;
//            double y = 0;
//            double z = 0;
//            switch (Type)
//            {
//                case ManipulatorType.TranslateX:
//                    x = mousePoint.X - _lastMousePoint.X;
//                    break;
//                case ManipulatorType.TranslateY:
//                    y = mousePoint.Y - _lastMousePoint.Y;
//                    break;
//                case ManipulatorType.TranslateZ:
//                    z = mousePoint.Z - _lastMousePoint.Z;
//                    break;
//            }
//            _lastMousePoint = mousePoint;
//            var delta = new Vector3D(x, y, z);
//            /* TranslateTransform3D transform = this.Transform as TranslateTransform3D;
//            transform.OffsetX += x;
//            transform.OffsetY += y;
//            transform.OffsetZ += z;*/
//            return delta;
//        }

//        public void EndMove()
//        {
//            isMoving = false;
//            Material = originalMaterial;
//        }

//        /*        public void UpdateVisual()
//                {
//                    Children.Clear();
//                    double l = Length;
//                    double d = l * 0.1;
//                    double hl = l * 0.3;
//                    arrow= new ArrowVisual3D() { Point2 = new Point3D(l, 0, 0), Diameter = d, HeadLength = hl, Fill = this.Fill };
//                    Children.Add(arrow);
//                }           */
//    }
//}