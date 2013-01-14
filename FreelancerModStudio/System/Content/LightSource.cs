using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class LightSource : ContentBase
    {
        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.LightSource;
        }

        public override Rect3D GetShapeBounds()
        {
            return new Rect3D(-0.5, -0.5, -0.5, 1, 1, 1);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
