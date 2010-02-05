using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using HelixEngine;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio.SystemPresenter
{
    public abstract class ContentBase
    {
        static Vector3D DefaultPosition = new Vector3D(0, 0, 0);
        static Rotation3D DefaultRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 0), 0);
        static Vector3D DefaultScale = new Vector3D(1, 1, 1);

        public EditorINIBlock Block { get; set; }
        public ModelVisual3D Model { get; set; }
        public bool Visibility { get; set; }

        private Vector3D position;
        public Vector3D Position
        {
            get
            {
                return position;
            }
            set
            {
                if (Model != null)
                    ContentAnimator.AnimatePosition(Model, position, value);

                position = value;

                //if (Model != null)
                //    ContentAnimator.AddTransformation(Model, new TranslateTransform3D(value));
            }
        }

        private Rotation3D rotation;
        public Rotation3D Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (Model != null)
                    ContentAnimator.AnimateRotation(Model, rotation, value, Position);

                rotation = value;

                //if (Model != null)
                //    ContentAnimator.AddTransformation(Model, new RotateTransform3D(value, Position.ToPoint3D()));
            }
        }

        private Vector3D scale;
        public Vector3D Scale
        {
            get
            {
                return scale;
            }
            set
            {
                if (Model != null)
                    ContentAnimator.AnimateScale(Model, scale, value, Position);

                scale = value;

                //if (Model != null)
                //    ContentAnimator.AddTransformation(Model, new ScaleTransform3D(value, Position.ToPoint3D()));
            }
        }

        public ContentBase()
        {
            position = DefaultPosition;
            rotation = DefaultRotation;
            scale = DefaultScale;

            Visibility = true;
        }

        protected abstract GeometryModel3D GetGeometry();
        public abstract MeshGeometry3D GetMesh();

        public void LoadModel()
        {
            Model = new ModelVisual3D() { Content = GetGeometry() };

            if (Position != DefaultPosition)
                ContentAnimator.AddTransformation(Model, new TranslateTransform3D(Position));

            if (this is Zone && ((Zone)this).Shape != ZoneShape.Sphere)
            {
                if (Scale != DefaultScale)
                    ContentAnimator.AddTransformation(Model, new ScaleTransform3D(Scale, Position.ToPoint3D()));
                if (Rotation != DefaultRotation)
                    ContentAnimator.AddTransformation(Model, new RotateTransform3D(Rotation, Position.ToPoint3D()));
            }
            else
            {
                if (Rotation != DefaultRotation)
                    ContentAnimator.AddTransformation(Model, new RotateTransform3D(Rotation, Position.ToPoint3D()));
                if (Scale != DefaultScale)
                    ContentAnimator.AddTransformation(Model, new ScaleTransform3D(Scale, Position.ToPoint3D()));
            }
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
