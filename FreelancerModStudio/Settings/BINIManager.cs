using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace FreelancerModStudio.Settings
{
    //format: http://nullprogram.com/projects/bini/binitools.html#The-BINI-Format
    public class BINIManager
    {
        public string File { get; set; }
        public bool IsBini { get; private set; }
        public INIBlocks Data { get; set; }

        public BINIManager(string file)
        {
            File = file;
        }

        public void Read()
        {
            Data = new INIBlocks();
            FileStream stream = null;
            BinaryReader binaryReader = null;
            //try
            //{
            stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read);
            binaryReader = new BinaryReader(stream, Encoding.Default);

            //read header
            if (Encoding.Default.GetString(binaryReader.ReadBytes(4)) != "BINI" ||
                binaryReader.ReadInt32() != 1)
            {
                IsBini = false;
                return;
            }

            IsBini = true;

            int stringTablePosition = binaryReader.ReadInt32();

            //read data
            while (stream.Position < stringTablePosition && stream.Position < stream.Length)
            {
                //read section
                int sectionStringPosition = binaryReader.ReadInt16() + stringTablePosition;
                int sectionEntriesCount = binaryReader.ReadInt16();

                string sectionName = ReadString(ref binaryReader, sectionStringPosition);
                KeyValuePair<string, INIOptions> block = new KeyValuePair<string, INIOptions>(sectionName.ToLower(), new INIOptions());

                //read each entry
                for (int i = 0; i < sectionEntriesCount; i++)
                {
                    //read entry
                    int entryStringPosition = binaryReader.ReadInt16() + stringTablePosition;
                    int entryValuesCount = binaryReader.ReadByte();
                    string entryName = ReadString(ref binaryReader, entryStringPosition);

                    List<string> options = new List<string>();
                    //read each value
                    for (int j = 0; j < entryValuesCount; j++)
                    {
                        //read value
                        int valueTypeIndex = binaryReader.ReadByte();
                        ValueType[] valueTypes = (ValueType[])Enum.GetValues(typeof(ValueType));
                        ValueType valueType = (ValueType)valueTypes.GetValue(valueTypeIndex - 1);

                        string entryValue = null;
                        if (valueType == ValueType.Integer)
                            entryValue = binaryReader.ReadInt32().ToString();
                        else if (valueType == ValueType.Float)
                            entryValue = binaryReader.ReadSingle().ToString();
                        else //string
                        {
                            int valueStringPosition = binaryReader.ReadInt32() + stringTablePosition;
                            entryValue = ReadString(ref binaryReader, valueStringPosition);
                        }
                        options.Add(entryValue.ToLower());
                    }
                    block.Value.Add2(new KeyValuePair<string, List<string>>(entryName.ToLower(), options));
                }
                Data.Add(block);
            }
            try { }
            catch (Exception ex)
            {
                throw ex;
            }

            if (binaryReader != null)
                binaryReader.Close();
        }

        private string ReadString(ref BinaryReader reader, long position)
        {
            long readerPosition = reader.BaseStream.Position;

            //goto string table
            reader.BaseStream.Position = position;

            //read string
            StringBuilder stringBuilder = new StringBuilder();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                char newValue = reader.ReadChar();
                if (newValue == '\0')
                    break;

                stringBuilder.Append(newValue);
            }

            //go back to data
            reader.BaseStream.Position = readerPosition;

            return stringBuilder.ToString();
        }
    }

    enum ValueType
    {
        Integer = 0,
        Float = 1,
        ValueStringPosition = 2
    }
}
