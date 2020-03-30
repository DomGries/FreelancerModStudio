namespace FreelancerModStudio.Data.IO
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    using FreelancerModStudio.Data.INI;

    public class BiniManager
    {
        public string File { get; set; }
        public List<IniBlock> Data { get; set; }

        private const string FILE_TYPE = "BINI";

        private const int FILE_VERSION = 0x1;

        public BiniManager(string file)
        {
            this.File = file;
        }

        public bool Read()
        {
            this.Data = new List<IniBlock>();

            using (FileStream stream = new FileStream(this.File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default))
            {
                long streamLength = stream.Length;

                if (streamLength < ByteLen.FileTag + ByteLen.Int ||
                    Encoding.ASCII.GetString(reader.ReadBytes(ByteLen.FileTag)) != FILE_TYPE ||
                    reader.ReadInt32() != FILE_VERSION)
                {
                    // return false if it is not a bini file
                    return false;
                }

                int stringBlockOffset = reader.ReadInt32();
                long dataPosition = stream.Position;

                // goto string block
                stream.Position = stringBlockOffset;

                // read string block
                byte[] buffer = new byte[streamLength - stringBlockOffset];
                reader.Read(buffer, 0, buffer.Length);
                string stringBlock = Encoding.ASCII.GetString(buffer);

                // go back to data
                stream.Position = dataPosition;

                // read data
                while (stream.Position < stringBlockOffset && stream.Position < streamLength)
                {
                    // read section
                    int sectionNameOffset = reader.ReadInt16();
                    int sectionEntriesCount = reader.ReadInt16();

                    string sectionName = stringBlock.Substring(
                        sectionNameOffset,
                        stringBlock.IndexOf('\0', sectionNameOffset) - sectionNameOffset);
                    IniOptions block = new IniOptions();

                    // read each entry
                    for (int i = 0; i < sectionEntriesCount; ++i)
                    {
                        // read entry
                        int entryNameOffset = reader.ReadInt16();
                        int entryValuesCount = reader.ReadByte();
                        string entryName = stringBlock.Substring(
                            entryNameOffset,
                            stringBlock.IndexOf('\0', entryNameOffset) - entryNameOffset);

                        // read each value
                        List<string> options = new List<string>();
                        for (int j = 0; j < entryValuesCount; ++j)
                        {
                            // read value
                            int valueType = reader.ReadByte();

                            string entryValue;
                            switch (valueType)
                            {
                                case 1:
                                    // read int
                                    entryValue = reader.ReadInt32().ToString("D", CultureInfo.InvariantCulture);
                                    break;
                                case 2:
                                    // read float
                                    entryValue = reader.ReadSingle().ToString("0.000000", CultureInfo.InvariantCulture);
                                    break;
                                default:
                                    {
                                        // read string
                                        int valueNameOffset = reader.ReadInt32();
                                        entryValue = stringBlock.Substring(
                                            valueNameOffset,
                                            stringBlock.IndexOf('\0', valueNameOffset) - valueNameOffset);
                                    }

                                    break;
                            }

                            options.Add(entryValue);
                        }

                        block.Add(entryName, new IniOption { Value = string.Join(", ", options.ToArray()), Index = i });
                    }

                    this.Data.Add(new IniBlock { Name = sectionName, Options = block });
                }
            }

            return true;
        }
    }
}
