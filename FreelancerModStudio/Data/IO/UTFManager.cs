using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FreelancerModStudio.Data.IO
{
    public class UTFManager
    {
        public string File { get; set; }

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
                int signature = reader.ReadInt32();
                int version = reader.ReadInt32();
                if (signature == 0x20465455 || version == 0x101)
                {
                    // get node chunk info
                    int nodeBlockOffset = reader.ReadInt32();
                    int nodeSize = reader.ReadInt32();

                    int unknown1 = reader.ReadInt32();
                    int header_size = reader.ReadInt32();

                    // get string chunk info
                    int stringBlockOffset = reader.ReadInt32();
                    int stringBlockSize = reader.ReadInt32();

                    int unknown2 = reader.ReadInt32();

                    // get data chunk info
                    int dataBlockOffset = reader.ReadInt32();

                    reader.BaseStream.Position = stringBlockOffset;

                    //read string table
                    StringTable stringTable =
                        new StringTable(Encoding.ASCII.GetString(reader.ReadBytes(stringBlockSize)));

                    info = new UTFNode();
                    ParseNode(reader, stringTable, nodeBlockOffset, 0, dataBlockOffset, info);
                }
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
            int flags = reader.ReadInt32();                     // bit 4 set = intermediate, bit 7 set = leaf
            int zero = reader.ReadInt32();                      // always seems to be zero
            int childOffset = reader.ReadInt32();               // next node in if intermediate, offset to data if leaf
            int allocatedSize = reader.ReadInt32();             // leaf node only, 0 for intermediate
            //int size = reader.ReadInt32();                      // leaf node only, 0 for intermediate
            //int size2 = reader.ReadInt32();                     // leaf node only, 0 for intermediate
            //int u1 = reader.ReadInt32();                        // seems to be timestamps. can be zero
            //int u2 = reader.ReadInt32();
            //int u3 = reader.ReadInt32();

            UTFNode node = new UTFNode();
            if (parent.Name != null)
                node.ParentNode = parent;
            node.Name = stringTable.GetString(nameOffset - 1);

            // Extract data if this is a leaf
            if ((flags & 0xFF) == 0x80)
            {
                //if (size != size2) Compression might be used

                reader.BaseStream.Position = childOffset + dataBlockOffset;
                node.Data = reader.ReadBytes(allocatedSize);
            }

            parent.Nodes.Add(node);

            if (childOffset > 0 && flags == 0x10)
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
