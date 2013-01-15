using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine
{
    /// <summary>
    /// Creates diffuse/specular materials.
    /// </summary>
    public static class MaterialHelper
    {
        /// <summary>
        /// Changes the opacity of a material.
        /// </summary>
        /// <param name="material">
        /// The material.
        /// </param>
        /// <param name="d">
        /// The d.
        /// </param>
        public static void ChangeOpacity(Material material, double d)
        {
            var dm = material as DiffuseMaterial;
            if (dm != null)
            {
                var scb = dm.Brush as SolidColorBrush;
                if (scb != null)
                {
                    scb.Opacity = d;
                }
            }
        }

        /// <summary>
        /// Creates a material for the specified color.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <returns>
        /// </returns>
        public static Material CreateMaterial(Color color)
        {
            return CreateMaterial(new SolidColorBrush(color));
        }

        /// <summary>
        /// Creates a material for the specified color and opacity.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        /// <param name="opacity">
        /// The opacity.
        /// </param>
        /// <returns>
        /// </returns>
        public static Material CreateMaterial(Color color, double opacity)
        {
            return CreateMaterial(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B));
        }

        /// <summary>
        /// Creates a material for the specified brush.
        /// </summary>
        /// <param name="brush">
        /// The brush.
        /// </param>
        /// <returns>
        /// </returns>
        public static Material CreateMaterial(Brush brush)
        {
            return new DiffuseMaterial(brush);
        }

        public static Material CreateEmissiveMaterial(Color color)
        {
            return CreateEmissiveMaterial(new SolidColorBrush(color));
        }

        public static Material CreateEmissiveMaterial(Brush brush)
        {
            return new EmissiveMaterial(brush);
        }

        public static Material CreateSpectacularMaterial(Color color, double specularPower)
        {
            return CreateSpectacularMaterial(new SolidColorBrush(color), specularPower);
        }

        public static Material CreateSpectacularMaterial(Brush brush, double specularPower)
        {
            return new SpecularMaterial(brush, specularPower);
        }
    }
}