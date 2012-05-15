using System;

namespace FreelancerModStudio.Data.UTF
{
    [Flags]
    internal enum NodeFlags
    {
        Intermediate = 0x10,
        Leaf = 0x80
    }
}
