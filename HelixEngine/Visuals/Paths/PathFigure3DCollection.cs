//-------------------------------------------------------
// PathFigure3DCollection.cs (c) 2007 by Charles Petzold
//-------------------------------------------------------
using System;
using System.Windows;

namespace HelixEngine.Paths
{
    //--------------------------------------------------------------------------------------
    // TODO: [System.ComponentModel.TypeConverter(typeof(PathFigure3DCollectionConverter))] 
    //--------------------------------------------------------------------------------------
    public class PathFigure3DCollection : FreezableCollection<PathFigure3D>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = "";

            foreach (PathFigure3D fig in this)
                str += fig.ToString();

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new PathFigure3DCollection();
        }
    }
}
