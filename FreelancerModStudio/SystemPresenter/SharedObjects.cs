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
        Path,
        None
    }

    public static class SharedMaterials
    {
        public static Material LightSource = MaterialHelper.CreateMaterial(Brushes.Yellow);
        public static Material Sun = MaterialHelper.CreateMaterial(Brushes.Orange);
        public static Material Planet = MaterialHelper.CreateMaterial(Brushes.DeepSkyBlue);
        public static Material Station = MaterialHelper.CreateMaterial(Brushes.Blue);
        public static Material Satellite = MaterialHelper.CreateMaterial(Brushes.BlueViolet);
        public static Material Construct = MaterialHelper.CreateMaterial(Brushes.Fuchsia);
        public static Material Depot = MaterialHelper.CreateMaterial(Brushes.SlateGray);
        public static Material Ship = MaterialHelper.CreateMaterial(Brushes.LawnGreen);
        public static Material WeaponsPlatform = MaterialHelper.CreateMaterial(Brushes.BurlyWood);
        public static Material TradeLane = MaterialHelper.CreateMaterial(Brushes.Cyan);
        public static Material JumpHole = MaterialHelper.CreateMaterial(Brushes.Coral);
        public static Material JumpGate = MaterialHelper.CreateMaterial(Brushes.OrangeRed);
        public static Material DockingRing = MaterialHelper.CreateMaterial(Brushes.DarkGreen);
        public static Material Zone = MaterialHelper.CreateMaterial(Colors.WhiteSmoke, 0.3);
        public static Material Path = MaterialHelper.CreateMaterial(Brushes.WhiteSmoke);
    }

    public static class SharedMeshes
    {
        public static MeshGeometry3D LightSource = GetMesh(new SphereMesh()
        {
            Radius = 0.4,
            Slices = 18,
            Stacks = 9
        });

        public static MeshGeometry3D Sun = GetMesh(new SphereMesh()
        {
            Slices = 18,
            Stacks = 9
        });

        public static MeshGeometry3D Planet = GetMesh(new SphereMesh()
        {
            Slices = 18,
            Stacks = 9
        });

        public static MeshGeometry3D Station = GetMesh(new BoxMesh());

        public static MeshGeometry3D Ship = GetMesh(new TetrahedronMesh());

        public static MeshGeometry3D WeaponsPlatform = GetMesh(new BoxMesh());

        public static MeshGeometry3D TradeLane = GetMesh(new BoxMesh()
        {
            Width = 1,
            Depth = 1.6,
            Height = 0.4,
        });

        public static MeshGeometry3D JumpHole = GetMesh(new CylinderMesh()
        {
            Radius = 0.5,
            Length = 0.25,
        });

        public static MeshGeometry3D JumpGate = GetMesh(new TetrahedronMesh());

        public static MeshGeometry3D DockingRing = GetMesh(new CylinderMesh()
        {
            Radius = 0.5,
            Length = 0.25,
        });

        public static MeshGeometry3D Satellite = GetMesh(new BoxMesh());

        public static MeshGeometry3D Construct = GetMesh(new BoxMesh());

        public static MeshGeometry3D Depot = GetMesh(new BoxMesh());

        public static MeshGeometry3D ZoneBox = GetMesh(new BoxMesh());

        public static MeshGeometry3D ZoneSphere = GetMesh(new SphereMesh()
        {
            Slices = 9,
            Stacks = 5
        });

        public static MeshGeometry3D ZoneCylinder = GetMesh(new BoxMesh()
        {
            //Width = 2,
            //Depth = 2,
        });

        //public static MeshGeometry3D ZoneCylinder = GetMesh(new CylinderMesh()
        //{
        //    Slices = 4,
        //});

        public static MeshGeometry3D Path = GetMesh(new SphereMesh()
        {
            Slices = 18,
            Stacks = 9
        });

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
            GetGeometry(SharedMeshes.LightSource, SharedMaterials.LightSource);

        public static GeometryModel3D Sun =
            GetGeometry(SharedMeshes.Sun, SharedMaterials.Sun);

        public static GeometryModel3D Planet =
            GetGeometry(SharedMeshes.Planet, SharedMaterials.Planet);

        public static GeometryModel3D Station =
            GetGeometry(SharedMeshes.Station, SharedMaterials.Station);

        public static GeometryModel3D Ship =
            GetGeometry(SharedMeshes.Ship, SharedMaterials.Ship);

        public static GeometryModel3D WeaponsPlatform =
            GetGeometry(SharedMeshes.WeaponsPlatform, SharedMaterials.WeaponsPlatform);

        public static GeometryModel3D TradeLane =
            GetGeometry(SharedMeshes.TradeLane, SharedMaterials.TradeLane);

        public static GeometryModel3D JumpHole =
            GetGeometry(SharedMeshes.JumpHole, SharedMaterials.JumpHole);

        public static GeometryModel3D JumpGate =
            GetGeometry(SharedMeshes.JumpGate, SharedMaterials.JumpGate);

        public static GeometryModel3D DockingRing =
            GetGeometry(SharedMeshes.DockingRing, SharedMaterials.DockingRing);

        public static GeometryModel3D Satellite =
            GetGeometry(SharedMeshes.Satellite, SharedMaterials.Satellite);

        public static GeometryModel3D Construct =
            GetGeometry(SharedMeshes.Construct, SharedMaterials.Construct);

        public static GeometryModel3D Depot =
            GetGeometry(SharedMeshes.Depot, SharedMaterials.Depot);

        public static GeometryModel3D ZoneBox =
            GetGeometry(SharedMeshes.ZoneBox, SharedMaterials.Zone);

        public static GeometryModel3D ZoneSphere =
            GetGeometry(SharedMeshes.ZoneSphere, SharedMaterials.Zone);

        public static GeometryModel3D ZoneCylinder =
            GetGeometry(SharedMeshes.ZoneCylinder, SharedMaterials.Zone);

        public static GeometryModel3D Path =
            GetGeometry(SharedMeshes.Path, SharedMaterials.Path);

        static GeometryModel3D GetGeometry(Geometry3D geometry, Material material)
        {
            GeometryModel3D model = new GeometryModel3D(geometry, material);
            model.Freeze();
            return model;
        }
    }
}
