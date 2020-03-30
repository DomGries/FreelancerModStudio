namespace FreelancerModStudio.Data.UTF
{
    using System.Collections.Generic;

    public class UtfNode
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public List<UtfNode> Nodes { get; set; }

        public UtfNode()
        {
            this.Nodes = new List<UtfNode>();
        }
    }
}
