//--------------------------------------------------------
// PathSegment3DCollection.cs (c) 2007 by Charles Petzold
//--------------------------------------------------------
using System;
using System.Windows;

namespace HelixEngine.Paths
{
    /// <summary>
    ///     Represents a collection of PathSegment3D objects that can be 
    ///     individually accessed by index. 
    /// </summary>
    public class PathSegment3DCollection : FreezableCollection<PathSegment3D>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";

            foreach (PathSegment3D seg in this)
                str += seg.ToString();

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new PathSegment3DCollection();
        }
    }
}
