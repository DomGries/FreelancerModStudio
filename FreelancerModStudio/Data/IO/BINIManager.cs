using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.IO
{
    //format: http://nullprogram.com/projects/bini/binitools.html#The-BINI-Format
    public class BINIManager
    {
        public string File { get; set; }
        public bool IsBini { get; private set; }
        public INIBlocks Data { get; private set; }

        public BINIManager(string file)
        {
            File = file;
        }

        public void Read()
        {
            Data = new INIBlocks();
            FileStream stream = null;
            BinaryReader binaryReader = null;
            try
            {
                stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read);
                binaryReader = new BinaryReader(stream, Encoding.Default);

                //read header
                if (Encoding.Default.GetString(binaryReader.ReadBytes(4)) != "BINI" ||
                    binaryReader.ReadInt32() != 1)
                {
                    IsBini = false;
                    binaryReader.Close();
                    return;
                }

                IsBini = true;

                int stringTablePosition = binaryReader.ReadInt32();
                long dataPosition = stream.Position;

                //goto string table
                stream.Position = stringTablePosition;

                //read string table
                StringTable stringTable = new StringTable(Encoding.Default.GetString(binaryReader.ReadBytes((int)(stream.Length - stream.Position))));

                //go back to data
                stream.Position = dataPosition;

                //us culture for american numbers
                System.Globalization.CultureInfo usCulture = new System.Globalization.CultureInfo("en-us");

                //read data
                while (stream.Position < stringTablePosition && stream.Position < stream.Length)
                {
                    //read section
                    int sectionStringPosition = binaryReader.ReadInt16();
                    int sectionEntriesCount = binaryReader.ReadInt16();

                    string sectionName = stringTable.GetString(sectionStringPosition);
                    INIOptions block = new INIOptions();
                    //read each entry
                    for (int i = 0; i < sectionEntriesCount; i++)
                    {
                        //read entry
                        int entryStringPosition = binaryReader.ReadInt16();
                        int entryValuesCount = binaryReader.ReadByte();
                        string entryName = stringTable.GetString(entryStringPosition);

                        //read each value
                        List<string> options = new List<string>();
                        for (int j = 0; j < entryValuesCount; j++)
                        {
                            //read value
                            int valueType = binaryReader.ReadByte();

                            string entryValue = null;
                            if (valueType == 1)
                                entryValue = binaryReader.ReadInt32().ToString("D", usCulture);
                            else if (valueType == 2)
                                entryValue = binaryReader.ReadSingle().ToString("0.000000", usCulture);
                            else //string
                            {
                                int valueStringPosition = binaryReader.ReadInt32();
                                entryValue = stringTable.GetString(valueStringPosition);
                            }
                            options.Add(entryValue);
                        }
                        block.Add(entryName, new INIOption(string.Join(", ", options.ToArray()), i));
                    }
                    Data.Add(sectionName, block);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (binaryReader != null)
                binaryReader.Close();
        }
    }

    public class StringTable
    {
        private SortedList<int, string> Strings = new SortedList<int, string>();

        public StringTable(string content)
        {
            int position = 0;
            foreach (string stringValue in content.Trim('\0').Split('\0'))
            {
                Strings.Add(position, stringValue);
                position += stringValue.Length + 1;
            }
        }

        public string GetString(int position)
        {
            return Strings[position];
        }
    }
}
