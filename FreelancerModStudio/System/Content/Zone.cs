using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class Zone : ContentBase
    {
        protected override Model3D GetShapeModel()
        {
            switch (Block.ObjectType)
            {
                default:
                    return SharedGeometries.ZoneSphereOrEllipsoid;
                case ContentType.ZoneCylinder:
                case ContentType.ZoneRing:
                    return SharedGeometries.ZoneCylinderOrRing;
                case ContentType.ZoneBox:
                    return SharedGeometries.ZoneBox;

                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneEllipsoidExclusion:
                    return SharedGeometries.ZoneSphereOrEllipsoidExclusion;
                case ContentType.ZoneCylinderExclusion:
                    return SharedGeometries.ZoneCylinderExclusion;
                case ContentType.ZoneBoxExclusion:
                    return SharedGeometries.ZoneBoxExclusion;

                case ContentType.ZoneVignette:
                    return SharedGeometries.ZoneVignette;
                case ContentType.ZonePath:
                    return SharedGeometries.ZonePath;
                case ContentType.ZonePathTrade:
                    return SharedGeometries.ZonePathTrade;
                case ContentType.ZonePathTradeLane:
                    return SharedGeometries.ZonePathTradeLane;
            }
        }

        public override Vector3D GetBaseScale()
        {
            switch (Block.ObjectType)
            {
                case ContentType.ZoneBox:
                case ContentType.ZoneBoxExclusion:
                case ContentType.ZonePathTradeLane:
                    return new Vector3D(0.5, 0.5, 0.5); // box base mesh
                case ContentType.ZoneRing:
                case ContentType.ZoneCylinder:
                case ContentType.ZoneCylinderExclusion:
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    return new Vector3D(1, 0.5, 1); // cylinder base mesh
            }
            return new Vector3D(1, 1, 1);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
