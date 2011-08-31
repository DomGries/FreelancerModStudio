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
            return SharedMeshes.SphereLightSource;
        }
    }

    public class Sun : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Sun;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Sphere;
        }
    }

    public class Planet : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Planet;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Sphere;
        }
    }

    public class Station : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Station;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public class Satellite : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Satellite;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public class Construct : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Construct;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public class Depot : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Depot;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public class Ship : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Ship;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Pyramid;
        }
    }

    public class WeaponsPlatform : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.WeaponsPlatform;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public class TradeLane : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.TradeLane;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.BoxTradeLane;
        }
    }

    public class JumpHole : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.JumpHole;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.CylinderRing;
        }
    }

    public class JumpGate : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.JumpGate;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Pyramid;
        }
    }

    public class DockingRing : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.DockingRing;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.CylinderRing;
        }
    }

    public class Zone : ContentBase
    {
        public ZoneShape Shape { get; set; }
        public ZoneType Type { get; set; }

        protected override GeometryModel3D GetGeometry()
        {
            //special zones
            if (Type == ZoneType.Path)
                return SharedGeometries.ZonePath;
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
            return SharedMeshes.Surface2Sided;
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
        Path,
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
