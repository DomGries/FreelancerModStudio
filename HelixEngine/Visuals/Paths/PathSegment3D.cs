//----------------------------------------------
// PathSegment3D.cs (c) 2007 by Charles Petzold
//----------------------------------------------
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace HelixEngine.Paths
{
    public abstract class PathSegment3D : Animatable
    {
        // IsStroked property.
        // -------------------
        public static readonly DependencyProperty IsStrokedProperty =
            DependencyProperty.Register("IsStroked", typeof(bool),
            typeof(PathSegment3D),
            new PropertyMetadata(true));

        public bool IsStroked
        {
            set { SetValue(IsStrokedProperty, value); }
            get { return (bool)GetValue(IsStrokedProperty); }
        }

        // IsSmoothJoin property.
        // ----------------------
        public static readonly DependencyProperty IsSmoothJoinProperty =
            DependencyProperty.Register("IsSmoothJoin", typeof(bool),
            typeof(PathSegment3D),
            new PropertyMetadata(false));

        public bool IsSmoothJoin
        {
            set { SetValue(IsSmoothJoinProperty, value); }
            get { return (bool)GetValue(IsSmoothJoinProperty); }
        }
    }
}
