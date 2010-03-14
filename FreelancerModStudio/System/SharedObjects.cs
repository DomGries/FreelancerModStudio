using System.Windows.Media.Media3D;
using System.Windows.Media;
using HelixEngine;
using HelixEngine.Meshes;

namespace FreelancerModStudio.SystemPresenter
{
    public enum ContentType
    {
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
        System,
        None
    }

    public static class SharedMaterials
    {
        public static Material LightSource = MaterialHelper.CreateEmissiveMaterial(Brushes.Yellow);
        public static Material Sun = MaterialHelper.CreateEmissiveMaterial(Brushes.Orange);
        public static Material Planet = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(0, 60, 120));
        public static Material Station = MaterialHelper.CreateEmissiveMaterial(Brushes.OrangeRed);
        public static Material Satellite = MaterialHelper.CreateEmissiveMaterial(Brushes.BlueViolet);
        public static Material Construct = MaterialHelper.CreateEmissiveMaterial(Brushes.Fuchsia);
        public static Material Depot = MaterialHelper.CreateEmissiveMaterial(Brushes.SlateGray);
        public static Material Ship = MaterialHelper.CreateEmissiveMaterial(Brushes.Gold);
        public static Material WeaponsPlatform = MaterialHelper.CreateEmissiveMaterial(Brushes.BurlyWood);
        public static Material TradeLane = MaterialHelper.CreateEmissiveMaterial(Brushes.Cyan);
        public static Material JumpHole = MaterialHelper.CreateEmissiveMaterial(Brushes.DarkGreen);
        public static Material JumpGate = MaterialHelper.CreateEmissiveMaterial(Brushes.DarkGreen);
        public static Material DockingRing = MaterialHelper.CreateEmissiveMaterial(Brushes.Firebrick);
        public static Material Zone = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(20, 20, 20));
        public static Material ZoneVignette = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(0, 20, 10));
        public static Material ZoneExclusion = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(20, 10, 0));

        public static Material System = MaterialHelper.CreateEmissiveMaterial(Brushes.Silver);
        public static Color ConnectionJumphole = Colors.OrangeRed;
        public static Color ConnectionJumpgate = Colors.Yellow;
        public static Color ConnectionBoth = Colors.DodgerBlue;
        public static Color ConnectionNone = Colors.Black;
    }

    public static class SharedMeshes
    {
        public static MeshGeometry3D Sphere = GetMesh(new SphereMesh()
        {
            Slices = 18,
            Stacks = 9
        });

        public static MeshGeometry3D SphereLightSource = GetMesh(new SphereMesh()
        {
            Radius = 0.4,
            Slices = 18,
            Stacks = 9
        });

        public static MeshGeometry3D BoxTradeLane = GetMesh(new BoxMesh()
        {
            Height = 0.3,
        });

        public static MeshGeometry3D CylinderRing = GetMesh(new CylinderMesh()
        {
            Radius = 0.5,
            Length = 0.25,
        });

        public static MeshGeometry3D Cylinder = GetMesh(new BoxMesh()
        {
            Depth = 2,
            Width = 2
        });

        public static MeshGeometry3D Pyramid = GetMesh(new PyramidMesh());
        public static MeshGeometry3D Box = GetMesh(new BoxMesh());

        static MeshGeometry3D GetMesh(MeshGeneratorBase mesh)
        {
            MeshGeometry3D geometry = mesh.Geometry;
            geometry.Freeze();
            return geometry;
        }
    }

    public static class SharedGeometries
    {
        public static GeometryModel3D LightSource =
            GetGeometry(SharedMeshes.SphereLightSource, SharedMaterials.LightSource);

        public static GeometryModel3D Sun =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Sun);

        public static GeometryModel3D Planet =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Planet);

        public static GeometryModel3D Station =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Station);

        public static GeometryModel3D Ship =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.Ship);

        public static GeometryModel3D WeaponsPlatform =
            GetGeometry(SharedMeshes.Box, SharedMaterials.WeaponsPlatform);

        public static GeometryModel3D TradeLane =
            GetGeometry(SharedMeshes.BoxTradeLane, SharedMaterials.TradeLane);

        public static GeometryModel3D JumpHole =
            GetGeometry(SharedMeshes.CylinderRing, SharedMaterials.JumpHole);

        public static GeometryModel3D JumpGate =
            GetGeometry(SharedMeshes.Pyramid, SharedMaterials.JumpGate);

        public static GeometryModel3D DockingRing =
            GetGeometry(SharedMeshes.CylinderRing, SharedMaterials.DockingRing);

        public static GeometryModel3D Satellite =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Satellite);

        public static GeometryModel3D Construct =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Construct);

        public static GeometryModel3D Depot =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Depot);

        public static GeometryModel3D ZoneBox =
            GetGeometry(SharedMeshes.Box, SharedMaterials.Zone);

        public static GeometryModel3D ZoneSphere =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.Zone);

        public static GeometryModel3D ZoneCylinder =
            GetGeometry(SharedMeshes.Cylinder, SharedMaterials.Zone);

        public static GeometryModel3D ZoneVignette =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.ZoneVignette);

        public static GeometryModel3D ZoneExclusionSphere =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.ZoneExclusion);

        public static GeometryModel3D ZoneExclusionBox =
            GetGeometry(SharedMeshes.Box, SharedMaterials.ZoneExclusion);

        public static GeometryModel3D System =
            GetGeometry(SharedMeshes.Sphere, SharedMaterials.System);

        public static GeometryModel3D GetGeometry(Geometry3D geometry, Material material)
        {
            GeometryModel3D model = new GeometryModel3D(geometry, material);
            model.Freeze();
            return model;
        }
    }
}
