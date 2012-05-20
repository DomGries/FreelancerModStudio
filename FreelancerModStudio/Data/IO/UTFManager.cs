using System.IO;
using System.Text;
using FreelancerModStudio.Data.UTF;

namespace FreelancerModStudio.Data.IO
{
    public class UTFManager
    {
        public string File { get; set; }

        const string FILE_TYPE = "UTF ";
        const int FILE_VERSION = 0x101;

        public UTFManager(string file)
        {
            File = file;
        }

        public UTFNode Read()
        {
            using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (stream.Length < ByteLen.FILE_TAG + ByteLen.INT ||
                    Encoding.ASCII.GetString(reader.ReadBytes(ByteLen.FILE_TAG)) != FILE_TYPE ||
                    reader.ReadInt32() != FILE_VERSION)
                {
                    return null;
                }

                // get node info
                int nodeBlockOffset = reader.ReadInt32();
                //int nodeBlockSize = reader.ReadInt32();

                //int unknown1 = reader.ReadInt32();
                //int header_size = reader.ReadInt32();
                stream.Position += ByteLen.INT * 3;

                // get string info
                int stringBlockOffset = reader.ReadInt32();
                int stringBlockSize = reader.ReadInt32();

                //int unknown2 = reader.ReadInt32();
                stream.Position += ByteLen.INT;

                // get data info
                int dataBlockOffset = reader.ReadInt32();

                // goto string block
                stream.Position = stringBlockOffset;

                // read string block
                byte[] buffer = new byte[stringBlockSize];
                reader.Read(buffer, 0, stringBlockSize);
                string stringBlock = Encoding.ASCII.GetString(buffer);

                UTFNode info = new UTFNode();
                ParseNode(reader, stringBlock, nodeBlockOffset, 0, dataBlockOffset, info);
                return info;
            }
        }

        static void ParseNode(BinaryReader reader, string stringBlock, int nodeBlockStart, int nodeStart, int dataBlockOffset, UTFNode parent)
        {
            int offset = nodeBlockStart + nodeStart;
            reader.BaseStream.Position = offset;

            int peerOffset = reader.ReadInt32(); // next node on same level
            int nameOffset = reader.ReadInt32(); // string for this node
            NodeFlags flags = (NodeFlags)reader.ReadInt32();

            //int zero = reader.ReadInt32();
            reader.BaseStream.Position += ByteLen.INT;

            UTFNode node = new UTFNode
                {
                    Name = stringBlock.Substring(nameOffset, stringBlock.IndexOf('\0', nameOffset) - nameOffset)
                };

            if ((flags & NodeFlags.Intermediate) == NodeFlags.Intermediate)
            {
                int childOffset = reader.ReadInt32();
                if (childOffset > 0)
                {
                    ParseNode(reader, stringBlock, nodeBlockStart, childOffset, dataBlockOffset, node);
                }

                //int allocatedSize = reader.ReadInt32();
                //int size = reader.ReadInt32();
                //int size2 = reader.ReadInt32();
                //int timestamp1 = reader.ReadInt32();
                //int timestamp2 = reader.ReadInt32();
                //int timestamp3 = reader.ReadInt32();
            }
            else if ((flags & NodeFlags.Leaf) == NodeFlags.Leaf)
            {
                int dataOffset = reader.ReadInt32();

                //int allocatedSize = reader.ReadInt32();
                reader.BaseStream.Position += ByteLen.INT;

                int size = reader.ReadInt32();
                //int size2 = reader.ReadInt32();
                //int timestamp1 = reader.ReadInt32();
                //int timestamp2 = reader.ReadInt32();
                //int timestamp3 = reader.ReadInt32();

                reader.BaseStream.Position = dataBlockOffset + dataOffset;
                node.Data = reader.ReadBytes(size);

            }

            parent.Nodes.Add(node);

            if (peerOffset > 0)
            {
                ParseNode(reader, stringBlock, nodeBlockStart, peerOffset, dataBlockOffset, parent);
            }
        }
    }
}
