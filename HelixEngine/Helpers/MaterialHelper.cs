using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// Creates diffuse/specular materials
    /// </summary>
    public static class MaterialHelper
    {
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
            return new DiffuseMaterial(brush);
        }

        public static Material CreateMaterial(Brush brush, double opacity)
        {
            return CreateMaterial(CreateTransparentBrush(brush, opacity));
        }

        public static Material CreateEmissiveMaterial(Color c)
        {
            return CreateEmissiveMaterial(new SolidColorBrush(c));
        }

        public static Material CreateEmissiveMaterial(Brush brush)
        {
            return new EmissiveMaterial(brush);
        }

        public static Material CreateEmissiveMaterial(Brush brush, double opacity)
        {
            return CreateEmissiveMaterial(CreateTransparentBrush(brush, opacity));
        }

        public static Material CreateSpectacularMaterial(Color c, double specularPower)
        {
            return CreateSpectacularMaterial(new SolidColorBrush(c), specularPower);
        }

        public static Material CreateSpectacularMaterial(Brush brush, double specularPower)
        {
            return new SpecularMaterial(brush, specularPower);
        }

        public static Material CreateSpectacularMaterial(Brush brush, double opacity, double specularPower)
        {
            return CreateSpectacularMaterial(CreateTransparentBrush(brush, opacity), specularPower);
        }
    }
}