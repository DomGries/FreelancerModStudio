using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Settings
{
    public class FileManager
    {
        public string File { get; set; }
        public bool IsBini { get; set; }

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
            INIBlocks iniData = null;
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

        private EditorINIData GetEditorData(INIBlocks iniData, int templateFileIndex)
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif

            EditorINIData editorData = new EditorINIData(templateFileIndex);

            //loop each template block
            for (int i = 0; i < Helper.Template.Data.Files[templateFileIndex].Blocks.Count; i++)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[templateFileIndex].Blocks[i];

                int iniBlockIndex = iniData.IndexOfKey(templateBlock.Name);
                if (iniBlockIndex != -1)
                {
                    //loop each ini block
                    foreach (INIOptions iniBlock in iniData.Values[iniBlockIndex])
                    {
                        EditorINIBlock editorBlock = new EditorINIBlock(templateBlock.Name, i);

                        //loop each template option
                        for (int j = 0; j < templateBlock.Options.Count; j++)
                        {
                            int childOptionIndex = -1;
                            Template.Option childTemplateOption = null;
                            if (j < templateBlock.Options.Count - 1 && templateBlock.Options[j + 1].Parent != null)
                            {
                                //next option is child of current option
                                childTemplateOption = templateBlock.Options[j + 1];
                                childOptionIndex = iniBlock.IndexOfKey(childTemplateOption.Name.ToLower());
                            }

                            Template.Option templateOption = templateBlock.Options[j];
                            EditorINIOption editorOption = new EditorINIOption(templateOption.Name, j);

                            int optionIndex = iniBlock.IndexOfKey(templateOption.Name.ToLower());
                            if (optionIndex != -1)
                            {
                                //h is used to start again at last child option in order to provide better performance
                                int h = 0;

                                //loop each ini option
                                for (int k = 0; k < iniBlock.Values[optionIndex].Count; k++)
                                {
                                    EditorINISubOptions editorSubOptions = null;
                                    if (childOptionIndex != -1)
                                    {
                                        editorSubOptions = new EditorINISubOptions(childTemplateOption.Name, j + 1);

                                        //loop each ini option of child
                                        for (; h < iniBlock.Values[childOptionIndex].Count; h++)
                                        {
                                            INIOption childOption = iniBlock.Values[childOptionIndex][h];
                                            if (k < iniBlock.Values[optionIndex].Count - 1 && childOption.Index > iniBlock.Values[optionIndex][k + 1].Index)
                                                break;

                                            if (childOption.Index > iniBlock.Values[optionIndex][k].Index)
                                                editorSubOptions.Values.Add(ConvertToTemplate(childTemplateOption.Type, childOption.Value));
                                        }
                                    }

                                    //add entry
                                    editorOption.Values.Add(new EditorINIEntry(ConvertToTemplate(templateOption.Type, iniBlock.Values[optionIndex][k].Value), editorSubOptions));
                                }

                                //add option
                                editorBlock.Options.Add(editorOption);
                            }
                            else
                                //add empty option
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
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("typecast data: " + st.ElapsedMilliseconds + "ms");
#endif

            return editorData;
        }

        private INIBlocks ReadINI()
        {
            IsBini = false;
            INIManager iniManager = new INIManager(File);
            return iniManager.Read();
        }

        public void Write(EditorINIData data)
        {
            INIBlocks newData = new INIBlocks();
            foreach (EditorINIBlock block in data.Blocks)
            {
                INIOptions newBlock = new INIOptions();
                for (int i = 0; i < block.Options.Count; i++)
                {
                    List<INIOption> newOption = new List<INIOption>();
                    for (int j = 0; j < block.Options[i].Values.Count; j++)
                        newOption.Add(new INIOption(block.Options[j].Values[i].ToString(), j));

                    newBlock.Add(block.Options[i].Name, newOption);
                }
                newData.Add(block.Name, newBlock);
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
                string pattern = ".+" + Helper.Template.Data.Files[i].Path.Replace("\\", "\\\\").Replace("*", "[^\\\\]+");
                if (System.Text.RegularExpressions.Regex.Match(file.ToLower(), pattern).Success)
                    return i;
            }
            return -1;
        }

        private object ConvertToTemplate(Template.OptionType type, string value)
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

        private object ConvertToArray(ArrayType type, string value)
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

    public class EditorINIData
    {
        public List<EditorINIBlock> Blocks = new List<EditorINIBlock>();

        public int TemplateIndex;

        public EditorINIData(int templateIndex)
        {
            this.TemplateIndex = templateIndex;
        }
    }

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

    public class EditorINIOption
    {
        public string Name;
        public int TemplateIndex;
        public List<EditorINIEntry> Values = new List<EditorINIEntry>();

        public EditorINIOption(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }

    public class EditorINIEntry
    {
        public object Value;
        public EditorINISubOptions SubOptions;

        public EditorINIEntry(object value)
        {
            Value = value;
        }

        public EditorINIEntry(object value, EditorINISubOptions subOptions)
        {
            Value = value;
            SubOptions = subOptions;
        }
    }

    public class EditorINISubOptions
    {
        public string Name;
        public int TemplateIndex;
        public List<object> Values = new List<object>();

        public EditorINISubOptions(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }
}