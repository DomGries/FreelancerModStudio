namespace FreelancerModStudio.SystemDesigner.Content
{
    using global::System.Windows.Media;
    using global::System.Windows.Media.Media3D;
    using HelixEngine;

    public class Connection : ContentBase
    {
        public ContentBase From { get; set; }
        public ContentBase To { get; set; }
        public ConnectionType FromType { get; set; }
        public ConnectionType ToType { get; set; }

        protected override Model3D GetShapeModel()
        {
            Material material;

            if (this.FromType == this.ToType)
            {
                // solid brush
                material = MaterialHelper.CreateEmissiveMaterial(GetColor(this.FromType));
            }
            else
            {
                // gradient brush
                material = MaterialHelper.CreateEmissiveMaterial(new LinearGradientBrush(GetColor(this.FromType), GetColor(this.ToType), 90));
            }

            return SharedGeometries.GetGeometry(SharedMeshes.Surface, material);
        }

        private static Color GetColor(ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.JumpGate:
                    return SharedMaterials.ConnectionJumpGate;
                case ConnectionType.JumpHole:
                    return SharedMaterials.ConnectionJumpHole;
                case ConnectionType.JumpGateAndHole:
                    return SharedMaterials.ConnectionJumpGateAndHole;
                default:
                    return SharedMaterials.ConnectionNone;
            }
        }

        public override Rect3D GetShapeBounds()
        {
            return new Rect3D(-1, -0.5, -1, 2, 1, 2);
        }

        public override bool IsEmissive()
        {
            return true;
        }
    }
}
