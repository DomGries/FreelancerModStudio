using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class Connection : ContentBase
    {
        public ContentBase From { get; set; }
        public ContentBase To { get; set; }
        public ConnectionType FromType { get; set; }
        public ConnectionType ToType { get; set; }

        protected override Model3D GetShapeModel()
        {
            Material material;

            if (FromType == ToType)
            {
                //solid brush
                material = MaterialHelper.CreateEmissiveMaterial(GetColor(FromType));
            }
            else
            {
                //gradient brush
                material = MaterialHelper.CreateEmissiveMaterial(new LinearGradientBrush(GetColor(FromType), GetColor(ToType), 90));
            }

            return SharedGeometries.GetGeometry(SharedMeshes.Surface, material);
        }

        static Color GetColor(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.JumpGate:
                    return SharedMaterials.ConnectionJumpGate;
                case ConnectionType.JumpHole:
                    return SharedMaterials.ConnectionJumpHole;
                case ConnectionType.JumpGateAndHole:
                    return SharedMaterials.ConnectionJumpGateAndHole;
                default:
                    return SharedMaterials.ConnectionNone;
            }
        }

        public override Vector3D GetBaseScale()
        {
            return new Vector3D(1, 0.5, 1);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
