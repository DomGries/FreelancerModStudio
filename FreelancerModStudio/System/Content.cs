using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;

namespace FreelancerModStudio.SystemPresenter
{
    public class LightSource : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.LightSource;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Octahedron;
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }

    public class SystemObject : ContentBase
    {
        public ContentType Type { get; set; }

        protected override GeometryModel3D GetGeometry()
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

        public override MeshGeometry3D GetMesh()
        {
            switch (Type)
            {
                case ContentType.Planet:
                case ContentType.Sun:
                    return SharedMeshes.Sphere;
                case ContentType.Construct:
                case ContentType.Depot:
                case ContentType.Satellite:
                case ContentType.Station:
                case ContentType.WeaponsPlatform:
                    return SharedMeshes.Box;
                case ContentType.TradeLane:
                    return SharedMeshes.BoxTradeLane;
                case ContentType.DockingRing:
                case ContentType.JumpHole:
                    return SharedMeshes.CylinderRing;
                case ContentType.JumpGate:
                case ContentType.Ship:
                    return SharedMeshes.Pyramid;
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

        protected override GeometryModel3D GetGeometry()
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
                            return SharedGeometries.ZoneExclusionBox;
                        case ZoneShape.Cylinder:
                            return SharedGeometries.ZoneExclusionCylinder;
                        case ZoneShape.Ring:
                            return SharedGeometries.ZoneExclusionRing;
                        default:
                            return SharedGeometries.ZoneExclusionSphere;
                    }
                default:
                    switch (Shape)
                    {
                        case ZoneShape.Box:
                            return SharedGeometries.ZoneBox;
                        case ZoneShape.Cylinder:
                            return SharedGeometries.ZoneCylinder;
                        case ZoneShape.Ring:
                            return SharedGeometries.ZoneRing;
                        default:
                            return SharedGeometries.ZoneSphere;
                    }
            }
        }

        public override MeshGeometry3D GetMesh()
        {
            switch (Shape)
            {
                case ZoneShape.Box:
                    return SharedMeshes.Box;
                case ZoneShape.Cylinder:
                    return SharedMeshes.Cylinder;
                default:
                    return SharedMeshes.Sphere;
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

        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.System;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Sphere;
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

        protected override GeometryModel3D GetGeometry()
        {
            Material material;

            if (FromType == ToType)
                //solid brush
                material = MaterialHelper.CreateEmissiveMaterial(GetColor(FromType));
            else
            {
                //gradient brush
                Color startColor = GetColor(FromType);
                Color endColor = GetColor(ToType);

                material = MaterialHelper.CreateEmissiveMaterial(new LinearGradientBrush(startColor, endColor, 90));
            }

            return SharedGeometries.GetGeometry(GetMesh(), material);
        }

        Color GetColor(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.Jumpgate:
                    return SharedMaterials.ConnectionJumpgate;
                case ConnectionType.Jumphole:
                    return SharedMaterials.ConnectionJumphole;
                case ConnectionType.Both:
                    return SharedMaterials.ConnectionBoth;
                default:
                    return SharedMaterials.ConnectionNone;
            }
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Surface;
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
        Jumpgate,
        Jumphole,
        None
    }
}
