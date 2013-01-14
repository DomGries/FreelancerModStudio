using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class SystemObject : ContentBase
    {
        protected override Model3D GetShapeModel()
        {
            switch (Block.ObjectType)
            {
                case ContentType.Construct:
                    return SharedGeometries.Construct;
                case ContentType.Depot:
                    return SharedGeometries.Depot;
                case ContentType.DockingRing:
                    return SharedGeometries.DockingRing;
                case ContentType.JumpGate:
                    return SharedGeometries.JumpGate;
                case ContentType.JumpHole:
                    return SharedGeometries.JumpHole;
                case ContentType.Planet:
                    return SharedGeometries.Planet;
                case ContentType.Satellite:
                    return SharedGeometries.Satellite;
                case ContentType.Ship:
                    return SharedGeometries.Ship;
                case ContentType.Station:
                    return SharedGeometries.Station;
                case ContentType.Sun:
                    return SharedGeometries.Sun;
                case ContentType.TradeLane:
                    return SharedGeometries.TradeLane;
                case ContentType.WeaponsPlatform:
                    return SharedGeometries.WeaponsPlatform;
            }

            return null;
        }

        public override Rect3D GetShapeBounds()
        {
            switch (Block.ObjectType)
            {
                case ContentType.Planet:
                case ContentType.Sun:
                    return new Rect3D(-1, -1, -1, 2, 2, 2);
            }
            return new Rect3D(-0.5, -0.5, -0.5, 1, 1, 1);
        }

        public override bool IsEmissive()
        {
            return false;
        }
    }
}
