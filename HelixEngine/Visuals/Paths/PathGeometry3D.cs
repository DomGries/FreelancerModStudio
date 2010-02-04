//-----------------------------------------------
// PathGeometry3D.cs (c) 2007 by Charles Petzold
//-----------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Paths
{
    /// <summary>
    /// 
    /// </summary>
    //------------------------------------------------------------------------------
    // TODO: [System.ComponentModel.TypeConverter(typeof(PathGeometry3DConverter))]
    //------------------------------------------------------------------------------
    [System.Windows.Markup.ContentProperty("Figures")] 
    public class PathGeometry3D : Animatable
    {
        // TODO: Bounds, FillRule, MayHaveCurves

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty FiguresProperty =
            DependencyProperty.Register("Figures", 
                typeof(PathFigure3DCollection),
                typeof(PathGeometry3D));

        /// <summary>
        /// 
        /// </summary>
        public PathFigure3DCollection Figures
        {
            set { SetValue(FiguresProperty, value); }
            get { return (PathFigure3DCollection)GetValue(FiguresProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public PathGeometry3D()
        {
            Figures = new PathFigure3DCollection();
        }

        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform",
                typeof(Transform3D),
                typeof(PathGeometry3D),
                new PropertyMetadata(Transform3D.Identity));

        public Transform3D Transform
        {
            set { SetValue(TransformProperty, value); }
            get { return (Transform3D)GetValue(TransformProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="point"></param>
        /// <param name="tangent"></param>
        public void GetPointAtFractionLength(double progress, out Point3D point, out Vector3D tangent)
        {
            progress = Math.Max(0, Math.Min(1, progress));
            point = new Point3D();
            tangent = new Vector3D();
            double lengthTotal = 0, length = 0;

            foreach (PathFigure3D fig in Figures)
            {
                PathFigure3D figFlattened = fig.GetFlattenedPathFigure();
                Point3D ptStart = figFlattened.StartPoint;

                foreach (PathSegment3D seg in figFlattened.Segments)
                {
                    PolyLineSegment3D segPoly = seg as PolyLineSegment3D;

                    foreach (Point3D pt in segPoly.Points)
                    {
                        lengthTotal += (pt - ptStart).Length;
                        ptStart = pt;
                    }
                }
            }

            foreach (PathFigure3D fig in Figures)
            {
                PathFigure3D figFlattened = fig.GetFlattenedPathFigure();
                Point3D ptStart = figFlattened.StartPoint;

                foreach (PathSegment3D seg in figFlattened.Segments)
                {
                    PolyLineSegment3D segPoly = seg as PolyLineSegment3D;

                    foreach (Point3D pt in segPoly.Points)
                    {
                        tangent = pt - ptStart;
                        double lengthSeg = tangent.Length;
                        length += lengthSeg;

                        if (length / lengthTotal >= progress)
                        {
                            double factor1 = ((length / lengthTotal) - progress) / (lengthSeg / lengthTotal);
                            double factor2 = 1 - factor1;

                            point = (Point3D) (factor1 * (Vector3D)ptStart +
                                               factor2 * (Vector3D)pt);
                            return;
                        }
                        
                        ptStart = pt;
                    }
                }
            }
        }

        public override string ToString()
        {
            return Figures.ToString();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PathGeometry3D();
        }
    }
}
