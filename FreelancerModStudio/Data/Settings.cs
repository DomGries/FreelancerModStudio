using System;
using System.Drawing;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Globalization;

namespace FreelancerModStudio.Data
{
    public class Settings
    {
        public SettingsData Data = new SettingsData();

        public void Load(System.IO.Stream stream)
        {
            this.Data = (SettingsData)Serializer.Load(stream, this.Data.GetType());
        }

        public void Load(string path)
        {
            this.Data = (SettingsData)Serializer.Load(path, this.Data.GetType());
        }

        public void Save(System.IO.Stream stream)
        {
            Serializer.Save(stream, this.Data);
        }

        public void Save(string path)
        {
            Serializer.Save(path, this.Data);
        }

        [XmlRootAttribute("FreelancerModStudio-Settings-1.0")]
        public class SettingsData
        {
            public General General = new General();
            public Forms Forms = new Forms();
        }

        [DisplayName("General")]
        public class General
        {
            [CategoryAttribute("General"),
                DisplayName("Display recent files")]
            public ushort RecentFilesCount { get; set; }

            [CategoryAttribute("General"),
                DisplayName("Language")]
            public LanguageType Language { get; set; }

            [CategoryAttribute("Properties"),
                DisplayName("Sort type")]
            public System.Windows.Forms.PropertySort PropertiesSortType { get; set; }

            [CategoryAttribute("Properties"),
                DisplayName("Show description")]
            public bool PropertiesShowHelp { get; set; }

            [XmlIgnore,
                CategoryAttribute("INI Editor"),
                DisplayName("Modified row color")]
            public Color EditorModifiedColor { get; set; }

            [XmlIgnore,
                CategoryAttribute("INI Editor"),
                DisplayName("Saved row color")]
            public Color EditorModifiedSavedColor { get; set; }

            [XmlIgnore,
                CategoryAttribute("INI Editor"),
                DisplayName("Hidden row color")]
            public Color EditorHiddenColor { get; set; }

            [Browsable(false)]
            public string EditorModifiedSavedColorXML
            {
                get { return ColorTranslator.ToHtml(EditorModifiedSavedColor); }
                set { EditorModifiedSavedColor = ColorTranslator.FromHtml(value); }
            }

            [Browsable(false)]
            public string EditorModifiedColorXML
            {
                get { return ColorTranslator.ToHtml(EditorModifiedColor); }
                set { EditorModifiedColor = ColorTranslator.FromHtml(value); }
            }

            [Browsable(false)]
            public string EditorHiddenColorXML
            {
                get { return ColorTranslator.ToHtml(EditorHiddenColor); }
                set { EditorHiddenColor = ColorTranslator.FromHtml(value); }
            }

            public AutoUpdate AutoUpdate { get; set; }

            public General()
            {
                RecentFilesCount = 4;
                Language = LanguageType.English;

                PropertiesSortType = System.Windows.Forms.PropertySort.NoSort;
                PropertiesShowHelp = false;

                EditorModifiedColor = Color.FromArgb(255, 255, 164);
                EditorModifiedSavedColor = Color.FromArgb(192, 255, 192);
                EditorHiddenColor = Color.FromArgb(128, 128, 128);

                AutoUpdate = new AutoUpdate();
            }
        }

        [CategoryAttribute("Auto Update"),
            DisplayName("Auto Update"),
            TypeConverter(typeof(SettingsConverter))]
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
                CheckInterval = 14;
                SilentDownload = true;
                UpdateFile = @"http://freelancermodstudio.googlecode.com/files/updates.txt";
                Proxy = new Proxy();
            }
        }

        public class Update
        {
            public string FileName;
            public bool Downloaded = false;
            public bool Installed = false;
            public bool SilentInstall;
        }

        [TypeConverter(typeof(SettingsConverter))]
        public class Proxy
        {
            [DisplayName("Active")]
            public bool Enabled { get; set; }
            [DisplayName("Address")]
            public string Uri { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public Proxy()
            {
                Enabled = false;
            }
        }

        public class Forms
        {
            public Main Main = new Main();
            public NewMod NewMod = new NewMod();
            public ChooseFileType ChooseFileType = new ChooseFileType();
        }

        public class Main
        {
            [XmlArrayItem("RecentFile")]
            public List<RecentFile> RecentFiles = new List<RecentFile>();

            public System.Drawing.Point Location;
            public System.Drawing.Size Size;

            public bool Maximized;
            public bool FullScreen;
        }

        public class NewMod
        {
            public string ModSaveLocation = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Mods");
            public System.Drawing.Size Size;
        }

        public class ChooseFileType
        {
            public int SelectedFileType;
        }

        public class RecentFile
        {
            public string File;
            public int TemplateIndex = -1;

            public RecentFile() { }
            public RecentFile(string file, int templateIndex)
            {
                File = file;
                TemplateIndex = templateIndex;
            }
        }
    }

    public enum LanguageType
    {
        English,
        German
    }

    public class SettingsConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            return "";
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            return false;
        }
    }
}