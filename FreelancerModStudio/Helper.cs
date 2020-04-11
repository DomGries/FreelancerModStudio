namespace FreelancerModStudio
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    using FLUtils;

    using FreelancerModStudio.AutoUpdate;
    using FreelancerModStudio.Data;
    using FreelancerModStudio.Properties;
    using FreelancerModStudio.SystemDesigner;

    internal static class Helper
    {
        internal struct Program
        {
            public static void Start()
            {
                // load settings
                Settings.Load();

                // install downloaded update if it exists
                if (Settings.Data.Data.General.AutoUpdate.Update.Downloaded)
                    if (AutoUpdate.AutoUpdate.InstallUpdate())
                        return;

                Template.Load();

                // whidbey color table (gray colors of menustrip and tabstrip)
                ProfessionalColorTable whidbeyColorTable = new ProfessionalColorTable { UseSystemColors = true };
                ToolStripManager.Renderer = new ToolStripProfessionalRenderer(whidbeyColorTable);

                //remove installed update if it exists
                if (Settings.Data.Data.General.AutoUpdate.Update.Installed)
                {
                    AutoUpdate.AutoUpdate.RemoveUpdate();
                }

                //check for update
                if (Settings.Data.Data.General.AutoUpdate.Enabled && Settings.Data.Data.General.AutoUpdate.UpdateFile != null && 
                    Settings.Data.Data.General.AutoUpdate.LastCheck.Date.AddDays(Settings.Data.Data.General.AutoUpdate.CheckInterval) <= DateTime.Now.Date)
                {
                    Update.Check(true, Settings.Data.Data.General.AutoUpdate.SilentDownload);
                }

                // start main form
                Application.Run(new MainForm());

                // save settings
                Settings.Save();
            }
        }

        internal struct Update
        {
            public static AutoUpdate.AutoUpdate AutoUpdate = new AutoUpdate.AutoUpdate();

            public static void Check(bool silentCheck, bool silentDownload)
            {
                if (AutoUpdate.Status != StatusType.Waiting)
                {
                    AutoUpdate.ShowUI();
                    return;
                }

                Uri checkFileUri;
                if (!Uri.TryCreate(Settings.Data.Data.General.AutoUpdate.UpdateFile, UriKind.Absolute, out checkFileUri))
                {
                    if (!silentCheck)
                    {
                        MessageBox.Show(string.Format(Strings.UpdatesDownloadException,  AssemblyUtils.Name),  AssemblyUtils.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;
                    return;
                }

                AutoUpdate.CheckFileUri = checkFileUri;

                string proxy = string.Empty;
                string userName = string.Empty;
                string password = string.Empty;

                if (Settings.Data.Data.General.AutoUpdate.Proxy.Enabled)
                {
                    proxy = Settings.Data.Data.General.AutoUpdate.Proxy.Uri;
                    userName = Settings.Data.Data.General.AutoUpdate.Proxy.UserName;
                    password = Settings.Data.Data.General.AutoUpdate.Proxy.Password;
                }

                AutoUpdate.SilentCheck = silentCheck;
                AutoUpdate.SilentDownload = silentDownload;
                AutoUpdate.SetProxy(proxy);
                AutoUpdate.SetCredentials(userName, password);

                AutoUpdate.Check();
            }
        }

        internal struct Template
        {
            private static FreelancerModStudio.Data.Template data;

            public static void Load()
            {
                Load(Path.Combine(Application.StartupPath, Resources.TemplatePath));
            }

            public static void Load(string file)
            {
                data = new FreelancerModStudio.Data.Template();

                try
                {
                    data.Load(file);
                    Data.SetSpecialFiles();
                }
                catch (IOException ex)
                {
                    Exceptions.Show(string.Format(Strings.TemplateLoadException, Resources.TemplatePath), ex);
                    Environment.Exit(0);
                }
            }

            public struct Data
            {
                public static int SystemFile { get; set; }
                public static int UniverseFile { get; set; }
                public static int SolarArchetypeFile { get; set; }
                public static int AsteroidArchetypeFile { get; set; }
                public static int ShipArchetypeFile { get; set; }
                public static int EquipmentFile { get; set; }
                public static int EffectExplosionsFile { get; set; }

                public static List<FreelancerModStudio.Data.Template.File> Files
                {
                    get => data.Data.Files;
                    set => data.Data.Files = value;
                }

                // public static FreelancerModStudio.Data.Template.CostumTypes CostumTypes
                // {
                // get { return data.Data.CostumTypes; }
                // set { data.Data.CostumTypes = value; }
                // }
                public static int GetIndex(string file)
                {
                    for (int i = 0; i < Files.Count; ++i)
                    {
                        foreach (string path in Files[i].Paths)
                        {
                            string pattern = ".*" + path.Replace("\\", "\\\\").Replace("*", "[^\\\\]*");
                            if (Regex.Match(file, pattern, RegexOptions.IgnoreCase).Success)
                            {
                                return i;
                            }
                        }
                    }

                    return -1;
                }

                public static string GetDataPath(string filePath, int fileTemplate)
                {
                    // return if invalid file template or template path
                    if (fileTemplate < 0 ||
                        fileTemplate > Files.Count - 1 ||
                        Files[fileTemplate].Paths == null ||
                        Files[fileTemplate].Paths.Count == 0)
                    {
                        return null;
                    }

                    string[] directories = Files[fileTemplate].Paths[0].Split(new[] { Path.DirectorySeparatorChar });
                    StringBuilder builder = new StringBuilder(filePath);
                    for (int i = 0; i < directories.Length; ++i)
                    {
                        int lastIndex = builder.ToString().LastIndexOf(Path.DirectorySeparatorChar);
                        if (lastIndex == -1)
                        {
                            break;
                        }

                        builder.Remove(lastIndex, builder.Length - lastIndex);
                    }

                    return builder.ToString();
                }

                public static void SetSpecialFiles()
                {
                    int count = 7;

                    for (int i = 0; i < Files.Count && count > 0; ++i)
                    {
                        switch (Files[i].Name.ToLowerInvariant())
                        {
                            case "system":
                                SystemFile = i;
                                count--;
                                break;
                            case "universe":
                                UniverseFile = i;
                                count--;
                                break;
                            case "solar archetype":
                                SolarArchetypeFile = i;
                                count--;
                                break;
                            case "solar asteroid archetype":
                                AsteroidArchetypeFile = i;
                                count--;
                                break;
                            case "ship archetype":
                                ShipArchetypeFile = i;
                                count--;
                                break;
                            case "equipment":
                                EquipmentFile = i;
                                count--;
                                break;
                            case "effect explosions":
                                EffectExplosionsFile = i;
                                count--;
                                break;
                        }
                    }
                }
            }
        }

        internal struct Settings
        {
            public static Data.Settings Data;

            public static void Save()
            {
                // Remove all templates that are manually created
                Data.Data.General.Templates.Templates = Data.Data.General.Templates.Templates.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.TemplateIndex > 0).ToList();
                LoadTemplates();

                string file = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.SettingsPath);
                try
                {
                    string directory = Path.GetDirectoryName(file);
                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    Data.Save(file);
                }
                catch (IOException ex)
                {
                    Exceptions.Show(string.Format(Strings.SettingsSaveException, Resources.SettingsPath), ex);
                }
            }

            public static void Load()
            {
                string file = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.SettingsPath);
                Data = new Data.Settings();

                if (File.Exists(file))
                {
                    try
                    {
                        Data.Load(file);
                    }
                    catch (IOException ex)
                    {
                        Exceptions.Show(string.Format(Strings.SettingsLoadException, Resources.SettingsPath), ex);
                    }
                }

                // check for valid data
                Data.Data.General.CheckVersion();
                Data.Data.General.CheckValidData();
                SharedGeometries.LoadColors(Data.Data.General.ColorBox);

                if (Data.Data.General.AutoUpdate.UpdateFile == @"http://freelancermodstudio.googlecode.com/svn/trunk/updates.txt")
                    Data.Data.General.AutoUpdate.UpdateFile = @"https://raw.githubusercontent.com/AftermathFreelancer/FLModStudio/master/updates.txt";
            }

            public static void LoadTemplates()
            {
                // Form is closing
                if (FrmTableEditor.Instance.IsDisposed)
                    return;

                ToolStripMenuItem add = (ToolStripMenuItem)FrmTableEditor.Instance.TableContextMenu.Items["mnuAdd"];
                ToolStripMenuItem items = (ToolStripMenuItem)add.DropDownItems["mnuFromTemplate"];
                items.DropDownItems.Clear();
                foreach (var template in Settings.Data.Data.General.Templates.Templates)
                {
                    var item = new ToolStripMenuItem();
                    item.Click += FrmTableEditor.Instance.AddTemplate;
                    item.Tag = template.TemplateIndex;
                    item.Text = template.Options.First(x => x.Name == "nickname").Values.First().ToString();
                    items.DropDownItems.Add(item);
                }
            }

            public static string ShortLanguage
            {
                get
                {
                    if (Data.Data.General.Language == LanguageType.German)
                    {
                        return "de";
                    }

                    return "en";
                }

                set
                {
                    if (value.Equals("de", StringComparison.OrdinalIgnoreCase))
                    {
                        Data.Data.General.Language = LanguageType.German;
                    }
                    else
                    {
                        Data.Data.General.Language = LanguageType.English;
                    }
                }
            }
        }

        internal struct Thread
        {
            public static void Start(ref System.Threading.Thread thread, ThreadStart threadDelegate, ThreadPriority priority, bool isBackground)
            {
                Abort(ref thread, true);

                thread = new System.Threading.Thread(threadDelegate)
                    {
                        Priority = priority,
                        IsBackground = isBackground
                    };
                thread.Start();
            }

            public static void Abort(ref System.Threading.Thread thread, bool wait)
            {
                if (IsRunning(ref thread))
                {
                    thread.Abort();

                    if (wait)
                    {
                        thread.Join();
                    }
                }
            }

            public static bool IsRunning(ref System.Threading.Thread thread)
            {
                return (thread != null && thread.IsAlive);
            }
        }

        internal struct Compare
        {
            public static bool Size(Point checkSize, Point currentSize, bool bigger)
            {
                return Size(new Size(checkSize.X, checkSize.Y), new Size(currentSize.X, currentSize.Y), bigger);
            }

            public static bool Size(Size checkSize, Size currentSize, bool bigger)
            {
                if (bigger)
                {
                    return (checkSize.Width >= currentSize.Width && checkSize.Height >= currentSize.Height);
                }

                return (checkSize.Width <= currentSize.Width && checkSize.Height <= currentSize.Height);
            }
        }

        internal struct String
        {
            public static readonly StringBuilder StringBuilder = new StringBuilder();
        }

        internal class Exceptions
        {
            public static void Show(Exception exception)
            {
                MessageBox.Show(ExceptionUtils.Get(exception),  AssemblyUtils.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public static void Show(string errorDescription, Exception exception)
            {
                MessageBox.Show(
                    errorDescription + Environment.NewLine + Environment.NewLine + ExceptionUtils.Get(exception),
                     AssemblyUtils.Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
    }
}
