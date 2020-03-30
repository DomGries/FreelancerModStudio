namespace FreelancerModStudio.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    public class Settings
    {
        private const int CURRENT_VERSION = 1;

        // const string FREELANCER_REGISTRY_KEY = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Microsoft Games\\Freelancer\\1.0";
        // const string FREELANCER_REGISTRY_VALUE = "AppPath";
        public SettingsData Data = new SettingsData();

        public void Load(Stream stream)
        {
            this.Data = (SettingsData)Serializer.Load(stream, typeof(SettingsData));
        }

        public void Load(string path)
        {
            this.Data = (SettingsData)Serializer.Load(path, typeof(SettingsData));
        }

        public void Save(Stream stream)
        {
            Serializer.Save(stream, this.Data, typeof(SettingsData));
        }

        public void Save(string path)
        {
            Serializer.Save(path, this.Data, typeof(SettingsData));
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
            [Browsable(false)]
            public int Version { get; set; }

            [Category("General")]
            [DisplayName("Display recent files")]
            public ushort RecentFilesCount { get; set; }

            [Category("General")]
            [DisplayName("Language")]
            public LanguageType Language { get; set; }

            [Category("General")]
            [DisplayName("Default Freelancer DATA folder")]
            public string DefaultDataDirectory { get; set; }

            [Category("Properties")]
            [DisplayName("Sort type")]
            public PropertySort PropertiesSortType { get; set; }

            [Category("Properties")]
            [DisplayName("Show description")]
            public bool PropertiesShowHelp { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Added row color")]
            public Color EditorModifiedAddedColor { get; set; }

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
            [DisplayName("Hidden text color")]
            public Color EditorHiddenColor { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Spaces around equal sign")]
            public bool FormattingSpaces { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Empty line between sections")]
            public bool FormattingEmptyLine { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Comments")]
            public bool FormattingComments { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Automatically round floating point values")]
            public bool RoundFloatingPoints { get; set; }

            [Browsable(false)]
            public string EditorModifiedAddedColorXml
            {
                get
                {
                    return ColorTranslator.ToHtml(this.EditorModifiedAddedColor);
                }

                set
                {
                    this.EditorModifiedAddedColor = ColorTranslator.FromHtml(value);
                }
            }

            [Browsable(false)]
            public string EditorModifiedColorXml
            {
                get
                {
                    return ColorTranslator.ToHtml(this.EditorModifiedColor);
                }

                set
                {
                    this.EditorModifiedColor = ColorTranslator.FromHtml(value);
                }
            }

            [Browsable(false)]
            public string EditorModifiedSavedColorXml
            {
                get
                {
                    return ColorTranslator.ToHtml(this.EditorModifiedSavedColor);
                }

                set
                {
                    this.EditorModifiedSavedColor = ColorTranslator.FromHtml(value);
                }
            }

            [Browsable(false)]
            public string EditorHiddenColorXml
            {
                get
                {
                    return ColorTranslator.ToHtml(this.EditorHiddenColor);
                }

                set
                {
                    this.EditorHiddenColor = ColorTranslator.FromHtml(value);
                }
            }

            public AutoUpdate AutoUpdate { get; set; }

            public General()
            {
                // set default values
                this.RecentFilesCount = 4;
                this.Language = LanguageType.English;

                this.PropertiesSortType = PropertySort.NoSort;
                this.PropertiesShowHelp = false;

                this.EditorModifiedAddedColor = Color.FromArgb(255, 255, 164);
                this.EditorModifiedColor = Color.FromArgb(255, 227, 164);
                this.EditorModifiedSavedColor = Color.FromArgb(192, 255, 192);
                this.EditorHiddenColor = Color.FromArgb(128, 128, 128);

                this.FormattingSpaces = true;
                this.FormattingEmptyLine = true;
                this.FormattingComments = true;

                this.AutoUpdate = new AutoUpdate { Enabled = true, Proxy = new Proxy(), };
                this.SetDefaultAutoUpdate();
            }

            public void CheckVersion()
            {
                if (this.Version < CURRENT_VERSION)
                {
                    this.SetDefaultAutoUpdate();

                    // DefaultDataDirectory = Registry.GetValue(FREELANCER_REGISTRY_KEY, FREELANCER_REGISTRY_VALUE, null) as string;
                    this.Version = CURRENT_VERSION;
                }
            }

            public void CheckValidData()
            {
                if (this.DefaultDataDirectory != null)
                {
                    if (!Directory.Exists(this.DefaultDataDirectory))
                    {
                        this.DefaultDataDirectory = null;
                    }
                }
            }

            private void SetDefaultAutoUpdate()
            {
                this.AutoUpdate.CheckInterval = 28;
                this.AutoUpdate.SilentDownload = false;
                this.AutoUpdate.UpdateFile = @"http://freelancermodstudio.googlecode.com/svn/trunk/updates.txt";
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
        }

        public class Forms
        {
            public Main Main = new Main();

            // public NewMod NewMod = new NewMod();
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
