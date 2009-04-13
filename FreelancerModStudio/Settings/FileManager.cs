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
            //try
            //{
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
                            data = INIManager.Read(file, false);
                        else
                            return null;
                    }
                }
                else
                    data = INIManager.Read(file, false);
            try{}
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
                                foreach (string optionValue in options.Values[optionIndex])
                                {
                                    templateBlock.Options.Add(new TemplateINIOption(option.Name, ConvertToTemplate(option.Type, optionValue), j));

                                    if (block.Identifier != null && block.Identifier.ToLower() == options.Keys[optionIndex].ToLower())
                                        templateBlock.MainOptionIndex = templateBlock.Options.Count - 1;
                                }
                            }
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
                KeyValuePair<string, INIOptions> newBlock = new KeyValuePair<string, INIOptions>(block.Name, new INIOptions());
                foreach (TemplateINIOption option in block.Options)
                    newBlock.Value.Add(new KeyValuePair<string, string>(option.Name, option.Value.ToString()));

                newData.Add(newBlock);
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

                default:
                    return value;
            }
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

    public class TemplateINIBlock : IComparable<TemplateINIBlock>
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

        public int CompareTo(TemplateINIBlock other)
        {
            int nameComparison = this.Name.CompareTo(other.Name);
            if (nameComparison == 0 && this.MainOptionIndex > -1 && other.MainOptionIndex > -1 && this.Options.Count >= this.MainOptionIndex + 1 && other.Options.Count >= other.MainOptionIndex + 1)
                return this.Options[this.MainOptionIndex].Value.ToString().CompareTo(other.Options[other.MainOptionIndex].Value.ToString());

            return nameComparison;
        }
    }

    public class TemplateINIOption : IComparable<TemplateINIOption>
    {
        public string Name;

        public object Value;

        public int TemplateIndex;

        public TemplateINIOption(string name, object value, int templateIndex)
        {
            this.Name = name;
            this.Value = value;
            this.TemplateIndex = templateIndex;
        }

        public int CompareTo(TemplateINIOption other)
        {
            return this.Name.CompareTo(other.Name);
        }
    }
}