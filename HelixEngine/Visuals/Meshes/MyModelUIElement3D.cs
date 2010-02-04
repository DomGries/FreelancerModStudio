using System.Windows;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// Alternative to the System.Windows.Media.Media3D.ModelUIElement3D (sealed)
    ///
    /// </summary>
    public class MyModelUIElement3D : UIElement3D
    {
        // The Children property for the sphere
        private static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Children",
                                        typeof (Model3D),
                                        typeof (MyModelUIElement3D),
                                        new PropertyMetadata(ModelPropertyChanged));

        public Model3D Model
        {
            get { return (Model3D) GetValue(ModelProperty); }

            set { SetValue(ModelProperty, value); }
        }

        private static void ModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (MyModelUIElement3D) d;
            s.Visual3DModel = s.Model;
        }
    }
}