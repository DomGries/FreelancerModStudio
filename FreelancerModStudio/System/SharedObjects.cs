using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;
using HelixEngine.Meshes;

namespace FreelancerModStudio.SystemPresenter
{
    public enum ContentType
    {
        None,
        LightSource,
        Sun,
        Planet,
        Station,
        Satellite,
        Construct,
        Depot,
        Ship,
        WeaponsPlatform,
        DockingRing,
        JumpHole,
        JumpGate,
        TradeLane,
        Zone,
        System
    }

    public static class SharedMaterials
    {
        public static readonly Material LightSource = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(120, 120, 0));
        public static readonly Material Sun = MaterialHelper.CreateMaterial(Brushes.Orange);
        public static readonly Material Planet = MaterialHelper.CreateMaterial(Color.FromRgb(0, 60, 120));
        public static readonly Material Station = MaterialHelper.CreateMaterial(Brushes.OrangeRed);
        public static readonly Material Satellite = MaterialHelper.CreateMaterial(Brushes.BlueViolet);
        public static readonly Material Construct = MaterialHelper.CreateMaterial(Brushes.Fuchsia);
        public static readonly Material Depot = MaterialHelper.CreateMaterial(Brushes.SlateGray);
        public static readonly Material Ship = MaterialHelper.CreateMaterial(Brushes.Gold);
        public static readonly Material WeaponsPlatform = MaterialHelper.CreateMaterial(Brushes.BurlyWood);
        public static readonly Material TradeLane = MaterialHelper.CreateMaterial(Brushes.Cyan);
        public static readonly Material JumpHole = MaterialHelper.CreateMaterial(Brushes.Firebrick);
        public static readonly Material JumpGate = MaterialHelper.CreateMaterial(Brushes.Green);
        public static readonly Material DockingRing = MaterialHelper.CreateMaterial(Brushes.DimGray);
        public static readonly Material CmpModel = MaterialHelper.CreateMaterial(Brushes.SlateGray);
        public static readonly Material Zone = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(30, 30, 30));
        public static readonly Material ZonePathTrade = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(10, 15, 30));
        public static readonly Material ZoneVignette = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(0, 30, 15));
        public static readonly Material ZoneExclusion = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(30, 15, 0));
        public static readonly Material System = MaterialHelper.CreateMaterial(Brushes.LightYellow);

        public static readonly Color ConnectionJumphole = Colors.OrangeRed;
        public static readonly Color ConnectionJumpgate = Colors.SlateGray;
        public static readonly Color ConnectionBoth = Colors.Snow;
        public static readonly Color ConnectionNone = Colors.Black;
    }

    public static class SharedMeshes
    {
        public static readonly MeshGeometry3D Sphere = GetMesh(new SphereMesh
        {
            Slices = 18,
            Stacks = 9
        });

        public static readonly MeshGeometry3D Box = GetMesh(new BoxMesh());

        public static readonly MeshGeometry3D BoxTradeLane = GetMesh(new BoxMesh
        {
            Height = 0.3,
        });

        public static readonly MeshGeometry3D Cylinder = GetMesh(new CylinderMesh
        {
            Slices = 18,
        });

        public static readonly MeshGeometry3D Pyramid = GetMesh(new PyramidMesh());

        public static readonly MeshGeometry3D Octahedron = GetMesh(new OctahedronMesh());

        public static readonly MeshGeometry3D Surface = GetMesh(new SurfaceMesh
        {
            Width = 2
        });

        static MeshGeometry3D GetMesh(MeshGeneratorBase mesh)
        {
            var geometry = mesh.Geometry;
            geometry.Freeze();
            return geometry;
        }
    }

    public static class SharedGeometries
    {
        public static readonly GeometryModel3D LightSource =
            GetGeometry(SharedMeshes.Octahedron, SharedMaterials.LightSource);

        public static readonly GeometryModel3D Sun =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Sun);

        public static readonly GeometryModel3D Planet =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Planet);

        public static readonly GeometryModel3D Station =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Station);

        public static readonly GeometryModel3D Ship =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.Ship);

        public static readonly GeometryModel3D WeaponsPlatform =
            GetGeometry(SharedMeshes.Box, SharedMaterials.WeaponsPlatform);

        public static readonly GeometryModel3D TradeLane =
            GetGeometry(SharedMeshes.BoxTradeLane, SharedMaterials.TradeLane);

        public static readonly GeometryModel3D JumpHole =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.JumpHole);

        public static readonly GeometryModel3D JumpGate =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.JumpGate);

        public static readonly GeometryModel3D DockingRing =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.DockingRing);

        public static readonly GeometryModel3D Satellite =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Satellite);

        public static readonly GeometryModel3D Construct =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Construct);

        public static readonly GeometryModel3D Depot =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Depot);

        public static readonly GeometryModel3D ZoneBox =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Zone);

        public static readonly GeometryModel3D ZoneSphere =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Zone);

        public static readonly GeometryModel3D ZoneCylinderOrRing =
            GetGeometry(SharedMeshes.Cylinder, SharedMaterials.Zone);

        public static readonly GeometryModel3D ZonePathPatrol =
            GetGeometry(SharedMeshes.Surface, SharedMaterials.Zone);

        public static readonly GeometryModel3D ZonePathTrade =
            GetGeometry(SharedMeshes.Surface, SharedMaterials.ZonePathTrade);

        public static readonly GeometryModel3D ZoneVignette =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.ZoneVignette);

        public static readonly GeometryModel3D ZoneBoxExclusion =
            GetGeometry(SharedMeshes.Box, SharedMaterials.ZoneExclusion);

        public static readonly GeometryModel3D ZoneSphereExclusion =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.ZoneExclusion);

        public static readonly GeometryModel3D ZoneCylinderOrRingExclusion =
            GetGeometry(SharedMeshes.Cylinder, SharedMaterials.ZoneExclusion);

        public static readonly GeometryModel3D System =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.System);

        public static GeometryModel3D GetGeometry(Geometry3D geometry, Material material)
        {
            return GetGeometry(geometry, material, Transform3D.Identity);
        }

        public static GeometryModel3D GetGeometry(Geometry3D geometry, Material material, Transform3D transform)
        {
            var model = new GeometryModel3D(geometry, material) { Transform = transform };
            model.Freeze();
            return model;
        }
    }
}
