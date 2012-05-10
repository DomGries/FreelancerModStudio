using System;

namespace FreelancerModStudio.Data.UTF
{
    [Flags]
    enum NodeFlags
    {
        Intermediate = 0x10,
        Leaf = 0x80
    }
}