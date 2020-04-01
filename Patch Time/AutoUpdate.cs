namespace PatchTime
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml.Serialization;

    using SharpCompress.Common;
    using SharpCompress.Readers;

    public class AutoUpdate : IDisposable
    { 
        public event EventHandler<CancelEventArgs> RestartingApplication;
        internal Uri CheckFileUri { get; set; }
        internal bool AlertOnFailure { get; set; } = false;

        private UpdateInformation updateInfo;
        private IAutoUpdateUi ui;
        private StatusType Status { get; set; }
        private readonly WebClient webClient = new WebClient();

        public void SetProxy(string proxy) => this.webClient.Proxy = string.IsNullOrEmpty(proxy) ? null : new WebProxy(proxy);
        public void SetCredentials(string userName, string password) => this.webClient.Credentials = string.IsNullOrEmpty(userName) ? null : new NetworkCredential(userName, password);

        public void Dispose()
        {
            this.webClient.Dispose();
            GC.SuppressFinalize(this);
        }

        internal AutoUpdate() => this.Status = StatusType.Waiting;

        internal void Check()
        {
            if (this.Status != StatusType.Waiting)
                return;

            this.Status = StatusType.Checking;
            this.ShowUi();

            // set event handlers
            this.webClient.DownloadStringCompleted += this.DownloadCheckFileCompleted;

            // download the checkfile
            this.webClient.DownloadStringAsync(this.CheckFileUri);
        }

        private void ShowUi() => new Thread(() => this.SetPage(this.Status)).Start();

        private void UpdateAvailable(bool value)
        {
            if (value)
            {
                this.Status = StatusType.UpdateAvailable;
                this.SetPage(this.Status);
                return;
            }

            this.Status = StatusType.UpdateNotAvailable;
            string a = new Version(1, 2, 0, 1).ToString();
            Environment.Exit(0);
        }

        private void SetPage(StatusType value)
        {
            if (this.ui is null)
            {
                this.ui = new FrmAutoUpdate();
                this.ui.ActionRequired += this.UiActionRequired;
                this.ui.SetPage(value, false);
                this.ui.ShowUi();
                return;
            }

            this.ui.SetPage(value, true);
        }

        private bool IsNewer(string fileContent)
        {
            try
            {
                using (StringReader textReader = new StringReader(fileContent))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(UpdateInformation));
                    this.updateInfo = (UpdateInformation)serializer.Deserialize(textReader);
                }
                if (this.updateInfo != null)
                    return this.updateInfo.Version > Program.Version;
            }

            catch (Exception ex)
            {
                Ping myPing = new Ping();
                string host = "google.com";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status != IPStatus.Success)
                    return false; // Their internet is down, so don't mention any issues

                else
                    MessageBox.Show(
                        "Unable to download updates. Please ensure the download URL is valid and reachable. \n\nError Code: "
                        + ex.Message,
                        "Download Failed.",
                        MessageBoxButtons.OK);
            }

            return false;
        }

        private void DownloadUpdate()
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName("FLModStudio"))
                    proc.Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Unable to update. Cannot close FLModStudio.\nAdditional Information: " + ex.Message);
                Environment.Exit(1);
            }
            this.Status = StatusType.Downloading;

            // display download form
            this.SetPage(this.Status);

            string destPath = Directory.GetCurrentDirectory();
            string destFile = Path.Combine(destPath, this.GetUpdateFileName());

            // set event handlers
            this.webClient.DownloadProgressChanged += this.DownloadUpdateProgressChanged;
            this.webClient.DownloadFileCompleted += this.DownloadUpdateCompleted;

            // download the update
            this.webClient.DownloadFileAsync(new Uri(this.updateInfo.FileUri), destFile);
        }

        private void DownloadCheckFileCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // delete event handlers
            this.webClient.DownloadStringCompleted -= this.DownloadCheckFileCompleted;

            if (!e.Cancelled)
            {
                if (e.Error is null)
                {
                    // check if file is newer
                    this.UpdateAvailable(this.IsNewer(e.Result));
                    return;
                }

                // exception occured while downloading
                this.Status = StatusType.Waiting;
                if (this.AlertOnFailure)
                    MessageBox.Show("Unable to download check file. \n" + e.Error.Message);
                Environment.Exit(1);
            }
            else
            {
                this.Status = StatusType.Waiting;
            }
        }

        private void DownloadUpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // delete event handlers
            this.webClient.DownloadProgressChanged -= this.DownloadUpdateProgressChanged;
            this.webClient.DownloadFileCompleted -= this.DownloadUpdateCompleted;

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    // download update completed
                    this.Status = StatusType.DownloadFinished;
                    using (Stream stream = File.OpenRead(Directory.GetCurrentDirectory() + "\\" + this.GetUpdateFileName()))
                    {
                        IReader reader = ReaderFactory.Open(stream);
                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                                reader.WriteEntryToDirectory(Directory.GetCurrentDirectory(), new ExtractionOptions() { Overwrite = true, ExtractFullPath = true });
                        }
                        reader.Dispose();
                        stream.Dispose();
                        File.Delete(Directory.GetCurrentDirectory() + "\\" + this.GetUpdateFileName());
                    }

                    this.SetPage(this.Status);
                }
                else
                {
                    // exception occured while downloading
                    MessageBox.Show("Unknown error occured.\n" + e.Error.Message);
                    this.Status = StatusType.UpdateAvailable;
                    Environment.Exit(1);
                }
            }
            else
            {
                this.Status = StatusType.UpdateAvailable;
            }
        }

        private void DownloadUpdateProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.ui?.SetProgress(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
        }

        private void Abort()
        {
            this.webClient.CancelAsync();
        }

        private void UiActionRequired(ActionType action)
        {
            switch (action)
            {
                case ActionType.Close:
                    this.ui = null;
                    break;
                case ActionType.CloseAndAbort:
                    this.ui = null;
                    this.Abort();
                    break;
                case ActionType.Download:
                    // start download in new thread to prevent cancel on UI close
                    new Thread(this.DownloadUpdate).Start();
                    break;
                case ActionType.Install:
                    Process.Start("FLModStudio.exe");
                    Environment.Exit(0);
                    break;
            }
        }

        private string GetUpdateFileName() => Path.GetFileName(this.updateInfo.FileUri);
    }
}
