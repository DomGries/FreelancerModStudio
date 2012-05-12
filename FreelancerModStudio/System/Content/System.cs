using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FreelancerModStudio.SystemPresenter.Content
{
    public class System : ContentBase
    {
        public string Path { get; set; }
        public List<Connection> Connections { get; set; }

        public System()
        {
            Connections = new List<Connection>();
        }

        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.System;
        }

        public override Vector3D GetBaseScale()
        {
            return new Vector3D(1, 1, 1);
        }

        public override bool IsEmissive()
        {
            return false;
        }
    }
}
