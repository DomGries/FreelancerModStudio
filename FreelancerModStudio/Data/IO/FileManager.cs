using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Data.IO
{
    public class FileManager
    {
        public string File { get; set; }
        public bool IsBini { get; set; }
        public bool WriteSpaces { get; set; }
        public bool WriteEmptyLine { get; set; }

        public FileManager(string file, bool isBINI)
        {
            IsBini = isBINI;
            File = file;
        }

        public FileManager(string file)
        {
            File = file;
        }

        public EditorINIData Read(FileEncoding encoding, int templateFileIndex)
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            List<INIBlock> iniData = null;
            try
            {
                //read basic file structure
                if (encoding == FileEncoding.Automatic || encoding == FileEncoding.BINI)
                {
                    IsBini = true;

                    BINIManager biniManager = new BINIManager(File);
                    biniManager.Read();
                    if (biniManager.IsBini)
                        iniData = biniManager.Data;
                    else
                    {
                        if (encoding == FileEncoding.Automatic)
                            iniData = ReadINI();
                        else
                            return null;
                    }
                }
                else
                    iniData = ReadINI();
            }
            catch (Exception ex)
            {
                throw ex;
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("load data: " + st.ElapsedMilliseconds + "ms");
#endif

            return GetEditorData(iniData, templateFileIndex); ;
        }

        EditorINIData GetEditorData(List<INIBlock> iniData, int templateFileIndex)
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
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
                    EditorINIBlock editorBlock = new EditorINIBlock(templateBlock.Name, templateBlockIndex);

                    //loop each template option
                    for (int j = 0; j < templateBlock.Options.Count; j++)
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

                        List<INIOption> iniOptions;

                        //get ini options based on template option name
                        if (iniBlock.Options.TryGetValue(templateOption.Name, out iniOptions))
                        {
                            //h is used to start again at last child option in order to provide better performance
                            int h = 0;

                            if (templateOption.Multiple)
                            {
                                //loop each ini option
                                for (int k = 0; k < iniOptions.Count; k++)
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
                                        for (; h < iniChildOptions.Count; h++)
                                        {
                                            INIOption childOption = iniChildOptions[h];
                                            if (k < iniOptions.Count - 1 && childOption.Index > iniOptions[k + 1].Index)
                                                break;

                                            if (childOption.Index > iniOptions[k].Index)
                                                editorChildOptions.Add(ConvertToTemplate(childTemplateOption.Type, childOption.Value));
                                        }
                                    }

                                    //add entry of multiple option
                                    editorOption.Values.Add(new EditorINIEntry(ConvertToTemplate(templateOption.Type, iniOptions[k].Value), editorChildOptions));
                                }
                            }
                            else //single option
                            {
                                if (iniOptions.Count > 0)
                                {
                                    if (iniOptions[0].Value.Length > 0)
                                        //just add the last option (Freelancer like) if aviable to prevent multiple options which should be single
                                        editorOption.Values.Add(new EditorINIEntry(ConvertToTemplate(templateOption.Type, iniOptions[iniOptions.Count - 1].Value)));
                                    else
                                        //use '=' for options which dont have values and are simply defined when using the option key followed by a colon equal
                                        editorOption.Values.Add(new EditorINIEntry("="));
                                }
                            }

                            //add option
                            editorBlock.Options.Add(editorOption);
                        }
                        else
                            //add empty option based on template
                            editorBlock.Options.Add(new EditorINIOption(templateOption.Name, j));

                        //set index of main option (value displayed in table view)
                        if (templateBlock.Identifier != null && templateBlock.Identifier.ToLower() == editorBlock.Options[editorBlock.Options.Count - 1].Name.ToLower())
                            editorBlock.MainOptionIndex = editorBlock.Options.Count - 1;

                        //ignore next option because we already added it as children to the current option
                        if (childTemplateOption != null)
                            j++;
                    }

                    //add block
                    editorData.Blocks.Add(editorBlock);
                }
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("typecast data: " + st.ElapsedMilliseconds + "ms");
#endif

            return editorData;
        }

        List<INIBlock> ReadINI()
        {
            IsBini = false;
            INIManager iniManager = new INIManager(File);
            return iniManager.Read();
        }

        public void Write(EditorINIData data)
        {
            //sort blocks first
            //for (int i = 0; i < Helper.Template.Data.Files.Values[data.TemplateIndex].Blocks.Count; i++)
            //{
            //    Template.Block templateBlock = Helper.Template.Data.Files.Values[data.TemplateIndex].Blocks[i];

            //    for (int j = i; j < data.Blocks.Count; j++)
            //    {
            //        if (data.Blocks[j].Name.ToLower() == templateBlock.Name.ToLower())
            //        {
            //            //swap blocks
            //            EditorINIBlock temporaryBlock = data.Blocks[i];
            //            data.Blocks[i] = data.Blocks[j];
            //            data.Blocks[j] = temporaryBlock;
            //            break;
            //        }
            //    }
            //}

            //save data
            List<INIBlock> newData = new List<INIBlock>();
            foreach (EditorINIBlock block in data.Blocks)
            {
                INIOptions newBlock = new INIOptions();
                for (int i = 0; i < block.Options.Count; i++)
                {
                    if (block.Options[i].Values.Count > 0)
                    {
                        List<INIOption> newOption = new List<INIOption>();

                        for (int j = 0; j < block.Options[i].Values.Count; j++)
                        {
                            string optionValue = block.Options[i].Values[j].Value.ToString();
                            if (optionValue == "=")
                                //use an empty value for options which dont have values and are simply defined when using the option key followed by a colon equal
                                newOption.Add(new INIOption(""));
                            else
                                newOption.Add(new INIOption(optionValue));

                            //add suboptions as options with defined parent
                            if (block.Options[i].Values[j].SubOptions != null)
                            {
                                for (int k = 0; k < block.Options[i].Values[j].SubOptions.Count; k++)
                                    newOption.Add(new INIOption(block.Options[i].Values[j].SubOptions[k].ToString(), block.Options[i].ChildName));
                            }
                        }

                        newBlock.Add(block.Options[i].Name, newOption);
                    }
                }
                newData.Add(new INIBlock() { Name = block.Name, Options = newBlock });
            }

            try
            {
                //if (IsBini)
                //{
                //    BINIManager biniManager = new BINIManager(File);
                //    biniManager.Data = newData;
                //    biniManager.Write();
                //}
                //else
                //{
                INIManager iniManager = new INIManager(File);
                iniManager.WriteSpaces = WriteSpaces;
                iniManager.WriteEmptyLine = WriteEmptyLine;
                iniManager.Write(newData);
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static int GetTemplateIndex(string file)
        {
            for (int i = 0; i < Helper.Template.Data.Files.Count; i++)
            {
                foreach (string path in Helper.Template.Data.Files[i].Pathes)
                {
                    string pattern = ".+" + path.Replace("\\", "\\\\").Replace("*", "[^\\\\]*");
                    if (System.Text.RegularExpressions.Regex.Match(file.ToLower(), pattern).Success)
                        return i;
                }
            }
            return -1;
        }

        object ConvertToTemplate(Template.OptionType type, string value)
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
            string[] arrayValues = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (type == ArrayType.String)
                return arrayValues;

            if (type == ArrayType.Int)
            {
                List<int> newValues = new List<int>();

                foreach (string arrayValue in arrayValues)
                    newValues.Add(Convert.ToInt32(arrayValue.Trim()));

                return newValues.ToArray();
            }
            else if (type == ArrayType.Double)
            {
                List<double> newValues = new List<double>();

                foreach (string arrayValue in arrayValues)
                    newValues.Add(Convert.ToDouble(arrayValue.Trim()));

                return newValues.ToArray();
            }
            else if (type == ArrayType.Point)
            {
                if (arrayValues.Length == 2)
                    return new System.Drawing.Point(Convert.ToInt32(arrayValues[0].Trim()), Convert.ToInt32(arrayValues[1].Trim()));
            }
            else if (type == ArrayType.RGB)
            {
                if (arrayValues.Length == 3)
                    return System.Drawing.Color.FromArgb(Convert.ToInt32(arrayValues[0].Trim()), Convert.ToInt32(arrayValues[1].Trim()), Convert.ToInt32(arrayValues[2].Trim()));
            }

            return null;
        }
    }

    enum ArrayType
    {
        Int,
        Double,
        String,
        Point,
        RGB
    }

    public enum FileEncoding
    {
        Automatic,
        BINI,
        INI
    }

    [Serializable]
    public class EditorINIData
    {
        public List<EditorINIBlock> Blocks = new List<EditorINIBlock>();
        public int TemplateIndex;

        public EditorINIData(int templateIndex)
        {
            this.TemplateIndex = templateIndex;
        }
    }

    [Serializable]
    public class EditorINIBlock
    {
        public string Name;
        public List<EditorINIOption> Options = new List<EditorINIOption>();
        public int TemplateIndex;
        public int MainOptionIndex = -1;

        public EditorINIBlock(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }

    [Serializable]
    public class EditorINIOption
    {
        public string Name;
        public int TemplateIndex = -1;

        public string ChildName;
        public int ChildTemplateIndex = -1;

        public List<EditorINIEntry> Values = new List<EditorINIEntry>();

        public EditorINIOption(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }

    [Serializable]
    public class EditorINIEntry
    {
        public object Value;
        public List<object> SubOptions;

        public EditorINIEntry(object value)
        {
            Value = value;
        }

        public EditorINIEntry(object value, List<object> subOptions)
        {
            Value = value;
            SubOptions = subOptions;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}