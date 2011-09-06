using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using HelixEngine;
using System.Windows.Media;
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
                case ContentType.Construct:
                    return SharedMeshes.Box;
                case ContentType.Depot:
                    return SharedMeshes.Box;
                case ContentType.DockingRing:
                    return SharedMeshes.CylinderRing;
                case ContentType.JumpGate:
                    return SharedMeshes.Pyramid;
                case ContentType.JumpHole:
                    return SharedMeshes.CylinderRing;
                case ContentType.Planet:
                    return SharedMeshes.Sphere;
                case ContentType.Satellite:
                    return SharedMeshes.Box;
                case ContentType.Ship:
                    return SharedMeshes.Pyramid;
                case ContentType.Station:
                    return SharedMeshes.Box;
                case ContentType.Sun:
                    return SharedMeshes.Sphere;
                case ContentType.TradeLane:
                    return SharedMeshes.BoxTradeLane;
                case ContentType.WeaponsPlatform:
                    return SharedMeshes.Box;
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
            if (Type == ZoneType.PathPatrol)
                return SharedGeometries.ZonePathPatrol;
            else if (Type == ZoneType.PathTrade)
                return SharedGeometries.ZonePathTrade;
            else if (Type == ZoneType.Vignette)
                return SharedGeometries.ZoneVignette;
            else if (Type == ZoneType.Exclusion)
            {
                if (Shape == ZoneShape.Box)
                    return SharedGeometries.ZoneExclusionBox;
                else if (Shape == ZoneShape.Cylinder)
                    return SharedGeometries.ZoneExclusionCylinder;
                else if (Shape == ZoneShape.Ring)
                    return SharedGeometries.ZoneExclusionRing;
                else
                    return SharedGeometries.ZoneExclusionSphere;
            }

            if (Shape == ZoneShape.Box)
                return SharedGeometries.ZoneBox;
            else if (Shape == ZoneShape.Cylinder)
                return SharedGeometries.ZoneCylinder;
            else if (Shape == ZoneShape.Ring)
                return SharedGeometries.ZoneRing;
            else
                return SharedGeometries.ZoneSphere;
        }

        public override MeshGeometry3D GetMesh()
        {
            if (Shape == ZoneShape.Box)
                return SharedMeshes.Box;
            else if (Shape == ZoneShape.Cylinder)
                return SharedMeshes.Cylinder;
            else
                return SharedMeshes.Sphere;
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
            if (type == ConnectionType.Jumpgate)
                return SharedMaterials.ConnectionJumpgate;
            else if (type == ConnectionType.Jumphole)
                return SharedMaterials.ConnectionJumphole;
            else if (type == ConnectionType.Both)
                return SharedMaterials.ConnectionBoth;
            else
                return SharedMaterials.ConnectionNone;
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
