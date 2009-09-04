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
                int i = 0;
                foreach (KeyValuePair<string, List<INIOptions>> block in data)
                {
                    for (int j = 0; j < block.Value.Count; j++)
                    {
                        if (i > 0)
                            streamWriter.Write(Environment.NewLine + Environment.NewLine);

                        streamWriter.WriteLine("[" + block.Key + "]");

                        //write each option
                        int k = 0;
                        foreach (KeyValuePair<string, List<INIOption>> option in block.Value[j])
                        {
                            for (int h = 0; h < option.Value.Count; h++)
                            {
                                string key;
                                if (option.Value[h].Parent == null)
                                    key = option.Key;
                                else
                                    key = option.Value[h].Parent;

                                streamWriter.Write(key + " = " + option.Value[h].Value);

                                if (h < option.Value.Count - 1)
                                    streamWriter.Write(Environment.NewLine);
                            }

                            if (k < block.Value[j].Count - 1)
                                streamWriter.Write(Environment.NewLine);

                            k++;
                        }
                    }
                    i++;
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
   
    public class INIBlocks : Dictionary<string, List<INIOptions>>
    {
        public INIBlocks() : base(StringComparer.OrdinalIgnoreCase) { }

        public new void Add(string key, List<INIOptions> values)
        {
            if (this.ContainsKey(key))
            {
                //add value to existing option
                foreach (INIOptions options in values)
                    this[key].Add(options);
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

    public class INIOptions : Dictionary<string, List<INIOption>>
    {
        public INIOptions() : base(StringComparer.OrdinalIgnoreCase) { }

        public new void Add(string key, List<INIOption> values)
        {
            if (this.ContainsKey(key))
            {
                //add value to existing option
                foreach (INIOption option in values)
                    this[key].Add(option);
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
