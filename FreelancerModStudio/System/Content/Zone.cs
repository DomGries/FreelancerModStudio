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
                case ContentType.ZoneSphere:
                case ContentType.ZoneEllipsoid:
                    return SharedGeometries.ZoneSphereOrEllipsoid;
                case ContentType.ZoneCylinderOrRing:
                    return SharedGeometries.ZoneCylinderOrRing;
                case ContentType.ZoneBox:
                    return SharedGeometries.ZoneBox;

                case ContentType.ZoneSphereExclusion:
                case ContentType.ZoneEllipsoidExclusion:
                    return SharedGeometries.ZoneSphereExclusion;
                case ContentType.ZoneCylinderOrRingExclusion:
                    return SharedGeometries.ZoneCylinderOrRingExclusion;
                case ContentType.ZoneBoxExclusion:
                    return SharedGeometries.ZoneBoxExclusion;

                case ContentType.ZoneVignette:
                    return SharedGeometries.ZoneVignette;
                case ContentType.ZonePath:
                    return SharedGeometries.ZonePathPatrol;
                case ContentType.ZonePathTrade:
                    return SharedGeometries.ZonePathTrade;
            }
        }

        public override Vector3D GetBaseScale()
        {
            switch (Block.ObjectType)
            {
                case ContentType.ZoneBox:
                case ContentType.ZoneBoxExclusion:
                    return new Vector3D(0.5, 0.5, 0.5);
                case ContentType.ZoneCylinderOrRing:
                case ContentType.ZoneCylinderOrRingExclusion:
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    return new Vector3D(1, 0.5, 1);
            }
            return new Vector3D(1, 1, 1);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
