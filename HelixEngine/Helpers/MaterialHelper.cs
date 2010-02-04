using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// Creates diffuse/specular materials
    /// </summary>
    public static class MaterialHelper
    {
        public static double DefaultSpecularPower = 100;

        public static Brush CreateTransparentBrush(Brush brush, double opacity)
        {
            brush = brush.Clone();
            brush.Opacity = opacity;
            return brush;
        }

        public static Material CreateMaterial(Color c)
        {
            return CreateMaterial(new SolidColorBrush(c));
        }

        public static Material CreateMaterial(Color c, double opacity)
        {
            Color c2 = Color.FromArgb((byte)(opacity * 255), c.R, c.G, c.B);
            return CreateMaterial(c2);
        }

        public static Material CreateMaterial(Brush brush)
        {
            return CreateMaterial(brush, DefaultSpecularPower);
        }

        public static Material CreateMaterial(Brush brush, double specularPower)
        {
            var mg = new MaterialGroup();
            mg.Children.Add(new DiffuseMaterial(brush));
            if (specularPower > 0)
                mg.Children.Add(new SpecularMaterial(Brushes.White, specularPower));
            return mg;
        }

        public static Material CreateMaterial(Brush diffuse, Brush emissive, Brush specular, double opacity, double specularPower)
        {
            var mg = new MaterialGroup();
            if (diffuse != null)
            {
                diffuse = diffuse.Clone();
                diffuse.Opacity = opacity;
                mg.Children.Add(new DiffuseMaterial(diffuse));
            }
            if (emissive != null)
            {
                emissive = emissive.Clone();
                emissive.Opacity = opacity;
                mg.Children.Add(new EmissiveMaterial(emissive));
            }
            if (specular != null)
            {
                specular = specular.Clone();
                specular.Opacity = opacity;
                mg.Children.Add(new SpecularMaterial(specular, specularPower));
            }
            return mg;
        }

    }
}