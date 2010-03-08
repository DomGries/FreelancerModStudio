using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace FreelancerModStudio.SystemPresenter
{
    public class DefaultLightsVisual3D : ModelVisual3D
    {
        public DefaultLightsVisual3D()
        {
            Model3DGroup lightGroup = new Model3DGroup();

            lightGroup.Children.Add(new AmbientLight(Color.FromRgb(128, 128, 128)));
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(128, 128, 128), new Vector3D(0.5, -0.5, -1)));

            this.Content = lightGroup;
        }
    }
}
