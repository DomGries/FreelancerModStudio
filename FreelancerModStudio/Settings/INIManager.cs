using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FreelancerModStudio.Settings
{
    public class INIManager
    {
        public static INIBlocks Read(string file, bool caseSensitive)
        {
            INIBlocks data = new INIBlocks();

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(file, Encoding.Default);
                KeyValuePair<string, INIOptions> currentBlock = new KeyValuePair<string, INIOptions>();

                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim();

                    //remove comments from data
                    int commentIndex = line.IndexOf(';');
                    if (commentIndex != -1)
                        line = line.Substring(0, commentIndex + 1).Trim();

                    if (line.Length > 0 && line[0] == '[' && line[line.Length - 1] == ']') //new block
                    {
                        if (currentBlock.Key != null)
                            data.Add(currentBlock);

                        string blockName = line.Substring(1, line.Length - 2).Trim();
                        //if (!caseSensitive)
                            blockName = blockName.ToLower();

                        currentBlock = new KeyValuePair<string, INIOptions>(blockName, new INIOptions());
                    }
                    else if (currentBlock.Key != null) //new value for block
                    {
                        //retrieve name and value from data
                        int valueIndex = line.IndexOf('=');
                        if (valueIndex != -1)
                        {
                            string optionName = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();
                            if (!caseSensitive)
                                optionName = optionName.ToLower();

                            KeyValuePair<string, string> option = new KeyValuePair<string, string>(
                                line.Substring(0, valueIndex).Trim(), optionName);

                            currentBlock.Value.Add(option);
                        }
                    }
                }

                if (currentBlock.Key != null)
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

        public static void Write(string file, INIBlocks data)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(file, false, Encoding.Default);

                //write each block
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data.Values[i].Count; j++)
                    {
                        if (i > 0)
                            streamWriter.Write(Environment.NewLine + Environment.NewLine);

                        streamWriter.WriteLine("[" + data.Keys[i] + "]");

                        //write each option
                        for (int k = 0; k < data.Values[i][j].Count; k++)
                        {
                            for (int h = 0; h < data.Values[i][j].Values[k].Count; h++)
                            {
                                streamWriter.Write(data.Values[i][j].Keys[k] + " = " + data.Values[i][j].Values[k][h]);

                                if (k < data.Values[i][j].Count - 1)
                                    streamWriter.Write(Environment.NewLine);
                            }
                        }
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

    public class INIBlocks : SortedList<string, List<INIOptions>>
    {
        public void Add(KeyValuePair<string, INIOptions> keyValuePair)
        {
            int index = this.IndexOfKey(keyValuePair.Key);
            if (index != -1)
                //add options to existing block
                this.Values[index].Add(keyValuePair.Value);
            else
            {
                //add new block
                List<INIOptions> options = new List<INIOptions>();
                options.Add(keyValuePair.Value);
                this.Add(keyValuePair.Key, options);
            }
        }
    }

    public class INIOptions : SortedList<string, List<string>>
    {
        public void Add2(KeyValuePair<string, List<string>> keyValuePair)
        {
            int index = this.IndexOfKey(keyValuePair.Key);
            if (index != -1)
            {
                //add value to existing option
                foreach(string option in keyValuePair.Value)
                    this.Values[index].Add(option);
            }
            else
            {
                //add new option
                this.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void Add(KeyValuePair<string, string> keyValuePair)
        {
            int index = this.IndexOfKey(keyValuePair.Key);
            if (index != -1)
                //add value to existing option
                this.Values[index].Add(keyValuePair.Value);
            else
            {
                //add new option
                List<string> options = new List<string>();
                options.Add(keyValuePair.Value);
                this.Add(keyValuePair.Key, options);
            }
        }
    }
}
