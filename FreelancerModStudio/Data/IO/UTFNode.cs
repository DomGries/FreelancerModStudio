using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.IO
{
    public class UTFNode
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public UTFNode ParentNode { get; set; }
        public List<UTFNode> Nodes { get; set; }

        public UTFNode()
        {
            Nodes = new List<UTFNode>();
        }

        public UTFNode GetNode(string name)
        {
            int index = IndexOf(name);
            if (index != -1)
            {
                return Nodes[index];
            }
            return null;
        }

        public int IndexOf(string name)
        {
            for (int index = 0; index < Nodes.Count; ++index)
            {
                if (Nodes[index].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }
            return -1;
        }

        public UTFNode FindNode(string name, bool searchChildren)
        {
            foreach (UTFNode node in Nodes)
            {
                if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }

                if (searchChildren)
                {
                    UTFNode foundNode = node.FindNode(name, true);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }

            return null;
        }

        public List<UTFNode> FindNodes(string name, bool searchChildren)
        {
            List<UTFNode> foundNodes = new List<UTFNode>();

            foreach (UTFNode node in Nodes)
            {
                if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    foundNodes.Add(node);
                }

                if (searchChildren)
                {
                    foundNodes.AddRange(node.FindNodes(name, true));
                }
            }

            return foundNodes;
        }
    }
}
