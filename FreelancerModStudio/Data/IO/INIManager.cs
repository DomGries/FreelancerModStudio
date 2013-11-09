using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreelancerModStudio.Data.INI;

namespace FreelancerModStudio.Data.IO
{
    public class INIManager
    {
        public string File { get; set; }
        public bool WriteSpaces { get; set; }
        public bool WriteEmptyLine { get; set; }

        public INIManager(string file)
        {
            File = file;
        }

        public List<INIBlock> Read()
        {
            List<INIBlock> data = new List<INIBlock>();

            INIBlock currentBlock = new INIBlock();
            int currentOptionIndex = 0;

            using (FileStream stream = new FileStream(File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream, Encoding.Default))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Trim();

                    //remove comments from data
                    int commentIndex = line.IndexOf(';');
                    if (commentIndex != -1)
                    {
                        line = line.Substring(0, commentIndex).Trim();
                    }

                    if (line.Length > 0)
                    {
                        if (line[0] == '[' && line[line.Length - 1] != ']')
                        {
                            //reset current block if block was commented out
                            if (currentBlock.Name != null)
                            {
                                data.Add(currentBlock);
                            }

                            currentBlock = new INIBlock();
                        }
                        else if (line[0] == '[' && line[line.Length - 1] == ']')
                        {
                            //new block
                            if (currentBlock.Name != null)
                            {
                                data.Add(currentBlock);
                            }

                            string blockName = line.Substring(1, line.Length - 2).Trim();

                            currentBlock = new INIBlock
                                {
                                    Name = blockName,
                                    Options = new INIOptions()
                                };
                            currentOptionIndex = 0;
                        }
                        else if (currentBlock.Name != null)
                        {
                            //new value for block
                            int valueIndex = line.IndexOf('=');
                            if (valueIndex != -1)
                            {
                                //retrieve name and value from data
                                string optionName = line.Substring(0, valueIndex).Trim();
                                string optionValue = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();

                                currentBlock.Options.Add(optionName, new INIOption
                                    {
                                        Value = optionValue,
                                        Index = currentOptionIndex
                                    });
                                ++currentOptionIndex;
                            }
                            else
                            {
                                // entry without value
                                currentBlock.Options.Add(line, new INIOption
                                    {
                                        Value = string.Empty,
                                        Index = currentOptionIndex
                                    });
                                ++currentOptionIndex;
                            }
                        }
                    }
                }
            }

            if (currentBlock.Name != null)
            {
                data.Add(currentBlock);
            }

            return data;
        }

        public void Write(List<INIBlock> data)
        {
            using (StreamWriter streamWriter = new StreamWriter(File, false, Encoding.Default))
            {
                int i = 0;
                foreach (INIBlock block in data)
                {
                    if (i > 0)
                    {
                        streamWriter.WriteLine();
                        if (WriteEmptyLine)
                        {
                            streamWriter.WriteLine();
                        }
                    }

                    streamWriter.WriteLine("[" + block.Name + "]");

                    //write each option
                    int k = 0;
                    foreach (KeyValuePair<string, List<INIOption>> option in block.Options)
                    {
                        for (int h = 0; h < option.Value.Count; ++h)
                        {
                            //write the key
                            streamWriter.Write(option.Value[h].Parent ?? option.Key);

                            // dont write the '=' for entries with no value
                            if (option.Value[h].Value.Length != 0)
                            {
                                if (WriteSpaces)
                                {
                                    streamWriter.Write(" = " + option.Value[h].Value);
                                }
                                else
                                {
                                    streamWriter.Write("=" + option.Value[h].Value);
                                }
                            }

                            if (h < option.Value.Count - 1)
                            {
                                streamWriter.Write(Environment.NewLine);
                            }
                        }

                        if (k < block.Options.Count - 1)
                        {
                            streamWriter.Write(Environment.NewLine);
                        }

                        ++k;
                    }
                    ++i;
                }
            }
        }
    }
}
