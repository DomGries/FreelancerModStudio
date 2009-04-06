using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FreelancerModStudio.Settings
{
    public class INIData
    {
        public static List<INIBlock> Read(string file)
        {
            List<INIBlock> data = new List<INIBlock>();

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(file, Encoding.Default);
                INIBlock currentBlock = null;

                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim();

                    //remove comments from data
                    int commentIndex = line.IndexOf(';');
                    if (commentIndex != -1)
                        line = line.Substring(0, commentIndex + 1).Trim();

                    if (line.Length > 0 && line[0] == '[' && line[line.Length - 1] == ']') //new block
                    {
                        if (currentBlock != null)
                            data.Add(currentBlock);

                        currentBlock = new INIBlock(line.Substring(1, line.Length - 2));
                    }
                    else if (currentBlock != null) //new value for block
                    {
                        //retrieve name and value from data
                        int valueIndex = line.IndexOf('=');
                        if (valueIndex != -1)
                        {
                            string name = line.Substring(0, valueIndex).Trim();
                            string value = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();
                            currentBlock.Values.Add(new INIOption(name, value));
                        }
                    }
                }

                if (currentBlock != null)
                    data.Add(currentBlock);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (streamReader != null)
                streamReader.Close();

            return data;
        }

        public static void Write(string file, List<INIBlock> data)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(file, false, Encoding.Default);

                //write each block
                for (int i = 0; i < data.Count; i++)
                {
                    if (i > 0 && i < data.Count - 1)
                        streamWriter.Write(Environment.NewLine + Environment.NewLine);

                    streamWriter.WriteLine("[" + data[i].Name + "]");

                    //write each option
                    for (int j = 0; j < data[i].Values.Count; j++)
                    {
                        streamWriter.Write(data[i].Values[j].Name + " = " + data[i].Values[j].Value);

                        if (j < data[i].Values.Count - 1)
                            streamWriter.Write(Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (streamWriter != null)
                streamWriter.Close();
        }
    }

    public class INIBlock
    {
        public string Name;
        public List<INIOption> Values = new List<INIOption>();

        public INIBlock(string name)
        {
            this.Name = name;
        }
    }

    public class INIOption
    {
        public string Name;
        public string Value;

        public INIOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
