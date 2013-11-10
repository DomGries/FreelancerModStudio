using System;
using System.Collections.Generic;
using System.Diagnostics;
using FreelancerModStudio.Data.INI;

namespace FreelancerModStudio.Data.IO
{
    public class FileManager
    {
        public string File { get; set; }

        public bool IsBINI { get; set; }

        public bool WriteSpaces { get; set; }
        public bool WriteEmptyLine { get; set; }
        public bool ReadWriteComments { get; set; }

        public FileManager(string file, bool isBINI)
        {
            IsBINI = isBINI;
            File = file;
        }

        public FileManager(string file)
        {
            File = file;
        }

        public EditorINIData Read(FileEncoding encoding, int templateFileIndex)
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif
            List<INIBlock> iniData;
            //read basic file structure
            if (encoding == FileEncoding.Automatic || encoding == FileEncoding.BINI)
            {
                IsBINI = true;

                BINIManager biniManager = new BINIManager(File);
                if (biniManager.Read())
                {
                    iniData = biniManager.Data;
                }
                else
                {
                    if (encoding == FileEncoding.Automatic)
                    {
                        iniData = ReadINI();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                iniData = ReadINI();
            }
#if DEBUG
            st.Stop();
            Debug.WriteLine("load data: " + st.ElapsedMilliseconds + "ms");
#endif

            return GetEditorData(iniData, templateFileIndex);
        }

        static EditorINIData GetEditorData(List<INIBlock> iniData, int templateFileIndex)
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif

            EditorINIData editorData = new EditorINIData(templateFileIndex);
            Template.File templateFile = Helper.Template.Data.Files[templateFileIndex];

            //loop each ini block
            foreach (INIBlock iniBlock in iniData)
            {
                Template.Block templateBlock;
                int templateBlockIndex;

                //get template block based on ini block name
                if (templateFile.Blocks.TryGetValue(iniBlock.Name, out templateBlock, out templateBlockIndex))
                {
                    EditorINIBlock editorBlock = new EditorINIBlock(templateBlock.Name, templateBlockIndex)
                        {
                            Comments = iniBlock.Comments
                        };

                    //loop each template option
                    for (int j = 0; j < templateBlock.Options.Count; ++j)
                    {
                        //handle nested options
                        Template.Option childTemplateOption = null;
                        if (j < templateBlock.Options.Count - 1 && templateBlock.Options[j + 1].Parent != null)
                        {
                            //next option is child of current option
                            childTemplateOption = templateBlock.Options[j + 1];
                        }

                        Template.Option templateOption = templateBlock.Options[j];
                        EditorINIOption editorOption = new EditorINIOption(templateOption.Name, j);

                        //get ini options based on all possible template option names
                        List<INIOption> iniOptions;
                        iniBlock.Options.TryGetValue(templateOption.Name, out iniOptions);

                        //search for options with alternative names
                        if (templateOption.RenameFrom != null)
                        {
                            string[] namesFromRename = templateOption.RenameFrom.Split(new[] { ',' });
                            for (int nameIndex = 0; nameIndex < namesFromRename.Length; ++nameIndex)
                            {
                                List<INIOption> alternativeOptions;
                                if (iniBlock.Options.TryGetValue(namesFromRename[nameIndex], out alternativeOptions))
                                {
                                    iniOptions.AddRange(alternativeOptions);

                                    //stop searching after we found the first option group with the correct name
                                    break;
                                }
                            }
                        }

                        if (iniOptions != null)
                        {
                            //h is used to start again at last child option in order to provide better performance
                            int h = 0;

                            if (templateOption.Multiple)
                            {
                                //loop each ini option
                                for (int k = 0; k < iniOptions.Count; ++k)
                                {
                                    List<object> editorChildOptions = null;
                                    List<INIOption> iniChildOptions;

                                    //get ini options of child based on child's template option name
                                    if (childTemplateOption != null && iniBlock.Options.TryGetValue(childTemplateOption.Name, out iniChildOptions))
                                    {
                                        editorOption.ChildTemplateIndex = j + 1;
                                        editorOption.ChildName = childTemplateOption.Name;
                                        editorChildOptions = new List<object>();

                                        //loop each ini option of child
                                        for (; h < iniChildOptions.Count; ++h)
                                        {
                                            INIOption childOption = iniChildOptions[h];
                                            if (k < iniOptions.Count - 1 && childOption.Index > iniOptions[k + 1].Index)
                                            {
                                                break;
                                            }

                                            if (childOption.Index > iniOptions[k].Index)
                                            {
                                                editorChildOptions.Add(childOption.Value);
                                            }
                                        }
                                    }

                                    //add entry of multiple option
                                    editorOption.Values.Add(new EditorINIEntry(iniOptions[k].Value, editorChildOptions));
                                }
                            }
                            else //single option
                            {
                                if (iniOptions.Count > 0)
                                {
                                    if (iniOptions[0].Value.Length > 0)
                                    {
                                        //just add the last option (Freelancer like) if aviable to prevent multiple options which should be single
                                        editorOption.Values.Add(new EditorINIEntry(iniOptions[iniOptions.Count - 1].Value));
                                    }
                                    else
                                    {
                                        //use '=' for options which dont have values and are simply defined when using the option key followed by a colon equal
                                        editorOption.Values.Add(new EditorINIEntry("="));
                                    }
                                }
                            }

                            //add option
                            editorBlock.Options.Add(editorOption);
                        }
                        else
                        {
                            //add empty option based on template
                            editorBlock.Options.Add(new EditorINIOption(templateOption.Name, j));
                        }

                        //set index of main option (value displayed in table view)
                        if (templateBlock.Identifier != null && templateBlock.Identifier.Equals(editorBlock.Options[editorBlock.Options.Count - 1].Name, StringComparison.OrdinalIgnoreCase))
                        {
                            editorBlock.MainOptionIndex = editorBlock.Options.Count - 1;
                        }

                        //ignore next option because we already added it as children to the current option
                        if (childTemplateOption != null)
                        {
                            ++j;
                        }
                    }

                    //add block
                    editorData.Blocks.Add(editorBlock);
                }
            }
#if DEBUG
            st.Stop();
            Debug.WriteLine("typecast data: " + st.ElapsedMilliseconds + "ms");
#endif

            return editorData;
        }

