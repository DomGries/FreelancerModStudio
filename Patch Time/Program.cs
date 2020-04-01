namespace PatchTime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    internal static class Program
    {
        internal static Version Version;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        [STAThread]
        private static void Main(string[] args)
        {
            Regex cmdRegEx = new Regex(@"(?<name>.+?)='(?<val>.*)'");

            Dictionary<string, string> cmdArgs = new Dictionary<string, string>();
            foreach (string s in args)
            {
                Match m = cmdRegEx.Match(s);
                if (m.Success)
                    cmdArgs.Add(m.Groups[1].Value, m.Groups[2].Value is "null" ? null : m.Groups[2].Value);

                else
                    Environment.Exit(1); // I want good format : <<<<<
            }

            if (!cmdArgs.ContainsKey("version") || !cmdArgs.ContainsKey("uri") || !cmdArgs.ContainsKey("proxy") || !cmdArgs.ContainsKey("username") || !cmdArgs.ContainsKey("password") || !cmdArgs.ContainsKey("alert"))
                Environment.Exit(1);
                
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AutoUpdate update = new AutoUpdate();
            Version = Version.Parse(cmdArgs["version"]);
            update.CheckFileUri = new Uri(cmdArgs["uri"]);
            update.SetProxy(cmdArgs["proxy"]);
            update.SetCredentials(cmdArgs["username"], cmdArgs["password"]);
            update.AlertOnFailure = bool.Parse(cmdArgs["alert"]);

            update.Check();
        }
    }
}
