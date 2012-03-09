//----------------------------------------------------------
// WireBaseAndUltimateParent.cs (c) 2007 by Charles Petzold
//----------------------------------------------------------
using System;
using System.Windows;

namespace HelixEngine.Wires
{
    class WireBaseAndUltimateParent
    {
        public WireBase wirebase;
        public Window window;

        public WireBaseAndUltimateParent(WireBase wirebase)
        {
            this.wirebase = wirebase;
        }
    }
}
