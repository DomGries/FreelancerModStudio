using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using FreelancerModStudio.Properties;

namespace FreelancerModStudio.AutoUpdate
{
    public class AutoUpdate : IDisposable
    {
        public StatusType Status { get; set; }

        public Uri CheckFileUri { get; set; }
        public bool SilentDownload { get; set; }
        public bool SilentCheck { get; set; }

        UpdateInformation _updateInfo;
        IAutoUpdateUI _ui;

        readonly WebClient _webClient = new WebClient();

        public AutoUpdate()
        {
            Status = StatusType.Waiting;
        }

        public event EventHandler<CancelEventArgs> RestartingApplication;

        public void Check()
        {
            if (Status != StatusType.Waiting)
            {
                return;
            }

            Status = StatusType.Checking;

            //display checking form
            if (!SilentCheck)
            {
                ShowUI();
            }

            //set event handlers
            _webClient.DownloadStringCompleted += Download_CheckFile_Completed;

            //download the checkfile
            _webClient.DownloadStringAsync(CheckFileUri);
        }

        public void ShowUI()
        {
            //start UI in new thread as this function is always called from main thread
            new Thread(() => SetPage(Status)).Start();
        }

        void UpdateAvailable(bool value)
        {
            if (value)
            {
                if (SilentDownload && SilentCheck)
                {
                    DownloadUpdate();
                }
                else
                {
                    Status = StatusType.UpdateAvailable;
                    SetPage(Status);
                }
            }
            else
            {
                Status = StatusType.UpdateNotAvailable;
                if (!SilentCheck)
                {
                    SetPage(Status);
                }
            }
        }

        void SetPage(StatusType value)
        {
            if (_ui == null)
            {
                _ui = new frmAutoUpdate();
                _ui.ActionRequired += UI_ActionRequired;
                _ui.SetPage(value, false);
                _ui.ShowUI();
            }
            else
            {
                _ui.SetPage(value, true);
            }
        }

        bool IsNewer(string fileContent)
        {
            try
            {
                _updateInfo = UpdateInformationParser.Parse(fileContent);
                if (_updateInfo != null)
                {
                    return _updateInfo.Version > Helper.Assembly.Version;
                }
            }
            catch (Exception ex)
            {
                if (!SilentCheck)
                {
                    Helper.Exceptions.Show(ex);
                }
            }

            return false;
        }

        void DownloadUpdate()
        {
            Status = StatusType.Downloading;

            //display download form
            if (!SilentDownload || !SilentCheck)
            {
                SetPage(Status);
            }

            string destFileName = Path.GetFileName(_updateInfo.FileUri.AbsolutePath);
            if (destFileName == null)
            {
                destFileName = "Update.exe";
            }

            string destPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.UpdateDownloadPath);
            string destFile = Path.Combine(destPath, destFileName);

            //create update directory if not existing
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            //set event handlers
            _webClient.DownloadProgressChanged += Download_Update_ProgressChanged;
            _webClient.DownloadFileCompleted += Download_Update_Completed;

            //download the update
            _webClient.DownloadFileAsync(_updateInfo.FileUri, destFile);
        }

        void Download_CheckFile_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            //delete event handlers
            _webClient.DownloadStringCompleted -= Download_CheckFile_Completed;

            if (!e.Cancelled)
            {
                Helper.Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;

                if (e.Error == null)
                {
                    //check if file is newer
                    UpdateAvailable(IsNewer(e.Result));
                }
                else
                {
                    //exception occured while downloading
                    Status = StatusType.Waiting;

                    if (!SilentCheck)
                    {
                        Helper.Exceptions.Show(string.Format(Strings.UpdatesDownloadException, Helper.Assembly.Name), e.Error);
                    }
                }
            }
            else
            {
                Status = StatusType.Waiting;
            }
        }

        void Download_Update_Completed(object sender, AsyncCompletedEventArgs e)
        {
            //delete event handlers
            _webClient.DownloadProgressChanged -= Download_Update_ProgressChanged;
            _webClient.DownloadFileCompleted -= Download_Update_Completed;

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    //download update completed
                    Status = StatusType.DownloadFinished;

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(_updateInfo.FileUri.AbsolutePath);
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = _updateInfo.Silent;

                    SetPage(Status);
                }
                else
                {
                    //exception occured while downloading
                    Status = StatusType.UpdateAvailable;

                    if (!SilentCheck)
                    {
                        Helper.Exceptions.Show(string.Format(Strings.UpdatesDownloadException, Helper.Assembly.Name), e.Error);
                    }
                }
            }
            else
            {
                Status = StatusType.UpdateAvailable;
            }
        }

        void Download_Update_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_ui != null)
            {
                _ui.SetProgress(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
            }
        }

        void Abort()
        {
            _webClient.CancelAsync();
        }

        void UI_ActionRequired(ActionType action)
        {
            switch (action)
            {
                case ActionType.Close:
                    _ui = null;
                    break;
                case ActionType.CloseAndAbort:
                    _ui = null;
                    Abort();
                    break;
                case ActionType.Download:
                    //start download in new thread to prevent cancel on UI close
                    new Thread(DownloadUpdate).Start();
                    break;
                case ActionType.Install:
                    InstallUpdate(RestartingApplication);
                    break;
            }
        }

        public void SetProxy(string proxy)
        {
            _webClient.Proxy = string.IsNullOrEmpty(proxy) ? null : new WebProxy(proxy);
        }

        public void SetCredentials(string userName, string password)
        {
            _webClient.Credentials = string.IsNullOrEmpty(userName) ? null : new NetworkCredential(userName, password);
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

            string file = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Path.Combine(Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

            //start file (setup) and exit app
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

                //save settings because application was already closed except for this thread
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
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName == null)
            {
                return;
            }

            string file = Path.Combine(Application.StartupPath, Path.Combine(Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

            //don't throw an exception if update file or the directory can't be removed
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                string directory = Path.GetDirectoryName(file);
                if (directory != null)
                {
                    Directory.Delete(directory);
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }

            Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = null;
            Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
        }

        public void Dispose()
        {
            _webClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
