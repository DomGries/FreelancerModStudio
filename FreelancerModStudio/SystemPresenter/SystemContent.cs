using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixEngine;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Data;

namespace FreelancerModStudio.SystemPresenter
{
    public abstract class ContentBase : IComparable<ContentBase>, ITableRow<int>
    {
        public int ID { get; set; }
        public TableBlock Block { get; set; }
        public ModelVisual3D Model { get; set; }
        public bool Visibility { get; set; }

        public Vector3D Position { get; set; }
        public Rotation3D Rotation { get; set; }
        public Vector3D Scale { get; set; }

        public void SetDisplay(Vector3D position, Rotation3D rotation, Vector3D scale)
        {
            SetDisplay(position, rotation, scale, false);
        }

        private void SetDisplay(Vector3D position, Rotation3D rotation, Vector3D scale, bool always)
        {
            if (Model != null)
            {
                //if (this is Zone && ((Zone)this).Shape != ZoneShape.Sphere)
                //{
                    ContentAnimator.SetScale(Model, Scale, scale, position, always);
                    ContentAnimator.SetRotation(Model, Rotation, rotation, position, always);
                //}
                //else
                //{
                //    ContentAnimator.SetRotation(Model, Rotation, rotation, position, always);
                //    ContentAnimator.SetScale(Model, Scale, scale, position, always);
                //}
                ContentAnimator.SetPosition(Model, Position, position, always);
            }

            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public ContentBase()
        {
            Position = new Vector3D(0, 0, 0);
            Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0);
            Scale = new Vector3D(1, 1, 1);

            Visibility = true;
        }

        protected abstract GeometryModel3D GetGeometry();
        public abstract MeshGeometry3D GetMesh();

        public void LoadModel()
        {
            Model = new ModelVisual3D() { Content = GetGeometry() };

            SetDisplay(Position, Rotation, Scale, true);
        }

        public int CompareTo(ContentBase other)
        {
            //sort by object type, scale
            int objectTypeComparison = this.Block.ObjectType.CompareTo(other.Block.ObjectType);
            if (objectTypeComparison == 0)
                return this.Scale.CompareTo(other.Scale);

            return objectTypeComparison;
        }
    }

    public class LightSource : ContentBase
    {
        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.LightSource;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.LightSource;
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
            return SharedMeshes.Sun;
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
            return SharedMeshes.Planet;
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
            return SharedMeshes.Station;
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
            return SharedMeshes.Satellite;
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
            return SharedMeshes.Construct;
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
            return SharedMeshes.Depot;
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
            return SharedMeshes.Ship;
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
            return SharedMeshes.WeaponsPlatform;
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
            return SharedMeshes.TradeLane;
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
            return SharedMeshes.JumpHole;
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
            return SharedMeshes.JumpGate;
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
            return SharedMeshes.DockingRing;
        }
    }

    public class Zone : ContentBase
    {
        public ZoneShape Shape { get; set; }

        protected override GeometryModel3D GetGeometry()
        {
            if (Shape == ZoneShape.Box)
                return SharedGeometries.ZoneBox;
            else if (Shape == ZoneShape.Cylinder)
                return SharedGeometries.ZoneCylinder;
            else
                return SharedGeometries.ZoneSphere;
        }

        public override MeshGeometry3D GetMesh()
        {
            if (Shape == ZoneShape.Box)
                return SharedMeshes.ZoneBox;
            else if (Shape == ZoneShape.Cylinder)
                return SharedMeshes.ZoneCylinder;
            else
                return SharedMeshes.ZoneSphere;
        }
    }

    public class Path : ContentBase
    {
        public PathType Type { get; set; }

        protected override GeometryModel3D GetGeometry()
        {
            return SharedGeometries.Path;
        }

        public override MeshGeometry3D GetMesh()
        {
            return SharedMeshes.Path;
        }
    }

    public enum PathType
    {
        Friendly,
        Hostile,
        Neutral
    }

    public enum ZoneShape
    {
        Box,
        Sphere,
        Ellipsoid,
        Cylinder
    }
}
