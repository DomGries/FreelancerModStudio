using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace FreelancerModStudio.SystemPresenter
{
    public class DefaultLightsVisual3D : ModelVisual3D
    {
        /// <summary>
        /// A <see cref="ModelVisual3D"/> consisting of a 3-point directional light setup.
        /// </summary>
        public DefaultLightsVisual3D()
        {
            Model3DGroup lightGroup = new Model3DGroup();
            // http://www.3drender.com/light/3point.html

            lightGroup.Children.Add(new AmbientLight(Color.FromRgb(128, 128, 128)));
            lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(128, 128, 128), new Vector3D(0.5, -0.5, -1)));

            // key light
            //lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(180, 180, 180), new Vector3D(-1, -1, -1)));
            //// fill light
            //lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(120, 120, 120), new Vector3D(1, -1, -0.1)));
            //// rim/back light
            //lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(60, 60, 60), new Vector3D(0.1, 1, -1)));
            //// and a little bit from below
            //lightGroup.Children.Add(new DirectionalLight(Color.FromRgb(50, 50, 50), new Vector3D(0.1, 0.1, 1)));

            //lightGroup.Children.Add(new AmbientLight(Color.FromRgb(30, 30, 30)));

            this.Content = lightGroup;
        }
    }

    //public class TestModel : ModelVisual3D
    //{
    //    public TestModel()
    //    {
    //        Point3DCollection vertices = new Point3DCollection();
    //        Vector3DCollection normals = new Vector3DCollection();
    //        Int32Collection indices = new Int32Collection();
    //        //PointCollection textures = new PointCollection();

    //        //Model3D model = new Model3D();
    //        //group.Children.Add();

    //        //this.Content = lightGroup;
    //    }
    //}
}