        List<INIBlock> ReadINI()
        {
            IsBINI = false;
            INIManager iniManager = new INIManager(File)
                {
                    ReadWriteComments = ReadWriteComments,
                };
            return iniManager.Read();
        }

        public void Write(EditorINIData data)
        {
            //save data
            List<INIBlock> newData = new List<INIBlock>();
            foreach (EditorINIBlock block in data.Blocks)
            {
                INIOptions newBlock = new INIOptions();
                foreach (EditorINIOption option in block.Options)
                {
                    if (option.Values.Count > 0)
                    {
                        List<INIOption> newOption = new List<INIOption>();

                        foreach (EditorINIEntry entry in option.Values)
                        {
                            string optionValue = entry.Value.ToString();
                            if (optionValue == "=")
                            {
                                //use an empty value for options which dont have values and are simply defined when using the option key followed by a colon equal
                                newOption.Add(new INIOption
                                    {
                                        Value = string.Empty
                                    });
                            }
                            else
                            {
                                newOption.Add(new INIOption
                                    {
                                        Value = optionValue
                                    });
                            }

                            //add suboptions as options with defined parent
                            if (entry.SubOptions != null)
                            {
                                for (int k = 0; k < entry.SubOptions.Count; ++k)
                                {
                                    newOption.Add(new INIOption
                                        {
                                            Value = entry.SubOptions[k].ToString(),
                                            Parent = option.ChildName
                                        });
                                }
                            }
                        }

                        newBlock.Add(option.Name, newOption);
                    }
                }
                newData.Add(new INIBlock
                    {
                        Name = block.Name,
                        Options = newBlock,
                        Comments = block.Comments,
                    });
            }

            //if (IsBINI)
            //{
            //    BINIManager biniManager = new BINIManager(File);
            //    biniManager.Data = newData;
            //    biniManager.Write();
            //}
            //else
            //{
            INIManager iniManager = new INIManager(File)
                {
                    WriteEmptyLine = WriteEmptyLine,
                    WriteSpaces = WriteSpaces,
                    ReadWriteComments = ReadWriteComments,
                };
            iniManager.Write(newData);
            //}
        }

        /*object ConvertToTemplate(Template.OptionType type, string value)
        {
            try
            {
                switch (type)
                {
                    case Template.OptionType.Int:
                        return Convert.ToInt32(value);

                    case Template.OptionType.Bool:
                        return Convert.ToBoolean(value);

                    case Template.OptionType.Double:
                        return Convert.ToDouble(value);

                    case Template.OptionType.Enum:
                        return Enum.Parse(typeof(string), value, true);

                    case Template.OptionType.StringArray:
                        return ConvertToArray(ArrayType.String, value);

                    case Template.OptionType.IntArray:
                        return ConvertToArray(ArrayType.Int, value);

                    case Template.OptionType.DoubleArray:
                        return ConvertToArray(ArrayType.Double, value);

                    case Template.OptionType.Point:
                        return ConvertToArray(ArrayType.Point, value);

                    case Template.OptionType.RGB:
                        return ConvertToArray(ArrayType.RGB, value);

                }
            }
            //return string if error occured at conversion
            catch { }

            return value;
        }

        object ConvertToArray(ArrayType type, string value)
        {
            string[] arrayValues = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (type == ArrayType.String)
            {
                return arrayValues;
            }

            switch (type)
            {
                case ArrayType.Int:
                    {
                        List<int> newValues = new List<int>();

                        foreach (string arrayValue in arrayValues)
                        {
                            newValues.Add(Convert.ToInt32(arrayValue.Trim()));
                        }

                        return newValues.ToArray();
                    }
                case ArrayType.Double:
                    {
                        List<double> newValues = new List<double>();

                        foreach (string arrayValue in arrayValues)
                        {
                            newValues.Add(Convert.ToDouble(arrayValue.Trim()));
                        }

                        return newValues.ToArray();
                    }
                case ArrayType.Point:
                    if (arrayValues.Length == 2)
                    {
                        return new Point(Convert.ToInt32(arrayValues[0].Trim()), Convert.ToInt32(arrayValues[1].Trim()));
                    }
                    break;
                case ArrayType.RGB:
                    if (arrayValues.Length == 3)
                    {
                        return Color.FromArgb(Convert.ToInt32(arrayValues[0].Trim()), Convert.ToInt32(arrayValues[1].Trim()), Convert.ToInt32(arrayValues[2].Trim()));
                    }
                    break;
            }

            return null;
        }*/
    }

    /*internal enum ArrayType
    {
        Int,
        Double,
        String,
        Point,
        RGB
    }*/
}
