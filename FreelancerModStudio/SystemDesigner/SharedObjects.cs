using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixEngine;
using HelixEngine.Meshes;

namespace FreelancerModStudio.SystemDesigner
{
    using System;
    using System.Drawing;
    using System.Xml.Serialization;

    using FreelancerModStudio.Data;

    using Brushes = System.Windows.Media.Brushes;
    using Color = System.Windows.Media.Color;

    public static class SharedMaterials
    {
        public static readonly Material Zone = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(30, 30, 30));
        public static readonly Material ZoneExclusion = MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(30, 15, 0));
        public static readonly Material CmpModel = MaterialHelper.CreateMaterial(Brushes.SlateGray);

        public static readonly Color ConnectionJumpHole = Colors.Orange;
        public static readonly Color ConnectionJumpGate = Colors.SlateGray;
        public static readonly Color ConnectionJumpGateAndHole = Colors.Snow;
        public static readonly Color ConnectionNone = Colors.Black;

        public static readonly Color Selection = Colors.Yellow;
        public static readonly Color TrackedLine = Colors.LightYellow;
        public static readonly Color ManipulatorX = Colors.OrangeRed;
        public static readonly Color ManipulatorY = Colors.LawnGreen;
        public static readonly Color ManipulatorZ = Color.FromRgb(0, 120, 255);
    }

    public static class SharedMeshes
    {
        public static readonly MeshGeometry3D Sphere = GetMesh(new SphereMesh
            {
                Slices = 18,
                Stacks = 14
            });

        public static readonly MeshGeometry3D Box = GetMesh(new BoxMesh());

        public static readonly MeshGeometry3D Cylinder = GetMesh(new CylinderMesh
            {
                Slices = 18
            });

        public static readonly MeshGeometry3D Pyramid = GetMesh(new PyramidMesh());

        public static readonly MeshGeometry3D Octahedron = GetMesh(new OctahedronMesh());

        public static readonly MeshGeometry3D Surface = GetMesh(new SurfaceMesh());

        public static readonly MeshGeometry3D SurfaceCylinder = GetMesh(new SurfaceMesh
            {
                Width = 2
            });

        private static MeshGeometry3D GetMesh(MeshGeneratorBase mesh)
        {
            MeshGeometry3D geometry = mesh.Geometry;
            geometry.Freeze();
            return geometry;
        }
    }

    public static class SharedGeometries
    {
        public static GeometryModel3D System;

        public static GeometryModel3D LightSource;

        public static GeometryModel3D Construct;

        public static GeometryModel3D Depot;

        public static GeometryModel3D DockingRing;

        public static GeometryModel3D JumpGate;

        public static GeometryModel3D JumpHole;

        public static GeometryModel3D Planet;

        public static GeometryModel3D Satellite;

        public static GeometryModel3D Ship;

        public static GeometryModel3D Station;

        public static GeometryModel3D Sun;

        public static GeometryModel3D TradeLane;

        public static GeometryModel3D WeaponsPlatform;

        public static GeometryModel3D ZoneSphereOrEllipsoid;

        public static GeometryModel3D ZoneBox;

        public static GeometryModel3D ZoneCylinderOrRing;

        public static GeometryModel3D ZoneSphereOrEllipsoidExclusion;

        public static GeometryModel3D ZoneBoxExclusion;

        public static GeometryModel3D ZoneCylinderExclusion;

        public static GeometryModel3D ZoneVignette;

        public static GeometryModel3D ZonePath;

        public static GeometryModel3D ZonePathTrade;

        public static GeometryModel3D ZonePathTradeLane;

        public static GeometryModel3D GetGeometry(Geometry3D geometry, Material material)
        {
            GeometryModel3D model = new GeometryModel3D(geometry, material);
            model.Freeze();
            return model;
        }

        public static void LoadColors(Settings.ColorBox color)
        {
            System = GetGeometry(SharedMeshes.Sphere, MaterialHelper.CreateMaterial(Brushes.LightYellow));

            LightSource = GetGeometry(
                SharedMeshes.Octahedron,
                MaterialHelper.CreateEmissiveMaterial(Color.FromRgb(120, 120, 0)));

            Construct = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Construct)));

            Depot = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Depot)));

            DockingRing = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.DockingRing)));

            JumpGate = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.JumpGate)));

            JumpHole = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.JumpHole)));

            Planet = GetGeometry(SharedMeshes.Sphere, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Planet)));

            Satellite = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Satellite)));

            Ship = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Ship)));

            Station = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Station)));

            Sun = GetGeometry(SharedMeshes.Sphere, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Sun)));

            TradeLane = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.Tradelane)));

            WeaponsPlatform = GetGeometry(SharedMeshes.Pyramid, MaterialHelper.CreateMaterial(new SolidColorBrush(color.WeaponsPlatform)));

            ZoneSphereOrEllipsoid = GetGeometry(SharedMeshes.Sphere, SharedMaterials.Zone);

            ZoneBox = GetGeometry(SharedMeshes.Box, SharedMaterials.Zone);

            ZoneCylinderOrRing = GetGeometry(SharedMeshes.Cylinder, SharedMaterials.Zone);

            ZoneSphereOrEllipsoidExclusion = GetGeometry(SharedMeshes.Sphere, SharedMaterials.ZoneExclusion);

            ZoneBoxExclusion = GetGeometry(SharedMeshes.Box, SharedMaterials.ZoneExclusion);

            ZoneCylinderExclusion = GetGeometry(SharedMeshes.Cylinder, SharedMaterials.ZoneExclusion);

            ZoneVignette = GetGeometry(
                SharedMeshes.Sphere,
                MaterialHelper.CreateEmissiveMaterial(color.ZoneVignette));

            ZonePath = GetGeometry(SharedMeshes.SurfaceCylinder, SharedMaterials.Zone);

            ZonePathTrade = GetGeometry(
                SharedMeshes.SurfaceCylinder,
                MaterialHelper.CreateEmissiveMaterial(color.ZonePathTrade));

            ZonePathTradeLane = GetGeometry(
                SharedMeshes.Surface,
                MaterialHelper.CreateEmissiveMaterial(color.ZonePathTradeLane));
        }
    }

    public class XmlColor
    {
        private System.Drawing.Color color_ = System.Drawing.Color.Black;

        public XmlColor()
        {
        }

        public XmlColor(Color c) => this.color_ = System.Drawing.Color.FromArgb(c.R, c.G, c.B);

        public Color ToColor() => new Color
                {
                    A = this.color_.A, R = this.color_.R, G = this.color_.G, B = this.color_.B
                };

        public void FromColor(System.Drawing.Color c) => this.color_ = c;

        public static implicit operator Color(XmlColor x) => x.ToColor();

        public static implicit operator XmlColor(Color c) => new XmlColor(c);

        [XmlAttribute]
        public string Web
        {
            get => ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(this.color_.R, this.color_.G, this.color_.B));
            set
            {
                try
                {
                    this.color_ = this.Alpha == 0xFF ? ColorTranslator.FromHtml(value) : System.Drawing.Color.FromArgb(this.Alpha, ColorTranslator.FromHtml(value));
                }
                catch (Exception)
                {
                    this.color_ = System.Drawing.Color.Black;
                }
            }
        }

        [XmlAttribute]
        public byte Alpha
        {
            get => this.color_.A;
            set
            {
                if (value != this.color_.A) // avoid hammering named color if no alpha change
                    this.color_ = System.Drawing.Color.FromArgb(value, this.color_);
            }
        }

        public bool ShouldSerializeAlpha() { return this.Alpha < 0xFF; }
    }
}
