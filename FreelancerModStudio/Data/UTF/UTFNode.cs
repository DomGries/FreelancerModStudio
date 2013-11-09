using System.Collections.Generic;

namespace FreelancerModStudio.Data.UTF
{
    public class UTFNode
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public List<UTFNode> Nodes { get; set; }

        public UTFNode()
        {
            Nodes = new List<UTFNode>();
        }
    }
}
