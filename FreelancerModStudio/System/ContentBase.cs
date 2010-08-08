using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HelixEngine;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;

namespace FreelancerModStudio.SystemPresenter
{
    public abstract class ContentBase : ITableRow<int>
    {
        public int ID { get; set; }

        public ModelVisual3D Model { get; set; }
        public Vector3D Position { get; set; }
        public Rotation3D Rotation { get; set; }
        public Vector3D Scale { get; set; }

        public string Title { get; set; }
        public bool Visibility { get; set; }

        public void SetDisplay(Vector3D position, Rotation3D rotation, Vector3D scale)
        {
            if (Model != null)
            {
                ContentAnimation animation = new ContentAnimation()
                {
                    OldPosition = Position,
                    OldRotation = Rotation,
                    OldScale = Scale,
                    NewPosition = position,
                    NewRotation = rotation,
                    NewScale = scale,
                };
                Animator.Animate(Model, animation);
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
            SetDisplay(Position, Rotation, Scale);
        }
    }
}
