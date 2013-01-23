using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Helpers
{
    // Derive from TransformInfo so can define ZeroMatrix and Name properties ????

    public class VisualInfo : Animatable
    {
        public static readonly Matrix3D ZeroMatrix = new Matrix3D(0, 0, 0, 0, 0, 0, 0, 0,
                                                           0, 0, 0, 0, 0, 0, 0, 0);


        public static readonly DependencyProperty ModelVisual3DProperty =
            DependencyProperty.Register("ModelVisual3D",
                typeof(ModelVisual3D),
                typeof(VisualInfo),
                new PropertyMetadata(null, ModelVisual3DChanged));

        public ModelVisual3D ModelVisual3D
        {
            set { SetValue(ModelVisual3DProperty, value); }
            get { return (ModelVisual3D)GetValue(ModelVisual3DProperty); }
        }

        static readonly DependencyPropertyKey TotalTransformKey =
            DependencyProperty.RegisterReadOnly("TotalTransform",
                typeof(Matrix3D),
                typeof(VisualInfo),
                new PropertyMetadata(new Matrix3D()));

        public static readonly DependencyProperty TotalTransformProperty =
            TotalTransformKey.DependencyProperty;

        public Matrix3D TotalTransform
        {
            protected set { SetValue(TotalTransformKey, value); }
            get { return (Matrix3D)GetValue(TotalTransformProperty); }
        }


        static void ModelVisual3DChanged(DependencyObject obj, 
                                         DependencyPropertyChangedEventArgs args)
        {
            (obj as VisualInfo).ModelVisual3DChanged(args);
        }

        void ModelVisual3DChanged(DependencyPropertyChangedEventArgs args)
        {
            TotalTransform = GetTotalTransform(args.NewValue as ModelVisual3D);
        }



        // TotalTransform

        // ViewportTransoform

        // SpaceTransform




                                // could have Model3D here as well????
                                // But can't find group that it's a part of.
                                // Plus, it's not unique since it can be shared !!!!


        public static Matrix3D GetTotalTransform(DependencyObject obj)      // argument is really a ModelVisual3D
        {
            Matrix3D matx = Matrix3D.Identity;

            while (!(obj is Viewport3DVisual))
            {
                // This occurs when the visual is parent-less.
                if (obj == null)
                {
                    return ZeroMatrix;
                }

                else if (obj is ModelVisual3D)
                {
                    if ((obj as ModelVisual3D).Transform != null)
                        matx.Append((obj as ModelVisual3D).Transform.Value);
                }

                else
                {
                    throw new ApplicationException("didn't end in Viewport3DVisual");
                }

                obj = VisualTreeHelper.GetParent(obj);
            }

            // At this point, we know obj is Viewport3DVisual
            Viewport3DVisual vis = obj as Viewport3DVisual;
            Matrix3D matxViewport = Viewport3DHelper.GetTotalTransform(vis);
            matx.Append(matxViewport);

            return matx;
        }

        public override string ToString()
        {
            return TotalTransform.ToString();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new VisualInfo();
        }
    }
}
