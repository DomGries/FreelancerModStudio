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
            if (Type == ZoneType.Vignette)
                return SharedGeometries.ZoneVignette;
            else if (Type == ZoneType.Exclusion)
            {
                if (Shape == ZoneShape.Sphere || Shape == ZoneShape.Ellipsoid)
                    return SharedGeometries.ZoneExclusionSphere;
                else
                    return SharedGeometries.ZoneExclusionBox;
            }

            if (Shape == ZoneShape.Box)
                return SharedGeometries.ZoneBox;
            else if (Shape == ZoneShape.Cylinder)
                return SharedGeometries.ZoneCylinder;
            else
                return SharedGeometries.ZoneSphere;
        }

        public override MeshGeometry3D GetMesh()
        {
            if (Shape == ZoneShape.Box || Shape == ZoneShape.Cylinder)
                return SharedMeshes.Box;
            else
                return SharedMeshes.Sphere;
        }
    }

    public class System : ContentBase
    {
        public string Path { get; set; }

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
        public ConnectionType StartType { get; set; }
        public ConnectionType EndType { get; set; }

        protected override GeometryModel3D GetGeometry()
        {
            Material material;

            if (StartType == EndType)
            {
                //solid brush
                if (StartType == ConnectionType.Jumpgate)
                    material = MaterialHelper.CreateEmissiveMaterial(SharedMaterials.ConnectionJumpgate);
                else if (StartType == ConnectionType.Jumphole)
                    material = MaterialHelper.CreateEmissiveMaterial(SharedMaterials.ConnectionJumphole);
                else
                    material = MaterialHelper.CreateEmissiveMaterial(SharedMaterials.ConnectionBoth);
            }
            else
            {
                Color startColor = GetColor(StartType);
                Color endColor = GetColor(EndType);

                material = MaterialHelper.CreateEmissiveMaterial(new LinearGradientBrush(startColor, endColor, 90));
            }

            return SharedGeometries.GetGeometry(SharedMeshes.Box, material);
        }

        Color GetColor(ConnectionType type)
        {
            if (StartType == ConnectionType.Jumpgate)
                return SharedMaterials.ConnectionJumpgate;
            else if (StartType == ConnectionType.Jumphole)
                return SharedMaterials.ConnectionJumphole;
            else
                return SharedMaterials.ConnectionBoth;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Box;
        }
    }

    public enum ZoneShape
    {
        Box,
        Sphere,
        Cylinder,
        Ellipsoid
    }

    public enum ZoneType
    {
        Zone,
        Vignette,
        Exclusion
    }

    public enum ConnectionType
    {
        Both,
        Jumpgate,
        Jumphole
    }
}
