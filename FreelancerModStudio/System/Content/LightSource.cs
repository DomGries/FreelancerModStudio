using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class LightSource : ContentBase
    {
        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.LightSource;
        }

        public override Vector3D GetBaseScale()
        {
            return new Vector3D(0.5, 0.5, 0.5);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
