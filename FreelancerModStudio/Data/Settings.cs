namespace FreelancerModStudio.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Xml.Serialization;

    using FreelancerModStudio.Controls;
    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.SystemDesigner;
    using FreelancerModStudio.SystemDesigner.Content;

    using Color = System.Windows.Media.Color;
    using ColorD = System.Drawing.Color;

    public class Settings
    {
        private const int CURRENT_VERSION = 1;

        // const string FREELANCER_REGISTRY_KEY = "HKEY_LOCAL_MACHINE\\Software\\Microsoft\\Microsoft Games\\Freelancer\\1.0";
        // const string FREELANCER_REGISTRY_VALUE = "AppPath";
        public SettingsData Data = new SettingsData();

        public void Load(Stream stream) => this.Data = (SettingsData)Serializer.Load(stream, typeof(SettingsData));
        public void Load(string path) => this.Data = (SettingsData)Serializer.Load(path, typeof(SettingsData));
        public void Save(Stream stream) => Serializer.Save(stream, this.Data, typeof(SettingsData));
        public void Save(string path) => Serializer.Save(path, this.Data, typeof(SettingsData));

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
            [DisplayName("Fallback Freelancer DATA folder")]
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
            public ColorD EditorModifiedAddedColor { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Modified row color")]
            public ColorD EditorModifiedColor { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Saved row color")]
            public ColorD EditorModifiedSavedColor { get; set; }

            [XmlIgnore]
            [Category("INI Editor")]
            [DisplayName("Hidden text color")]
            public ColorD EditorHiddenColor { get; set; }

            [Category("INI Editor")]
            [DisplayName("Templates")]
            public ObjectTemplate Templates { get; set; }

            [Category("INI Editor")]
            [DisplayName("Ignored 3D Editor Types")]
            public List<ContentType> IgnoredEditorTypes { get; set; } = new List<ContentType>()
                                                                            {
                                                                                ContentType.ZonePath,
                                                                                ContentType.ZoneVignette
                                                                            };

            [Category("INI Editor")]
            [DisplayName("Round floating point values")]
            public bool RoundFloatingPointValues { get; set; } = false;

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
            [DisplayName("Only insert objects at bottom of INI")]
            public bool OnlyInsertObjectsAtIniBottom { get; set; }

            [Category("INI Formatting")]
            [DisplayName("Alert when providing invalid ini property type")]
            public bool AlertIncorrectPropertyType { get; set; }

            [Browsable(false)]
            public string EditorModifiedAddedColorXml
            {
                get => ColorTranslator.ToHtml(this.EditorModifiedAddedColor);
                set => this.EditorModifiedAddedColor = ColorTranslator.FromHtml(value);
            }

            [Browsable(false)]
            public string EditorModifiedColorXml
            {
                get => ColorTranslator.ToHtml(this.EditorModifiedColor);
                set => this.EditorModifiedColor = ColorTranslator.FromHtml(value);
            }

            [Browsable(false)]
            public string EditorModifiedSavedColorXml
            {
                get => ColorTranslator.ToHtml(this.EditorModifiedSavedColor);
                set => this.EditorModifiedSavedColor = ColorTranslator.FromHtml(value);
            }

            [Browsable(false)]
            public string EditorHiddenColorXml
            {
                get => ColorTranslator.ToHtml(this.EditorHiddenColor);
                set => this.EditorHiddenColor = ColorTranslator.FromHtml(value);
            }

            [XmlElement("ColorBox")]
            public ColorBox ColorBox { get; set; }

            public AutoUpdate AutoUpdate { get; set; }

            public General()
            {
                // set default values
                this.RecentFilesCount = 4;
                this.Language = LanguageType.English;

                this.PropertiesSortType = PropertySort.NoSort;
                this.PropertiesShowHelp = false;

                this.EditorModifiedAddedColor = ColorD.FromArgb(255, 255, 164);
                this.EditorModifiedColor = ColorD.FromArgb(255, 227, 164);
                this.EditorModifiedSavedColor = ColorD.FromArgb(192, 255, 192);
                this.EditorHiddenColor = ColorD.FromArgb(128, 128, 128);

                this.FormattingSpaces = true;
                this.FormattingEmptyLine = true;
                this.FormattingComments = true;
                this.AlertIncorrectPropertyType = true;
                this.OnlyInsertObjectsAtIniBottom = false;

                this.AutoUpdate = new AutoUpdate { Enabled = true, Proxy = new Proxy(), };
                this.SetDefaultAutoUpdate();
                this.ColorBox = new ColorBox();
                this.Templates = new ObjectTemplate();
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
                    if (!Directory.Exists(this.DefaultDataDirectory))
                        this.DefaultDataDirectory = null;
            }

            private void SetDefaultAutoUpdate()
            {
                this.AutoUpdate.CheckInterval = 28;
                this.AutoUpdate.SilentDownload = true;
                this.AutoUpdate.UpdateFile = @"https://raw.githubusercontent.com/AftermathFreelancer/FLModStudio/master/updates.txt";
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

        [Category("Colours")]
        [DisplayName("Colours")]
        [TypeConverter(typeof(SettingsConverter))]
        public class ColorBox
        {
            [DisplayName("Construct")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Construct")]
            public Color Construct { get; set; } = Colors.Fuchsia;

            [DisplayName("Depot")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Depot")]
            public Color Depot { get; set; } = Colors.SlateGray;

            [DisplayName("DockingRing")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "DockingRing")]
            public Color DockingRing { get; set; } = Colors.DimGray;

            [DisplayName("JumpGate")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "JumpGate")]
            public Color JumpGate { get; set; } = Colors.Green;

            [DisplayName("JumpHole")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "JumpHole")]
            public Color JumpHole { get; set; } = Colors.Firebrick;

            [DisplayName("Planet")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Planet")]
            public Color Planet { get; set; } = Color.FromRgb(0, 60, 120);

            [DisplayName("Satellite")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Satellite")]
            public Color Satellite { get; set; } = Colors.BlueViolet;

            [DisplayName("Ship")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Ship")]
            public Color Ship { get; set; } = Colors.Gold;

            [DisplayName("Station")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Station")]
            public Color Station { get; set; } = Colors.Orange;

            [DisplayName("Sun")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Sun")]
            public Color Sun { get; set; } = Colors.OrangeRed;

            [DisplayName("Tradelane")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "Tradelane")]
            public Color Tradelane { get; set; } = Colors.Cyan;

            [DisplayName("Weapons Platforms")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "WeaponsPlatform")]
            public Color WeaponsPlatform { get; set; } = Colors.BurlyWood;

            [DisplayName("ZoneVigentte")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "ZoneVignette")]
            public Color ZoneVignette { get; set; } = Color.FromRgb(0, 30, 15);

            [DisplayName("ZonePathTrade")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "ZonePathTrade")]
            public Color ZonePathTrade { get; set; } = Color.FromRgb(0, 30, 30);

            [DisplayName("ZonePathTradelane")]
            [XmlElement(Type = typeof(XmlColor), ElementName = "ZonePathTradeLane")]
            public Color ZonePathTradeLane { get; set; } = Color.FromRgb(0, 30, 30);
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

        [TypeConverter(typeof(SettingsConverter))]
        public class ObjectTemplate
        {
            public List<EditorIniBlock> Templates { get; set; }
            /*public string nickname { get; set; }
            public string ids_name { get; set; }
            public string pos { get; set; }
            public string rotate { get; set; }
            public string ambient_color { get; set; }
            public string archetype { get; set; }
            public string star { get; set; }
            public string spin { get; set; }
            public string msg_id_prefix { get; set; }
            public string jump_effect { get; set; }
            public string atmosphere_range { get; set; }
            public string burn_color { get; set; }
            public string prev_ring { get; set; }
            public string next_ring { get; set; }
            public string ids_info  { get; set; }
            public string ring { get; set; }
            public string Base { get; set; }
            public string dock_with { get; set; }
            public string Ambient { get; set; }
            public string visit { get; set; }
            public string reputation { get; set; }
            public string tradelane_space_name { get; set; }
            public string behavior { get; set; }
            public string voice { get; set; }
            public string space_costume { get; set; }
            public string faction { get; set; }
            public string difficulty_level { get; set; }
            public string Goto { get; set; }
            public string loadout { get; set; }
            public string pilot { get; set; }
            public string parent { get; set; }*/

            public ObjectTemplate()
            {
                this.Templates = new List<EditorIniBlock>();
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
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => string.Empty;
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;
    }
}
