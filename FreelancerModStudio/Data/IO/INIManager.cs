namespace FreelancerModStudio.Data.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using FreelancerModStudio.Data.INI;

    public class IniManager
    {
        private const char TOKEN_COMMENT = ';';

        private const char TOKEN_BLOCK_START = '[';

        private const char TOKEN_BLOCK_END = ']';

        private const char TOKEN_MAPPING_VALUE = '=';

        private const string MAPPING_VALUE_WRITE_FORMAT = " = ";

        public string File { get; set; }

        public bool WriteSpaces { get; set; }
        public bool WriteEmptyLine { get; set; }
        public bool ReadWriteComments { get; set; }

        public IniManager(string file)
        {
            this.File = file;
        }

        public List<IniBlock> Read()
        {
            List<IniBlock> data = new List<IniBlock>();

            IniBlock currentBlock = new IniBlock();
            int currentOptionIndex = 0;
            bool currentBlockCommentedOut = false;

            using (FileStream stream = new FileStream(this.File, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader streamReader = new StreamReader(stream, Encoding.Default))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    int commentIndex;
                    string newComment = null;

                    // check for commented out block
                    if (this.ReadWriteComments && line.Length > 2 && line[0] == TOKEN_BLOCK_START && line[1] == TOKEN_COMMENT)
                    {
                        line = line.Substring(2);

                        // add additional comments
                        commentIndex = line.IndexOf(TOKEN_COMMENT);
                        if (commentIndex != -1)
                        {
                            // add comment
                            newComment = line.Substring(commentIndex + 1);

                            // remove comment from data
                            line = line.Substring(0, commentIndex).TrimEnd();
                        }

                        if (line[line.Length - 1] == TOKEN_BLOCK_END)
                        {
                            // add current block
                            if (currentBlock.Name != null)
                            {
                                data.Add(currentBlock);
                            }

                            // new block
                            string blockName = line.Substring(0, line.Length - 1).Trim();
                            currentBlock = CreateBlock(blockName, new IniOptions(), currentBlock.Name == null ? currentBlock.Comments : null);

                            currentBlockCommentedOut = true;

                            AddComment(currentBlock, newComment);
                            continue;
                        }
                    }

                    // handle comments
                    commentIndex = line.IndexOf(TOKEN_COMMENT);
                    if (commentIndex != -1)
                    {
                        // add comment
                        if (this.ReadWriteComments)
                        {
                            newComment = line.Substring(commentIndex + 1);
                        }

                        // remove comment from data
                        line = line.Substring(0, commentIndex).TrimEnd();
                    }
                    else
                    {
                        line = line.Trim();
                    }

                    if (line.Length == 0)
                    {
                        AddComment(currentBlock, newComment);
                        continue;
                    }

                    if (line[0] == TOKEN_BLOCK_START)
                    {
                        // add current block
                        if (currentBlock.Name != null)
                        {
                            data.Add(currentBlock);
                        }

                        if (line[line.Length - 1] == TOKEN_BLOCK_END)
                        {
                            // new block
                            string blockName = line.Substring(1, line.Length - 2).Trim();
                            currentBlock = CreateBlock(blockName, new IniOptions(), currentBlock.Name == null ? currentBlock.Comments : null);
                        }
                        else
                        {
                            // reset block
                            currentBlock = CreateBlock(null, null, null);
                        }

                        currentOptionIndex = 0;
                        currentBlockCommentedOut = false;
                    }
                    else if (currentBlockCommentedOut)
                    {
                        // add comment
                        AddComment(currentBlock, line);
                    }
                    else if (currentBlock.Name != null)
                    {
                        // new value for block
                        int valueIndex = line.IndexOf(TOKEN_MAPPING_VALUE);
                        if (valueIndex != -1)
                        {
                            // retrieve name and value from data
                            string optionName = line.Substring(0, valueIndex).Trim();
                            string optionValue = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();

                            currentBlock.Options.Add(optionName, new IniOption
                                {
                                    Value = optionValue,
                                    Index = currentOptionIndex
                                });
                            ++currentOptionIndex;
                        }
                        else
                        {
                            // entry without value
                            currentBlock.Options.Add(line, new IniOption
                                {
                                    Value = string.Empty,
                                    Index = currentOptionIndex
                                });
                            ++currentOptionIndex;
                        }
                    }

                    // add comment to current block
                    AddComment(currentBlock, newComment);
                }
            }

            // add final block
            if (currentBlock.Name != null)
            {
                data.Add(currentBlock);
            }

            return data;
        }

        private static void AddComment(IniBlock block, string value)
        {
            if (value == null)
            {
                return;
            }

            if (block.Comments != null)
            {
                block.Comments += Environment.NewLine;
            }

            block.Comments += value;
        }

        private static IniBlock CreateBlock(string name, IniOptions options, string comments)
        {
            return new IniBlock
                {
                    Name = name,
                    Options = options,
                    Comments = comments,
                };
        }

        public void Write(List<IniBlock> data)
        {
            using (StreamWriter streamWriter = new StreamWriter(this.File, false, Encoding.Default))
            {
                int i = 0;
                foreach (IniBlock block in data)
                {
                    if (i > 0)
                    {
                        streamWriter.WriteLine();
                        if (this.WriteEmptyLine)
                        {
                            streamWriter.WriteLine();
                        }
                    }

                    streamWriter.Write(TOKEN_BLOCK_START);
                    streamWriter.Write(block.Name);
                    streamWriter.Write(TOKEN_BLOCK_END);

                    if (block.Options.Count > 0)
                    {
                        streamWriter.WriteLine();

                        // write each option
                        int k = 0;
                        foreach (KeyValuePair<string, List<IniOption>> option in block.Options)
                        {
                            for (int h = 0; h < option.Value.Count; ++h)
                            {
                                // write the key
                                streamWriter.Write(option.Value[h].Parent ?? option.Key);

                                // dont write the '=' for entries with no value
                                if (option.Value[h].Value.Length != 0)
                                {
                                    if (this.WriteSpaces)
                                    {
                                        streamWriter.Write(MAPPING_VALUE_WRITE_FORMAT);
                                    }
                                    else
                                    {
                                        streamWriter.Write(TOKEN_MAPPING_VALUE);
                                    }

                                    streamWriter.Write(option.Value[h].Value);
                                }

                                if (h < option.Value.Count - 1)
                                {
                                    streamWriter.WriteLine();
                                }
                            }

                            if (k < block.Options.Count - 1)
                            {
                                streamWriter.WriteLine();
                            }

                            ++k;
                        }
                    }

                    // write comments
                    if (this.ReadWriteComments && block.Comments != null)
                    {
                        streamWriter.WriteLine();
                        streamWriter.Write(TOKEN_COMMENT);
                        streamWriter.Write(block.Comments.Replace(Environment.NewLine, Environment.NewLine + TOKEN_COMMENT));
                    }

                    ++i;
                }
            }
        }
    }
}
