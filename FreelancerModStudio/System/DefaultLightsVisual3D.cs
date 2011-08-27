using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace FreelancerModStudio.SystemPresenter
{
    public class DefaultLightsVisual3D : ModelVisual3D
    {
        public DefaultLightsVisual3D()
        {
            Model3DGroup lightGroup = new Model3DGroup();

            //lightGroup.Children.Add(new AmbientLight(Color.FromRgb(40, 40, 40)));
            lightGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0.2, 0.2, -1)));

            this.Content = lightGroup;
        }
    }
}
