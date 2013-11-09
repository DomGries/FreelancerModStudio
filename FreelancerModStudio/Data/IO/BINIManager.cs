using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FreelancerModStudio.Data.INI;

namespace FreelancerModStudio.Data.IO
{
    public class BINIManager
    {
        public string File { get; set; }
        public List<INIBlock> Data { get; set; }

        const string FILE_TYPE = "BINI";
        const int FILE_VERSION = 0x1;

        public BINIManager(string file)
        {
            File = file;
        }

        public bool Read()
        {
            Data = new List<INIBlock>();

            using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(stream, Encoding.Default))
            {
                long streamLength = stream.Length;

                if (streamLength < ByteLen.FILE_TAG + ByteLen.INT ||
                    Encoding.ASCII.GetString(reader.ReadBytes(ByteLen.FILE_TAG)) != FILE_TYPE ||
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

                    string sectionName = stringBlock.Substring(sectionNameOffset, stringBlock.IndexOf('\0', sectionNameOffset) - sectionNameOffset);
                    INIOptions block = new INIOptions();
                    // read each entry
                    for (int i = 0; i < sectionEntriesCount; ++i)
                    {
                        // read entry
                        int entryNameOffset = reader.ReadInt16();
                        int entryValuesCount = reader.ReadByte();
                        string entryName = stringBlock.Substring(entryNameOffset, stringBlock.IndexOf('\0', entryNameOffset) - entryNameOffset);

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
                                        entryValue = stringBlock.Substring(valueNameOffset, stringBlock.IndexOf('\0', valueNameOffset) - valueNameOffset);
                                    }
                                    break;
                            }
                            options.Add(entryValue);
                        }
                        block.Add(entryName, new INIOption
                            {
                                Value = string.Join(", ", options.ToArray()),
                                Index = i
                            });
                    }
                    Data.Add(new INIBlock
                        {
                            Name = sectionName,
                            Options = block
                        });
                }
            }
            return true;
        }
    }
}
