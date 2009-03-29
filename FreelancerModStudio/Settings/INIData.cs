using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FreelancerModStudio.Settings
{
    public class INIData
    {
        private string mFile;
        public List<INIGroup> Data;

        public string File
        {
            get { return this.mFile; }
            set { this.mFile = value; }
        }

        public INIData(string file)
        {
            this.mFile = file;
        }

        public INIData(string file, List<INIGroup> data)
        {
            this.mFile = file;
            this.Data = data;
        }

        public void Read()
        {
            List<INIGroup> data = new List<INIGroup>();

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(mFile, Encoding.Default);
                INIGroup currentGroup = null;

                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim();

                    //remove comments from data
                    int commentIndex = line.IndexOf(';');
                    if (commentIndex != -1)
                        line = line.Substring(0, commentIndex + 1).Trim();

                    if (line.Length > 0 && line[0] == '[' && line[line.Length - 1] == ']') //new group
                    {
                        if (currentGroup != null)
                            data.Add(currentGroup);

                        currentGroup = new INIGroup(line.Substring(1, line.Length - 2));
                    }
                    else if (currentGroup != null) //new value for group
                    {
                        //retrieve name and value from data
                        int valueIndex = line.IndexOf('=');
                        if (valueIndex != -1)
                        {
                            string name = line.Substring(0, valueIndex).Trim();
                            string value = line.Substring(valueIndex + 1, line.Length - valueIndex - 1).Trim();
                            currentGroup.Values.Add(new INIOption(name, value));
                        }
                    }
                }

                if (currentGroup != null)
                    data.Add(currentGroup);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(ex);
            }

            if (streamReader != null)
                streamReader.Close();

            Data = data;
        }

        public void Write()
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(mFile, false, Encoding.Default);

                //write each group
                for (int i = 0; i < Data.Count; i++)
                {
                    if (i > 0 && i < Data.Count - 1)
                        streamWriter.Write(Environment.NewLine + Environment.NewLine);

                    streamWriter.WriteLine("[" + Data[i].Name + "]");

                    //write each option
                    for (int j = 0; j < Data[i].Values.Count; j++)
                    {
                        streamWriter.Write(Data[i].Values[j].Name + " = " + Data[i].Values[j].Value);

                        if (j < Data[i].Values.Count - 1)
                            streamWriter.Write(Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(ex);
            }

            if (streamWriter != null)
                streamWriter.Close();
        }
    }

    public class INIGroup
    {
        public string Name;
        public List<INIOption> Values = new List<INIOption>();

        public INIGroup(string name)
        {
            this.Name = name;
        }
    }

    public class INIOption
    {
        public string Name;
        public string Value;

        public INIOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
