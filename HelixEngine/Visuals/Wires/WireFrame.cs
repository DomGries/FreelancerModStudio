//------------------------------------------
// WireFrame.cs (c) 2007 by Charles Petzold
//------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Wires
{
    public class WireFrame : WireBase
    {
        /// <summary>
        ///     Identifies the Positions dependency property.
        /// </summary>
        public static readonly DependencyProperty PositionsProperty =
            MeshGeometry3D.PositionsProperty.AddOwner(typeof(WireFrame),
                new PropertyMetadata(PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Point3DCollection Positions
        {
            set { SetValue(PositionsProperty, value); }
            get { return (Point3DCollection)GetValue(PositionsProperty); }
        }

        /// <summary>
        ///     Identfies the Normals dependency property.
        /// </summary>
        public static readonly DependencyProperty NormalsProperty =
            MeshGeometry3D.NormalsProperty.AddOwner(typeof(WireFrame),
                new PropertyMetadata(PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Vector3DCollection Normals
        {
            set { SetValue(NormalsProperty, value); }
            get { return (Vector3DCollection)GetValue(NormalsProperty); }
        }

        /// <summary>
        ///     Identifies the TriangleIndices dependency property.
        /// </summary>
        public static readonly DependencyProperty TriangleIndicesProperty =
            MeshGeometry3D.TriangleIndicesProperty.AddOwner(typeof(WireFrame),
                new PropertyMetadata(PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public Int32Collection TriangleIndices
        {
            set { SetValue(TriangleIndicesProperty, value); }
            get { return (Int32Collection)GetValue(TriangleIndicesProperty); }
        }

        /// <summary>
        ///     Identifies the TextureCoordinates dependency property.
        /// </summary>
        public static readonly DependencyProperty TextureCoordinatesProperty =
            MeshGeometry3D.TextureCoordinatesProperty.AddOwner(typeof(WireFrame),
                new PropertyMetadata(PropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        public PointCollection TextureCoordinates
        {
            set { SetValue(TextureCoordinatesProperty, value); }
            get { return (PointCollection)GetValue(TextureCoordinatesProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="lines"></param>
        protected override void Generate(DependencyPropertyChangedEventArgs args,
                                         Point3DCollection lines)
        {
            Point3DCollection vertices = Positions;
            Int32Collection indices = TriangleIndices;
            lines.Clear();

            if (vertices != null && vertices.Count > 0 &&
                    indices != null && indices.Count > 0)
            {

                // Check that this doesn't overflow !!!!!!
                // -----------------------------------------

                // Special logic if there are no indices !!!!
                // -------------------------------------------

                for (int i = 0; i < indices.Count; i += 3)
                {
                    lines.Add(vertices[indices[i + 0]]);
                    lines.Add(vertices[indices[i + 1]]);

                    lines.Add(vertices[indices[i + 1]]);
                    lines.Add(vertices[indices[i + 2]]);

                    lines.Add(vertices[indices[i + 2]]);
                    lines.Add(vertices[indices[i + 0]]);
                }
            }
        }
    }
}






