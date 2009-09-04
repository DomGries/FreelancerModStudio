using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreelancerModStudio
{
    class Helper
    {
        public struct Program
        {
            public static void Start()
            {
#if DEBUG
                System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
                st.Start();
#endif
                //load settings
                Settings.Load();
                Template.Load();
#if DEBUG
                st.Stop();
                System.Diagnostics.Debug.WriteLine("loading settings.xml and template.xml: " + st.ElapsedMilliseconds + "ms");
#endif

                //whidbey color table (gray colors of menustrip and tabstrip)
                ProfessionalColorTable whidbeyColorTable = new ProfessionalColorTable();
                whidbeyColorTable.UseSystemColors = true;
                ToolStripManager.Renderer = new ToolStripProfessionalRenderer(whidbeyColorTable);

                if (Settings.Data.Data.General.AutoUpdate.Update.Downloaded)
                {
                    //install update
                    AutoUpdate.AutoUpdate autoUpdate = new AutoUpdate.AutoUpdate("", "", "", null);
                    if (autoUpdate.Install())
                        return;
                }

                //remove installed update
                if (Settings.Data.Data.General.AutoUpdate.Update.Installed)
                    AutoUpdate.AutoUpdate.RemoveUpdate();

                //download update
                if (Settings.Data.Data.General.AutoUpdate.Enabled && Settings.Data.Data.General.AutoUpdate.UpdateFile != null && Settings.Data.Data.General.AutoUpdate.LastCheck.Date.AddDays(Settings.Data.Data.General.AutoUpdate.CheckInterval) <= DateTime.Now.Date)
                    Update.BackgroundCheck();

                //start main form
                Application.Run(new frmMain());

                //save settings
                Helper.Settings.Save();
            }
        }

        public struct Update
        {
            public static AutoUpdate.AutoUpdate AutoUpdate = new FreelancerModStudio.AutoUpdate.AutoUpdate();

            public static void BackgroundCheck()
            {
                //download in thread with lowest performance
                System.Threading.Thread autoUpdateThread = new System.Threading.Thread(new System.Threading.ThreadStart(delegate
                {
                    Check(true, Settings.Data.Data.General.AutoUpdate.SilentDownload);
                }));
                autoUpdateThread.Priority = System.Threading.ThreadPriority.Highest;
                autoUpdateThread.IsBackground = true;
                autoUpdateThread.Start();
            }

            public static void Check(bool silentCheck, bool silentDownload)
            {
                string proxy = "";
                string username = "";
                string password = "";

                if (Settings.Data.Data.General.AutoUpdate.Proxy.Enabled)
                {
                    proxy = Settings.Data.Data.General.AutoUpdate.Proxy.Uri;
                    username = Settings.Data.Data.General.AutoUpdate.Proxy.Username;
                    password = Settings.Data.Data.General.AutoUpdate.Proxy.Password;
                }

                AutoUpdate.CheckFileUri = new Uri(Helper.Settings.Data.Data.General.AutoUpdate.UpdateFile);
                AutoUpdate.SilentCheck = silentCheck;
                AutoUpdate.SilentDownload = silentDownload;
                AutoUpdate.SetProxy(proxy);
                AutoUpdate.SetCredentials(username, password);

                AutoUpdate.Check();
            }
        }

        public struct Template
        {
            private static FreelancerModStudio.Settings.Template data;
            private static int selectedLanguage = -1;

            public static void Load()
            {
                string File = System.IO.Path.Combine(Application.StartupPath, Properties.Resources.TemplatePath);
                data = new FreelancerModStudio.Settings.Template();

                try
                {
                    data.Load(File);
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.TemplateLoadException, Properties.Resources.TemplatePath), ex));
                    Environment.Exit(0);
                }

                //set selected language
                for (int i = 0; i < data.Data.Languages.Count; i++)
                {
                    if (data.Data.Languages[i].ID.ToLower() == Settings.GetShortLanguage())
                    {
                        selectedLanguage = i;
                        break;
                    }
                }
            }

            public struct Data
            {
                public static FreelancerModStudio.Settings.Template.Files Files
                {
                    get { return data.Data.Files; }
                    set { data.Data.Files = value; }
                }

                public static FreelancerModStudio.Settings.Template.Language Language
                {
                    get
                    {
                        if (selectedLanguage != -1)
                            return data.Data.Languages[selectedLanguage];
                        else
                            return null;
                    }
                    set
                    {
                        if (selectedLanguage != -1)
                            data.Data.Languages[selectedLanguage] = value;
                    }
                }

                public static FreelancerModStudio.Settings.Template.CostumTypes CostumTypes
                {
                    get { return data.Data.CostumTypes; }
                    set { data.Data.CostumTypes = value; }
                }
            }
        }

        public struct Settings
        {
            public static FreelancerModStudio.Settings.Settings Data;

            public static void Save()
            {
                try
                {
                    Data.Save(System.IO.Path.Combine(Application.StartupPath, Properties.Resources.SettingsPath));
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.SettingsSaveException, Properties.Resources.SettingsPath), ex));
                }
            }

            public static void Load()
            {
                string File = System.IO.Path.Combine(Application.StartupPath, Properties.Resources.SettingsPath);
                Data = new FreelancerModStudio.Settings.Settings();

                if (System.IO.File.Exists(File))
                {
                    try
                    {
                        Data.Load(File);
                    }
                    catch (Exception ex)
                    {
                        Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.SettingsLoadException, Properties.Resources.SettingsPath), ex));
                    }
                }
            }

            public static string GetShortLanguage()
            {
                if (Data.Data.General.Language == FreelancerModStudio.Settings.LanguageType.German)
                    return "de";

                return "en";
            }

            public static void SetShortLanguage(string language)
            {
                if (language.ToLower() == "de")
                    Data.Data.General.Language = FreelancerModStudio.Settings.LanguageType.German;
                else
                    Data.Data.General.Language = FreelancerModStudio.Settings.LanguageType.English;
            }
        }

        public struct Thread
        {
            public static void Start(ref System.Threading.Thread thread, System.Threading.ThreadStart threadDelegate, System.Threading.ThreadPriority priority, bool isBackground)
            {
                Abort(ref thread, true);

                thread = new System.Threading.Thread(threadDelegate);
                thread.Priority = priority;
                thread.IsBackground = isBackground;

                thread.Start();
            }

            public static void Abort(ref System.Threading.Thread thread, bool wait)
            {
                if (IsRunning(ref thread))
                {
                    thread.Abort();

                    if (wait)
                        thread.Join();
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
                    return (checkSize.Width >= currentSize.Width && checkSize.Height >= currentSize.Height);
                else
                    return (checkSize.Width <= currentSize.Width && checkSize.Height <= currentSize.Height);
            }
        }

        public struct Exceptions
        {
            public static void Show(Exception exception)
            {
                MessageBox.Show(Get(exception), Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            private static string Get(Exception exception)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(exception.Message);

                if (exception.InnerException != null)
                    stringBuilder.Append(Environment.NewLine + Environment.NewLine + Get(exception.InnerException).ToString());

                return stringBuilder.ToString();
            }
        }

        public struct Assembly
        {
            public static string Title
            {
                get
                {
                    // Alle Title-Attribute in dieser Assembly abrufen
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
                    // Wenn mindestens ein Title-Attribut vorhanden ist
                    if (attributes.Length > 0)
                    {
                        // Erstes auswählen
                        System.Reflection.AssemblyTitleAttribute titleAttribute = (System.Reflection.AssemblyTitleAttribute)attributes[0];
                        // Zurückgeben, wenn es keine leere Zeichenfolge ist
                        if (titleAttribute.Title != "")
                            return titleAttribute.Title;
                    }
                    // Wenn kein Title-Attribut vorhanden oder das Title-Attribut eine leere Zeichenfolge war, den EXE-Namen zurückgeben
                    return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                }
            }

            public static Version Version
            {
                get
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                }
            }

            public static string Description
            {
                get
                {
                    // Alle Description-Attribute in dieser Assembly abrufen
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
                    // Eine leere Zeichenfolge zurückgeben, wenn keine Description-Attribute vorhanden sind
                    if (attributes.Length == 0)
                        return "";
                    // Den Wert des Description-Attributs zurückgeben, wenn eines vorhanden ist
                    return ((System.Reflection.AssemblyDescriptionAttribute)attributes[0]).Description;
                }
            }

            public static string Product
            {
                get
                {
                    // Alle Product-Attribute in dieser Assembly abrufen
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                    // Eine leere Zeichenfolge zurückgeben, wenn keine Product-Attribute vorhanden sind
                    if (attributes.Length == 0)
                        return "";
                    // Den Wert des Product-Attributs zurückgeben, wenn eines vorhanden ist
                    return ((System.Reflection.AssemblyProductAttribute)attributes[0]).Product;
                }
            }

            public static string Copyright
            {
                get
                {
                    // Alle Copyright-Attribute in dieser Assembly abrufen
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
                    // Eine leere Zeichenfolge zurückgeben, wenn keine Copyright-Attribute vorhanden sind
                    if (attributes.Length == 0)
                        return "";
                    // Den Wert des Copyright-Attributs zurückgeben, wenn eines vorhanden ist
                    return ((System.Reflection.AssemblyCopyrightAttribute)attributes[0]).Copyright;
                }
            }

            public static string Company
            {
                get
                {
                    // Alle Company-Attribute in dieser Assembly abrufen
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);
                    // Eine leere Zeichenfolge zurückgeben, wenn keine Company-Attribute vorhanden sind
                    if (attributes.Length == 0)
                        return "";
                    // Den Wert des Company-Attributs zurückgeben, wenn eines vorhanden ist
                    return ((System.Reflection.AssemblyCompanyAttribute)attributes[0]).Company;
                }
            }
        }
    }
}