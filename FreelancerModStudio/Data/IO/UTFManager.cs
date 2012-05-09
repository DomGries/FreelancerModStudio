using System.Collections.Generic;
using System.IO;
using System.Text;
using FreelancerModStudio.Data.UTF;

namespace FreelancerModStudio.Data.IO
{
    public class UTFManager
    {
        public string File { get; set; }

        const string FILE_TYPE = "UTF ";
        const int FILE_VERSION = 257;

        public UTFManager(string file)
        {
            File = file;
        }

        public UTFNode Read()
        {
            UTFNode info = null;

            using (var stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                if (stream.Length < 4 + 4 ||
                    Encoding.ASCII.GetString(reader.ReadBytes(4)) != FILE_TYPE ||
                    reader.ReadInt32() != FILE_VERSION)
                {
                    return null;
                }

                // get node info
                int nodeBlockOffset = reader.ReadInt32();
                //int nodeBlockSize = reader.ReadInt32();

                //int unknown1 = reader.ReadInt32();
                //int header_size = reader.ReadInt32();
                reader.BaseStream.Seek(12, SeekOrigin.Current);

                // get string info
                int stringBlockOffset = reader.ReadInt32();
                int stringBlockSize = reader.ReadInt32();

                //int unknown2 = reader.ReadInt32();
                reader.BaseStream.Seek(4, SeekOrigin.Current);

                // get data info
                int dataBlockOffset = reader.ReadInt32();

                reader.BaseStream.Position = stringBlockOffset;

                // read string table
                StringTable stringTable =
                    new StringTable(Encoding.ASCII.GetString(reader.ReadBytes(stringBlockSize)));

                info = new UTFNode();
                ParseNode(reader, stringTable, nodeBlockOffset, 0, dataBlockOffset, info);
            }
            return info;
        }

        private void ParseNode(BinaryReader reader, StringTable stringTable, int nodeBlockStart, int nodeStart, int dataBlockOffset, UTFNode parent)
        {
            int offset = nodeBlockStart + nodeStart;
            reader.BaseStream.Position = offset;

            int nodeOffset = offset;

            int peerOffset = reader.ReadInt32();                // next node on same level
            int nameOffset = reader.ReadInt32();                // string for this node
            NodeFlags flags = (NodeFlags)reader.ReadInt32();

            int zero = reader.ReadInt32();                      // always seems to be zero
            int childOffset = reader.ReadInt32();               // next node in if intermediate, offset to data if leaf
            int allocatedSize = reader.ReadInt32();             // leaf node only, 0 for intermediate
            //int size = reader.ReadInt32();                      // leaf node only, 0 for intermediate
            //int size2 = reader.ReadInt32();                     // leaf node only, 0 for intermediate

            //int timestamp1 = reader.ReadInt32();
            //int timestamp2 = reader.ReadInt32();
            //int timestamp3 = reader.ReadInt32();

            UTFNode node = new UTFNode();
            if (parent.Name != null)
                node.ParentNode = parent;
            node.Name = stringTable.GetString(nameOffset - 1);

            // Extract data if this is a leaf
            if ((flags & NodeFlags.Leaf) == NodeFlags.Leaf)
            {
                //if (size != size2) Compression might be used

                reader.BaseStream.Position = childOffset + dataBlockOffset;
                node.Data = reader.ReadBytes(allocatedSize);
            }

            parent.Nodes.Add(node);

            if (childOffset > 0 && (flags & NodeFlags.Intermediate) == NodeFlags.Intermediate)
                ParseNode(reader, stringTable, nodeBlockStart, childOffset, dataBlockOffset, node);

            if (peerOffset > 0)
                ParseNode(reader, stringTable, nodeBlockStart, peerOffset, dataBlockOffset, parent);
        }
    }

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
                return Nodes[index];

            return null;
        }

        public int IndexOf(string name)
        {
            for (int index = 0; index < Nodes.Count; index++)
            {
                if (Nodes[index].Name.ToLower() == name.ToLower())
                    return index;
            }
            return -1;
        }

        public UTFNode FindNode(string name, bool searchAllChildren)
        {
            foreach (UTFNode node in Nodes)
            {
                if (node.Name.ToLower() == name.ToLower())
                    return node;

                if (searchAllChildren)
                {
                    UTFNode foundNode = node.FindNode(name, true);
                    if (foundNode != null)
                        return foundNode;
                }
            }

            return null;
        }

        public List<UTFNode> FindNodes(string name, bool searchAllChildren)
        {
            List<UTFNode> foundNodes = new List<UTFNode>();

            foreach (var node in Nodes)
            {
                if (searchAllChildren)
                    foundNodes.AddRange(node.FindNodes(name, true));

                if (node.Name.ToLower() == name.ToLower())
                    foundNodes.Add(node);
            }

            return foundNodes;
        }
    }
}
