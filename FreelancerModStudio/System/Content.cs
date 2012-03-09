using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;

namespace FreelancerModStudio.SystemPresenter
{
    public class LightSource : ContentBase
    {
        protected override ModelVisual3D GetShapeModel()
        {
            return new ModelVisual3D { Content = SharedGeometries.LightSource };
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }

    public class SystemObject : ContentBase
    {
        public ContentType Type { get; set; }

        protected override ModelVisual3D GetShapeModel()
        {
            switch (Type)
            {
                case ContentType.Construct:
                    return new ModelVisual3D { Content = SharedGeometries.Construct };
                case ContentType.Depot:
                    return new ModelVisual3D { Content = SharedGeometries.Depot };
                case ContentType.DockingRing:
                    return new ModelVisual3D { Content = SharedGeometries.DockingRing };
                case ContentType.JumpGate:
                    return new ModelVisual3D { Content = SharedGeometries.JumpGate };
                case ContentType.JumpHole:
                    return new ModelVisual3D { Content = SharedGeometries.JumpHole };
                case ContentType.Planet:
                    return new ModelVisual3D { Content = SharedGeometries.Planet };
                case ContentType.Satellite:
                    return new ModelVisual3D { Content = SharedGeometries.Satellite };
                case ContentType.Ship:
                    return new ModelVisual3D { Content = SharedGeometries.Ship };
                case ContentType.Station:
                    return new ModelVisual3D { Content = SharedGeometries.Station };
                case ContentType.Sun:
                    return new ModelVisual3D { Content = SharedGeometries.Sun };
                case ContentType.TradeLane:
                    return new ModelVisual3D { Content = SharedGeometries.TradeLane };
                case ContentType.WeaponsPlatform:
                    return new ModelVisual3D { Content = SharedGeometries.WeaponsPlatform };
            }

            return null;
        }

        public override bool IsEmissive()
        {
            return false;
        }
    }

    public class Zone : ContentBase
    {
        public ZoneShape Shape { get; set; }
        public ZoneType Type { get; set; }

        protected override ModelVisual3D GetShapeModel()
        {
            //special zones
            switch (Type)
            {
                case ZoneType.PathPatrol:
                    return new ModelVisual3D { Content = SharedGeometries.ZonePathPatrol };
                case ZoneType.PathTrade:
                    return new ModelVisual3D { Content = SharedGeometries.ZonePathTrade };
                case ZoneType.Vignette:
                    return new ModelVisual3D { Content = SharedGeometries.ZoneVignette };
                case ZoneType.Exclusion:
                    switch (Shape)
                    {
                        case ZoneShape.Box:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneBoxExclusion };
                        case ZoneShape.Cylinder:
                        case ZoneShape.Ring:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneCylinderOrRingExclusion };
                        default:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneSphereExclusion };
                    }
                default:
                    switch (Shape)
                    {
                        case ZoneShape.Box:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneBox };
                        case ZoneShape.Cylinder:
                        case ZoneShape.Ring:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneCylinderOrRing };
                        default:
                            return new ModelVisual3D { Content = SharedGeometries.ZoneSphere };
                    }
            }
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }

    public class System : ContentBase
    {
        public string Path { get; set; }
        public List<Connection> Connections { get; set; }

        public System()
        {
            Connections = new List<Connection>();
        }

        protected override ModelVisual3D GetShapeModel()
        {
            return new ModelVisual3D { Content = SharedGeometries.System };
        }

        public override bool IsEmissive()
        {
            return false;
        }
    }

    public class Connection : ContentBase
    {
        public ContentBase From { get; set; }
        public ContentBase To { get; set; }
        public ConnectionType FromType { get; set; }
        public ConnectionType ToType { get; set; }

        protected override ModelVisual3D GetShapeModel()
        {
            Material material;

            if (FromType == ToType)
                //solid brush
                material = MaterialHelper.CreateEmissiveMaterial(GetColor(FromType));
            else
            {
                //gradient brush
                material = MaterialHelper.CreateEmissiveMaterial(new LinearGradientBrush(GetColor(FromType), GetColor(ToType), 90));
            }

            return new ModelVisual3D { Content = SharedGeometries.GetGeometry(SharedMeshes.Surface, material) };
        }

        Color GetColor(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.JumpGate:
                    return SharedMaterials.ConnectionJumpgate;
                case ConnectionType.JumpHole:
                    return SharedMaterials.ConnectionJumphole;
                case ConnectionType.Both:
                    return SharedMaterials.ConnectionBoth;
                default:
                    return SharedMaterials.ConnectionNone;
            }
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }

    public enum ZoneShape
    {
        Box,
        Sphere,
        Cylinder,
        Ellipsoid,
        Ring
    }

    public enum ZoneType
    {
        Zone,
        PathTrade,
        PathPatrol,
        Vignette,
        Exclusion
    }

    public enum ConnectionType
    {
        Both,
        JumpGate,
        JumpHole,
        None
    }
}
