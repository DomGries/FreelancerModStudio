namespace FreelancerModStudio.SystemDesigner.Content
{
    using global::System.Collections.Generic;
    using global::System.Windows.Media.Media3D;

    public class System : ContentBase
    {
        public string Path { get; set; }
        public List<Connection> Connections { get; set; }

        public System()
        {
            this.Connections = new List<Connection>();
        }

        protected override Model3D GetShapeModel()
        {
            return SharedGeometries.System;
        }

        public override Rect3D GetShapeBounds()
        {
            return new Rect3D(-1, -1, -1, 2, 2, 2);
        }

        public override bool IsEmissive()
        {
            return false;
        }
    }
}
