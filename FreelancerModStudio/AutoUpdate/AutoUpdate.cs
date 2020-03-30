namespace FreelancerModStudio.AutoUpdate
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Windows.Forms;

    using FreelancerModStudio.Properties;

    public class AutoUpdate : IDisposable
    {
        public StatusType Status { get; set; }

        public Uri CheckFileUri { get; set; }
        public bool SilentDownload { get; set; }
        public bool SilentCheck { get; set; }

        private UpdateInformation updateInfo;

        private IAutoUpdateUi ui;

        private readonly WebClient webClient = new WebClient();

        public AutoUpdate()
        {
            this.Status = StatusType.Waiting;
        }

        public event EventHandler<CancelEventArgs> RestartingApplication;

        public void Check()
        {
            if (this.Status != StatusType.Waiting)
            {
                return;
            }

            this.Status = StatusType.Checking;

            // display checking form
            if (!this.SilentCheck)
            {
                this.ShowUi();
            }

            // set event handlers
            this.webClient.DownloadStringCompleted += this.DownloadCheckFileCompleted;

            // download the checkfile
            this.webClient.DownloadStringAsync(this.CheckFileUri);
        }

        public void ShowUi()
        {
            // start UI in new thread as this function is always called from main thread
            new Thread(() => this.SetPage(this.Status)).Start();
        }

        private void UpdateAvailable(bool value)
        {
            if (value)
            {
                if (this.SilentDownload && this.SilentCheck)
                {
                    this.DownloadUpdate();
                }
                else
                {
                    this.Status = StatusType.UpdateAvailable;
                    this.SetPage(this.Status);
                }
            }
            else
            {
                this.Status = StatusType.UpdateNotAvailable;
                if (!this.SilentCheck)
                {
                    this.SetPage(this.Status);
                }
            }
        }

        private void SetPage(StatusType value)
        {
            if (this.ui == null)
            {
                this.ui = new FrmAutoUpdate();
                this.ui.ActionRequired += this.UiActionRequired;
                this.ui.SetPage(value, false);
                this.ui.ShowUi();
            }
            else
            {
                this.ui.SetPage(value, true);
            }
        }

        private bool IsNewer(string fileContent)
        {
            try
            {
                this.updateInfo = UpdateInformationParser.Parse(fileContent);
                if (this.updateInfo != null)
                {
                    return this.updateInfo.Version > Helper.Assembly.Version;
                }
            }
            catch (Exception ex)
            {
                if (!this.SilentCheck)
                {
                    Helper.Exceptions.Show(ex);
                }
            }

            return false;
        }

        private void DownloadUpdate()
        {
            this.Status = StatusType.Downloading;

            // display download form
            if (!this.SilentDownload || !this.SilentCheck)
            {
                this.SetPage(this.Status);
            }

            string destPath = GetUpdateDirectory();
            string destFile = Path.Combine(destPath, this.GetUpdateFileName());

            // create update directory if not existing
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // set event handlers
            this.webClient.DownloadProgressChanged += this.DownloadUpdateProgressChanged;
            this.webClient.DownloadFileCompleted += this.DownloadUpdateCompleted;

            // download the update
            this.webClient.DownloadFileAsync(this.updateInfo.FileUri, destFile);
        }

        private void DownloadCheckFileCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // delete event handlers
            this.webClient.DownloadStringCompleted -= this.DownloadCheckFileCompleted;

            if (!e.Cancelled)
            {
                Helper.Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;

                if (e.Error == null)
                {
                    // check if file is newer
                    this.UpdateAvailable(this.IsNewer(e.Result));
                }
                else
                {
                    // exception occured while downloading
                    this.Status = StatusType.Waiting;

                    if (!this.SilentCheck)
                    {
                        Helper.Exceptions.Show(string.Format(Strings.UpdatesDownloadException, Helper.Assembly.Name), e.Error);
                    }
                }
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

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = this.GetUpdateFileName();
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = this.updateInfo.Silent;

                    this.SetPage(this.Status);
                }
                else
                {
                    // exception occured while downloading
                    this.Status = StatusType.UpdateAvailable;

                    if (!this.SilentCheck)
                    {
                        Helper.Exceptions.Show(string.Format(Strings.UpdatesDownloadException, Helper.Assembly.Name), e.Error);
                    }
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
                    InstallUpdate(this.RestartingApplication);
                    break;
            }
        }

        private string GetUpdateFileName()
        {
            return Path.GetFileName(this.updateInfo.FileUri.AbsolutePath) ?? "Update.exe";
        }

        private static string GetUpdateDirectory()
        {
            return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.UpdateDownloadPath);
        }

        public void SetProxy(string proxy)
        {
            this.webClient.Proxy = string.IsNullOrEmpty(proxy) ? null : new WebProxy(proxy);
        }

        public void SetCredentials(string userName, string password)
        {
            this.webClient.Credentials = string.IsNullOrEmpty(userName) ? null : new NetworkCredential(userName, password);
        }

        public static bool InstallUpdate()
        {
            return InstallUpdate(null);
        }

        public static bool InstallUpdate(EventHandler<CancelEventArgs> restartingApplication)
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName == null)
            {
                return false;
            }

            string file = Path.Combine(GetUpdateDirectory(), Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName);

            // start file (setup) and exit app
            if (File.Exists(file))
            {
                if (restartingApplication != null)
                {
                    CancelEventArgs cancelEventArgs = new CancelEventArgs();
                    restartingApplication(null, cancelEventArgs);
                    if (cancelEventArgs.Cancel)
                    {
                        return false;
                    }
                }

                Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = false;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = true;

                // save settings because application was already closed except for this thread
                Helper.Settings.Save();

                string arguments = string.Empty;
                if (Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall)
                {
                    arguments = "/SILENT";
                }

                Process.Start(file, arguments);
                return true;
            }

            return false;
        }

        public static void RemoveUpdate()
        {
            // don't throw an exception if update file or the directory can't be removed
            try
            {
                string directory = GetUpdateDirectory();
                if (directory != null && Directory.Exists(directory))
                {
                    string file = Path.Combine(directory, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName);
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }

                    Directory.Delete(directory);
                }
            }

            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // ReSharper restore EmptyGeneralCatchClause
            }

            Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = null;
            Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
        }

        public void Dispose()
        {
            this.webClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
