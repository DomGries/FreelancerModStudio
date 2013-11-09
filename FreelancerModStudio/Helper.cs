using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using FreelancerModStudio.AutoUpdate;
using FreelancerModStudio.Data;
using FreelancerModStudio.Properties;

namespace FreelancerModStudio
{
    internal static class Helper
    {
        public struct Program
        {
            public static void Start()
            {
#if DEBUG
                Stopwatch st = new Stopwatch();
                st.Start();
#endif
                //load settings
                Settings.Load();

                //install downloaded update if it exists
                if (Settings.Data.Data.General.AutoUpdate.Update.Downloaded)
                {
                    if (AutoUpdate.AutoUpdate.InstallUpdate())
                    {
                        return;
                    }
                }

                Template.Load();
#if DEBUG
                st.Stop();
                Debug.WriteLine("loading settings.xml and template.xml: " + st.ElapsedMilliseconds + "ms");
#endif

                //whidbey color table (gray colors of menustrip and tabstrip)
                ProfessionalColorTable whidbeyColorTable = new ProfessionalColorTable
                    {
                        UseSystemColors = true
                    };
                ToolStripManager.Renderer = new ToolStripProfessionalRenderer(whidbeyColorTable);

                //remove installed update if it exists
                if (Settings.Data.Data.General.AutoUpdate.Update.Installed)
                {
                    AutoUpdate.AutoUpdate.RemoveUpdate();
                }

                //check for update
                if (Settings.Data.Data.General.AutoUpdate.Enabled && Settings.Data.Data.General.AutoUpdate.UpdateFile != null && Settings.Data.Data.General.AutoUpdate.LastCheck.Date.AddDays(Settings.Data.Data.General.AutoUpdate.CheckInterval) <= DateTime.Now.Date)
                {
                    Update.Check(true, Settings.Data.Data.General.AutoUpdate.SilentDownload);
                }

                //start main form
                Application.Run(new frmMain());

                //save settings
                Settings.Save();
            }
        }

        public struct Update
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
                        MessageBox.Show(string.Format(Strings.UpdatesDownloadException, Assembly.Name), Assembly.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public struct Template
        {
            static FreelancerModStudio.Data.Template _data;

            public static void Load()
            {
                Load(Path.Combine(Application.StartupPath, Resources.TemplatePath));
            }

            public static void Load(string file)
            {
                _data = new FreelancerModStudio.Data.Template();

                try
                {
                    _data.Load(file);
                    Data.SetSpecialFiles();
                }
                catch (Exception ex)
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
                    get
                    {
                        return _data.Data.Files;
                    }
                    set
                    {
                        _data.Data.Files = value;
                    }
                }

                //public static FreelancerModStudio.Data.Template.CostumTypes CostumTypes
                //{
                //    get { return data.Data.CostumTypes; }
                //    set { data.Data.CostumTypes = value; }
                //}

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

        public struct Settings
        {
            public static Data.Settings Data;

            public static void Save()
            {
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
                catch (Exception ex)
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
                    catch (Exception ex)
                    {
                        Exceptions.Show(string.Format(Strings.SettingsLoadException, Resources.SettingsPath), ex);
                    }
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

        public struct Thread
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

        public struct Compare
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

        public struct String
        {
            public static readonly StringBuilder StringBuilder = new StringBuilder();
        }

        public struct Exceptions
        {
            public static void Show(Exception exception)
            {
                MessageBox.Show(Get(exception), Assembly.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public static void Show(string errorDescription, Exception exception)
            {
                MessageBox.Show(errorDescription + Environment.NewLine + Environment.NewLine + Get(exception), Assembly.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public static string Get(Exception exception)
            {
                StringBuilder stringBuilder = new StringBuilder(exception.Message);

                if (exception.InnerException != null)
                {
                    stringBuilder.Append(Environment.NewLine + Environment.NewLine + Get(exception.InnerException));
                }

                return stringBuilder.ToString();
            }
        }

        public struct Assembly
        {
            public static string Name
            {
                get
                {
                    return Application.ProductName;
                }
            }

            public static Version Version
            {
                get
                {
                    return new Version(Application.ProductVersion);
                }
            }

            public static string Company
            {
                get
                {
                    return Application.CompanyName;
                }
            }

            public static string Description
            {
                get
                {
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return string.Empty;
                    }

                    return ((AssemblyDescriptionAttribute)attributes[0]).Description;
                }
            }

            public static string Copyright
            {
                get
                {
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                    if (attributes.Length == 0)
                    {
                        return string.Empty;
                    }

                    return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
                }
            }
        }
    }
}
