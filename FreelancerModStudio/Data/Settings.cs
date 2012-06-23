using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FreelancerModStudio.Data
{
    public class Settings
    {
        public SettingsData Data = new SettingsData();

        public void Load(Stream stream)
        {
            Data = (SettingsData)Serializer.Load(stream, Data.GetType());
        }

        public void Load(string path)
        {
            Data = (SettingsData)Serializer.Load(path, Data.GetType());
        }

        public void Save(Stream stream)
        {
            Serializer.Save(stream, Data);
        }

        public void Save(string path)
        {
            Serializer.Save(path, Data);
        }

        [XmlRoot("FreelancerModStudio-Settings-1.0")]
        public class SettingsData
        {
            public General General = new General();
            public Forms Forms = new Forms();
        }

        [DisplayName("General")]
        public class General
        {
            [Category("General")]
            [DisplayName("Display recent files")]
            public ushort RecentFilesCount { get; set; }

            [Category("General")]
            [DisplayName("Language")]
            public LanguageType Language { get; set; }

            [Category("Properties")]
            [DisplayName("Sort type")]
            public PropertySort PropertiesSortType { get; set; }

            [Category("Properties")]
            [DisplayName("Show description")]
            public bool PropertiesShowHelp { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Modified row color")]
            public Color EditorModifiedColor { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Saved row color")]
            public Color EditorModifiedSavedColor { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Hidden row color")]
            public Color EditorHiddenColor { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Spaces around equal sign")]
            public bool FormattingSpaces { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Empty line between sections")]
            public bool FormattingEmptyLine { get; set; }

            [Browsable(false)]
            public string EditorModifiedSavedColorXML
            {
                get
                {
                    return ColorTranslator.ToHtml(EditorModifiedSavedColor);
                }
                set
                {
                    EditorModifiedSavedColor = ColorTranslator.FromHtml(value);
                }
            }

            [Browsable(false)]
            public string EditorModifiedColorXML
            {
                get
                {
                    return ColorTranslator.ToHtml(EditorModifiedColor);
                }
                set
                {
                    EditorModifiedColor = ColorTranslator.FromHtml(value);
                }
            }

            [Browsable(false)]
            public string EditorHiddenColorXML
            {
                get
                {
                    return ColorTranslator.ToHtml(EditorHiddenColor);
                }
                set
                {
                    EditorHiddenColor = ColorTranslator.FromHtml(value);
                }
            }

            public AutoUpdate AutoUpdate { get; set; }

            public General()
            {
                RecentFilesCount = 4;
                Language = LanguageType.English;

                PropertiesSortType = PropertySort.NoSort;
                PropertiesShowHelp = false;

                EditorModifiedColor = Color.FromArgb(255, 255, 164);
                EditorModifiedSavedColor = Color.FromArgb(192, 255, 192);
                EditorHiddenColor = Color.FromArgb(128, 128, 128);

                FormattingSpaces = true;
                FormattingEmptyLine = true;

                AutoUpdate = new AutoUpdate();
            }
        }

        [Category("Auto Update")]
        [DisplayName("Auto Update")]
        [TypeConverter(typeof(SettingsConverter))]
        public class AutoUpdate
        {
            [DisplayName("Active")]
            public bool Enabled { get; set; }

            [DisplayName("Check each days")]
            public uint CheckInterval { get; set; }

            [DisplayName("Download silent")]
            public bool SilentDownload { get; set; }

            [DisplayName("Check file")]
            public string UpdateFile { get; set; }

            public DateTime LastCheck;

            public Update Update = new Update();
            public Proxy Proxy { get; set; }

            public AutoUpdate()
            {
                Enabled = true;
                CheckInterval = 28;
                SilentDownload = false;
                UpdateFile = @"http://freelancermodstudio.googlecode.com/files/updates.txt";
                Proxy = new Proxy();
            }
        }

        public class Update
        {
            public string FileName;
            public bool Downloaded;
            public bool Installed;
            public bool SilentInstall;
        }

        [TypeConverter(typeof(SettingsConverter))]
        public class Proxy
        {
            [DisplayName("Active")]
            public bool Enabled { get; set; }

            [DisplayName("Address")]
            public string Uri { get; set; }

            [DisplayName("Username")]
            public string UserName { get; set; }

            public string Password { get; set; }

            public Proxy()
            {
                Enabled = false;
            }
        }

        public class Forms
        {
            public Main Main = new Main();
            //public NewMod NewMod = new NewMod();
            public ChooseFileType ChooseFileType = new ChooseFileType();
        }

        public class Main
        {
            [XmlArrayItem("RecentFile")]
            public List<RecentFile> RecentFiles = new List<RecentFile>();

            public Point Location;
            public Size Size;

            public bool Maximized;
            public bool FullScreen;
        }

        /*public class NewMod
        {
            public string ModSaveLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Mods");
            public Size Size;
        }*/

        public class ChooseFileType
        {
            public int SelectedFileType;
        }

        public class RecentFile
        {
            public string File;
            public int TemplateIndex = -1;
        }
    }

    public enum LanguageType
    {
        English,
        German
    }

    public class SettingsConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return string.Empty;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }
    }
}
