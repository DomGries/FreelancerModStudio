using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FreelancerModStudio.Settings
{
    public class INIManager
    {
        public string File { get; set; }

        public INIManager(string file)
        {
            File = file;
        }

        public INIBlocks Read()
        {
            INIBlocks data = new INIBlocks();

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(File, Encoding.Default);
                KeyValuePair<string, INIOptions> currentBlock = new KeyValuePair<string, INIOptions>();
                int currentOptionIndex = 0;

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
                            data.Add(currentBlock.Key, currentBlock.Value);

                        string blockName = line.Substring(1, line.Length - 2).Trim();

                        currentBlock = new KeyValuePair<string, INIOptions>(blockName, new INIOptions());
                        currentOptionIndex = 0;
                    }
                    else if (currentBlock.Key != null) //new value for block
                    {
                        //retrieve name and value from data
                        int valueIndex = line.IndexOf('=');
                        if (valueIndex != -1)
                        {
                            string optionName = line.Substring(0, valueIndex).Trim();
                            string optionValue = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();

                            currentBlock.Value.Add(optionName, new INIOption(optionValue, currentOptionIndex));
                            currentOptionIndex++;
                        }
                    }
                }

                if (currentBlock.Key != null)
                    data.Add(currentBlock.Key, currentBlock.Value);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (streamReader != null)
                streamReader.Close();

            return data;
        }

        public void Write(INIBlocks data)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(File, false, Encoding.Default);

                //write each block
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data.Values[i].Count; j++)
                    {
                        if (i > 0 || j > 0)
                            streamWriter.Write(Environment.NewLine + Environment.NewLine);

                        streamWriter.WriteLine("[" + data.Keys[i] + "]");

                        //write each option
                        for (int k = 0; k < data.Values[i][j].Count; k++)
                        {
                            for (int h = 0; h < data.Values[i][j].Values[k].Count; h++)
                            {
                                string key;
                                if (data.Values[i][j].Values[k][h].Parent == null)
                                    key = data.Values[i][j].Keys[k];
                                else
                                    key = data.Values[i][j].Values[k][h].Parent;

                                streamWriter.Write(key + " = " + data.Values[i][j].Values[k][h].Value);

                                if (k < data.Values[i][j].Count)
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
        public INIBlocks() : base(StringComparer.OrdinalIgnoreCase) { }

        public new void Add(string key, List<INIOptions> values)
        {
            int index = this.IndexOfKey(key);
            if (index != -1)
            {
                //add value to existing option
                foreach (INIOptions options in values)
                    this.Values[index].Add(options);
            }
            else
            {
                //add new option
                base.Add(key, values);
            }
        }

        public void Add(string key, INIOptions values)
        {
            List<INIOptions> options = new List<INIOptions>();
            options.Add(values);
            this.Add(key, options);
        }
    }

    public class INIOptions : SortedList<string, List<INIOption>>
    {
        public INIOptions() : base(StringComparer.OrdinalIgnoreCase) { }

        public new void Add(string key, List<INIOption> values)
        {
            int index = this.IndexOfKey(key);
            if (index != -1)
            {
                //add value to existing option
                foreach (INIOption option in values)
                    this.Values[index].Add(option);
            }
            else
            {
                //add new option
                base.Add(key, values);
            }
        }

        public void Add(string key, INIOption value)
        {
            List<INIOption> options = new List<INIOption>();
            options.Add(value);
            this.Add(key, options);
        }
    }

    public class INIOption
    {
        public string Value;
        public string Parent; //used to save nested options in correct order
        public int Index; //used to load nested options in correct order

        public INIOption(string value)
        {
            Value = value;
        }

        public INIOption(string value, int index)
        {
            Value = value;
            Index = index;
        }

        public INIOption(string value, string parent)
        {
            Value = value;
            Parent = parent;
        }
    }
}
