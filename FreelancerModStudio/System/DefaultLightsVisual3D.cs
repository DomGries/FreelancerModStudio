using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter
{
    public class SystemLightsVisual3D : ModelVisual3D
    {
        public SystemLightsVisual3D()
        {
            Model3DGroup lightGroup = new Model3DGroup();

            lightGroup.Children.Add(new AmbientLight(Color.FromRgb(100, 100, 100)));
            lightGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0.2, 0.2, -1)));

            Content = lightGroup;
        }
    }

    public class UniverseLightsVisual3D : ModelVisual3D
    {
        public UniverseLightsVisual3D()
        {
            Model3DGroup lightGroup = new Model3DGroup();

            lightGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0.2, 0.2, -1)));

            Content = lightGroup;
        }
    }
}
