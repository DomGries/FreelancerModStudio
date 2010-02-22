using System;
using System.Collections.Generic;
using System.Text;
using FreelancerModStudio.Data;
using System.IO;
using System.Windows;

namespace FreelancerModStudio
{
    class Helper
    {
        public struct Template
        {
            static FreelancerModStudio.Data.Template data;

            public static void Load()
            {
                string File = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template.xml");
                data = new FreelancerModStudio.Data.Template();

                try
                {
                    data.Load(File);
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(ex);
                    Environment.Exit(0);
                }
            }

            public struct Data
            {
                public static FreelancerModStudio.Data.Template.Files Files
                {
                    get { return data.Data.Files; }
                    set { data.Data.Files = value; }
                }
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

        public struct Exceptions
        {
            public static void Show(Exception exception)
            {
                MessageBox.Show(Get(exception), Assembly.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            static string Get(Exception exception)
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
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
                    if (attributes.Length > 0)
                    {
                        System.Reflection.AssemblyTitleAttribute titleAttribute = (System.Reflection.AssemblyTitleAttribute)attributes[0];

                        if (titleAttribute.Title != "")
                            return titleAttribute.Title;
                    }

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
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
                    if (attributes.Length == 0)
                        return "";

                    return ((System.Reflection.AssemblyDescriptionAttribute)attributes[0]).Description;
                }
            }

            public static string Product
            {
                get
                {
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), false);
                    if (attributes.Length == 0)
                        return "";

                    return ((System.Reflection.AssemblyProductAttribute)attributes[0]).Product;
                }
            }

            public static string Copyright
            {
                get
                {
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
                    if (attributes.Length == 0)
                        return "";

                    return ((System.Reflection.AssemblyCopyrightAttribute)attributes[0]).Copyright;
                }
            }

            public static string Company
            {
                get
                {
                    object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyCompanyAttribute), false);
                    if (attributes.Length == 0)
                        return "";

                    return ((System.Reflection.AssemblyCompanyAttribute)attributes[0]).Company;
                }
            }
        }
    }
}