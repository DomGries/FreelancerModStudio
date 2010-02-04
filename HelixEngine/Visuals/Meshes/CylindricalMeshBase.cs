//----------------------------------------------------
// CylindricalMeshBase.cs (c) 2007 by Charles Petzold
//----------------------------------------------------
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HelixEngine.Meshes
{
    /// <summary>
    ///     Defines properties for cylindrical mesh geometries.
    /// </summary>
    public abstract class CylindricalMeshBase : MeshGeneratorBase
    {
        /// <summary>
        ///     Identifies the Length dependency property.
        /// </summary>
        /// <value>
        ///     The identifier for the Length dependency property.
        /// </value>
        public static readonly DependencyProperty LengthProperty =
            DependencyProperty.Register("Length",
                typeof(double),
                typeof(CylindricalMeshBase),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the length of the cylinder.
        /// </summary>
        /// <value>
        ///     The length of the cylinder is an amount 
        ///     on the positive Y axis. The default is 1.
        /// </value>
        public double Length
        {
            set { SetValue(LengthProperty, value); }
            get { return (double)GetValue(LengthProperty); }
        }

        /// <summary>
        ///     Identifies the Radius dependency property.
        /// </summary>
        /// <value>
        ///     The radius of the cylinder in world units.
        ///     The default is 1. 
        ///     The Radius property can be set to a negative value,
        ///     but in effect the cylinder is turned inside out so that
        ///     the surface of the cylinder is colored with the BackMaterial
        ///     brush rather than the Material brush.
        /// </value>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius",
                typeof(double),
                typeof(CylindricalMeshBase),
                new PropertyMetadata(1.0, PropertyChanged));

        /// <summary>
        ///     Gets or sets the radius of the hollow cylinder.
        /// </summary>
        public double Radius
        {
            set { SetValue(RadiusProperty, value); }
            get { return (double)GetValue(RadiusProperty); }
        }

        /// <summary>
        ///     Identifies the Slices dependency property.
        /// </summary>
        public static readonly DependencyProperty SlicesProperty =
            DependencyProperty.Register("Slices",
                typeof(int),
                typeof(CylindricalMeshBase),
                new PropertyMetadata(36, PropertyChanged),
                ValidateSlices);

        // Validation callback for Slices.
        static bool ValidateSlices(object obj)
        {
            return (int)obj > 2;
        }

        /// <summary>
        ///     Gets or sets the number of divisions around the Y axis
        ///     used to approximate the cylinder.
        /// </summary>
        /// <value>
        ///     The number of divisions around the Y axis. 
        ///     This property must be at least 3. 
        ///     The default value is 36.
        /// </value>
        /// <remarks>
        ///     The Slices property approximates the curvature of the cylinder.
        ///     The number of degrees of each slice is equivalent to 
        ///     360 divided by the Slices value.
        /// </remarks>
        public int Slices
        {
            set { SetValue(SlicesProperty, value); }
            get { return (int)GetValue(SlicesProperty); }
        }

        /// <summary>
        ///     Identifies the Stacks dependency property.
        /// </summary>
        public static readonly DependencyProperty StacksProperty =
            DependencyProperty.Register("Stacks",
                typeof(int),
                typeof(CylindricalMeshBase),
                new PropertyMetadata(1, PropertyChanged),
                ValidateStacks);

        // Validation callback for Stacks.
        static bool ValidateStacks(object obj)
        {
            return (int)obj > 0;
        }

        /// <summary>
        ///     Gets or sets the number of divisions parallel to the XZ
        ///     plane used to build the cylinder.
        /// </summary>
        /// <value>
        ///     The number of divisions parallel to the XZ plane. 
        ///     This property must be at least 1, which is also the 
        ///     default value. 
        /// </value>
        /// <remarks>
        ///     The default value of 1 is appropriate in many cases. 
        ///     However, if PointLight or SpotLight objects are applied to the
        ///     cylinder, or if non-linear transforms are used to deform
        ///     the figure, you should set Stacks to a higher value.
        /// </remarks>
        public int Stacks
        {
            set { SetValue(StacksProperty, value); }
            get { return (int)GetValue(StacksProperty); }
        }
    }
}

