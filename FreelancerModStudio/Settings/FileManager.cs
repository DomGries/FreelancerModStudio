using System;
using System.Collections.Generic;

namespace FreelancerModStudio.Settings
{
    public class FileManager
    {
        public static TemplateINIData Read(FileEncoding encoding, int templateIndex, string file)
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            INIBlocks data = null;
            try
            {
                //read basic file structure
                if (encoding == FileEncoding.Automatic || encoding == FileEncoding.BINI)
                {
                    BINIManager biniManager = new BINIManager(file);
                    biniManager.Read();
                    if (biniManager.IsBini)
                        data = biniManager.Data;
                    else
                    {
                        if (encoding == FileEncoding.Automatic)
                            data = INIManager.Read(file);
                        else
                            return null;
                    }
                }
                else
                    data = INIManager.Read(file);
            }
            catch (Exception ex)
            {
                throw ex;
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("load data: " + st.ElapsedMilliseconds + "ms");

            st.Reset();
            st.Start();
#endif

            TemplateINIData newData = new TemplateINIData(templateIndex);
            for (int i = 0; i < Helper.Template.Data.Files[templateIndex].Blocks.Count; i++)
            {
                Template.Block block = Helper.Template.Data.Files[templateIndex].Blocks[i];
                int blockIndex = data.IndexOfKey(block.Name.ToLower());
                if (blockIndex != -1)
                {
                    foreach (INIOptions options in data.Values[blockIndex])
                    {
                        TemplateINIBlock templateBlock = new TemplateINIBlock(block.Name, i);
                        for (int j = 0; j < block.Options.Count; j++)
                        {
                            Template.Option option = block.Options[j];
                            int optionIndex = options.IndexOfKey(option.Name.ToLower());
                            if (optionIndex != -1)
                            {
                                if (options.Values[optionIndex].Count > 0)
                                {
                                    TemplateINIOption templateOption = new TemplateINIOption(option.Name, j);
                                    for (int k = 0; k < options.Values[optionIndex].Count; k++)
                                        templateOption.Values.Add(ConvertToTemplate(option.Type, options.Values[optionIndex][k].Value));

                                    templateBlock.Options.Add(templateOption);
                                }
                            }
                            else
                            {
                                TemplateINIOption templateOption = new TemplateINIOption(option.Name, j);
                                templateBlock.Options.Add(templateOption);
                            }

                            if (block.Identifier != null && block.Identifier.ToLower() == templateBlock.Options[templateBlock.Options.Count - 1].Name.ToLower())
                                templateBlock.MainOptionIndex = templateBlock.Options.Count - 1;
                        }
                        newData.Blocks.Add(templateBlock);
                    }
                }
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("typecast data: " + st.ElapsedMilliseconds + "ms");
#endif

            return newData;
        }

        public static void Write(TemplateINIData data, string file)
        {
            INIBlocks newData = new INIBlocks();
            foreach (TemplateINIBlock block in data.Blocks)
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
                INIManager.Write(file, newData);
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

        private static object ConvertToTemplate(Template.OptionType type, string value)
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

        private static object ConvertToArray(ArrayType type, string value)
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

    public class TemplateINIData
    {
        public List<TemplateINIBlock> Blocks = new List<TemplateINIBlock>();

        public int TemplateIndex;

        public TemplateINIData(int templateIndex)
        {
            this.TemplateIndex = templateIndex;
        }
    }

    public class TemplateINIBlock
    {
        public string Name;
        public List<TemplateINIOption> Options = new List<TemplateINIOption>();
        public int TemplateIndex;
        public int MainOptionIndex = -1;

        public TemplateINIBlock(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }
    }

    public class TemplateINIOption : IComparable<TemplateINIOption>
    {
        public string Name;
        public int TemplateIndex;
        public List<object> Values = new List<object>();

        public TemplateINIOption(string name, int templateIndex)
        {
            this.Name = name;
            this.TemplateIndex = templateIndex;
        }

        public int CompareTo(TemplateINIOption other)
        {
            return this.Name.CompareTo(other.Name);
        }
    }
}