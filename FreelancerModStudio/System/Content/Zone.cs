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

        public override Rect3D GetShapeBounds()
        {
            switch (Block.ObjectType)
            {
                case ContentType.ZoneBox:
                case ContentType.ZoneBoxExclusion:
                case ContentType.ZonePathTradeLane:
                    return new Rect3D(-0.5, -0.5, -0.5, 1, 1, 1); // box base mesh
                case ContentType.ZoneRing:
                case ContentType.ZoneCylinder:
                case ContentType.ZoneCylinderExclusion:
                case ContentType.ZonePath:
                case ContentType.ZonePathTrade:
                    return new Rect3D(-1, -0.5, -1, 2, 1, 2); // cylinder base mesh
            }
            return new Rect3D(-1, -1, -1, 2, 2, 2);
        }

        public override bool IsEmissive()
        {
            return true;
        }

        protected override Matrix3D GetMatrix()
        {
            // special scale for zone rings
            if (Block.ObjectType == ContentType.ZoneRing)
            {
                Vector3D newScale = Scale;

                // use bigger radius of either outer radius (X) or inner radius (Z) of ring
                if (newScale.X > newScale.Z)
                {
                    newScale.Z = newScale.X;
                }
                else
                {
                    newScale.X = newScale.Z;
                }

                Matrix3D matrix = new Matrix3D();

                matrix.Scale(newScale);
                matrix *= RotationMatrix(Rotation);
                matrix.Translate(Position);

                return matrix;
            }

            return base.GetMatrix();
        }
    }
}
