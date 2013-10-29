using System;

namespace HelixEngine
{
    [Flags]
    public enum CameraFlyMode
    {
        None = 0,
        Forward = 0x1,
        Backward = 0x2,
        Left = 0x4,
        Right = 0x8,
        Up = 0x10,
        Down = 0x20,
    }
}