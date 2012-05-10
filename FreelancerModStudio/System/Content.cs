using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;

namespace FreelancerModStudio.SystemPresenter
{
    public class LightSource : ContentBase
    {
        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.LightSource;
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }

    public class SystemObject : ContentBase
    {
        public ContentType Type { get; set; }

        protected override Model3D GetShapeModel()
        {
            switch (Type)
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

        public override bool IsEmissive()
        {
            return false;
        }
    }

    public class Zone : ContentBase
    {
        public ZoneShape Shape { get; set; }
        public ZoneType Type { get; set; }

        protected override Model3D GetShapeModel()
        {
            //special zones
            switch (Type)
            {
                case ZoneType.PathPatrol:
                    return SharedGeometries.ZonePathPatrol;
                case ZoneType.PathTrade:
                    return SharedGeometries.ZonePathTrade;
                case ZoneType.Vignette:
                    return SharedGeometries.ZoneVignette;
                case ZoneType.Exclusion:
                    switch (Shape)
                    {
                        case ZoneShape.Box:
                            return SharedGeometries.ZoneBoxExclusion;
                        case ZoneShape.Cylinder:
                        case ZoneShape.Ring:
                            return SharedGeometries.ZoneCylinderOrRingExclusion;
                        default:
                            return SharedGeometries.ZoneSphereExclusion;
                    }
                default:
                    switch (Shape)
                    {
                        case ZoneShape.Box:
                            return SharedGeometries.ZoneBox;
                        case ZoneShape.Cylinder:
                        case ZoneShape.Ring:
                            return SharedGeometries.ZoneCylinderOrRing;
                        default:
                            return SharedGeometries.ZoneSphere;
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

        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.System;
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
