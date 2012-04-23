using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using FreelancerModStudio.Properties;

namespace FreelancerModStudio.AutoUpdate
{
    public class AutoUpdate
    {
        public Uri CheckFileUri { get; set; }
        public bool SilentDownload { get; set; }
        public bool SilentCheck { get; set; }

        UpdateInformation _updateInfo;

        StatusType _status = StatusType.Waiting;
        frmAutoUpdate _updaterForm;

        readonly WebClient _webClient = new WebClient();

        public event EventHandler<CancelEventArgs> RestartingApplication;

        delegate void ProgressChangedInvoker(long bytes, long bytesTotal, int percent);
        delegate void SetStatusInvoker(StatusType status);

        public AutoUpdate()
        {
        }

        public AutoUpdate(string proxy, string username, string password)
        {
            SetCredentials(username, password);
            SetProxy(proxy);
        }

        public void Check()
        {
            if (_status == StatusType.Waiting)
            {
                _status = StatusType.Checking;

                //display checking form
                if (!SilentCheck)
                {
                    SetPage(_status, false);
                }

                //set event handlers
                _webClient.DownloadStringCompleted += Download_CheckFile_Completed;

                //download the checkfile
                _webClient.DownloadStringAsync(CheckFileUri);
            }
            else
            {
                if (!SilentCheck)
                {
                    SetPage(_status, false);
                }
            }
        }

        void UpdateAviable(bool value)
        {
            if (value)
            {
                if (SilentDownload && SilentCheck)
                {
                    DownloadUpdate();
                }
                else
                {
                    _status = StatusType.UpdateAvailable;
                    SetPage(_status, false);
                }
            }
            else
            {
                _status = StatusType.UpdateNotAvailable;
                if (!SilentCheck)
                {
                    SetPage(_status, false);
                }
            }
        }

        void SetPage(StatusType value, bool wait)
        {
            if (_updaterForm == null || _updaterForm.IsDisposed || !_updaterForm.IsHandleCreated)
            {
                _updaterForm = new frmAutoUpdate();
                _updaterForm.ActionRequired += AutoUpdateForm_ActionRequired;
                _updaterForm.SetPage(value);

                if (wait)
                {
                    _updaterForm.ShowDialog();
                }
                else
                {
                    _updaterForm.Show();
                }
            }
            else
            {
                _updaterForm.Invoke(new SetStatusInvoker(_updaterForm.SetPage), value);
            }
        }

        bool IsNewer(string fileContent)
        {
            try
            {
                _updateInfo = UpdateInformationParser.Parse(fileContent);
                return _updateInfo.Version > Helper.Assembly.Version;
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
            _status = StatusType.Downloading;

            //display download form
            if (!SilentDownload || !SilentCheck)
            {
                SetPage(_status, false);
            }

            string destPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.UpdateDownloadPath);
            string destFile = Path.Combine(destPath, Path.GetFileName(_updateInfo.FileUri.AbsolutePath));

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
                    UpdateAviable(IsNewer(e.Result));
                }
                else
                {
                    //exception occured while downloading
                    _status = StatusType.Waiting;

                    if (!SilentCheck)
                    {
                        Helper.Exceptions.Show(new Exception(String.Format(Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                    }
                }
            }
            else
            {
                _status = StatusType.Waiting;
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
                    _status = StatusType.DownloadFinished;
                    SetPage(_status, SilentCheck);

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(_updateInfo.FileUri.AbsolutePath);
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = _updateInfo.Silent;
                }
                else
                {
                    //exception occured while downloading
                    _status = StatusType.UpdateAvailable;

                    if (!SilentCheck)
                    {
                        Helper.Exceptions.Show(new Exception(String.Format(Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                    }
                }
            }
            else
            {
                _status = StatusType.UpdateAvailable;
            }
        }

        void Download_Update_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!SilentDownload && _updaterForm != null && !_updaterForm.IsDisposed && _updaterForm.IsHandleCreated)
            {
                _updaterForm.BeginInvoke(new ProgressChangedInvoker(_updaterForm.ChangeProgress), new object[] {e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage});
            }
        }

        void Abort()
        {
            _webClient.CancelAsync();
        }

        void AutoUpdateForm_ActionRequired(ActionType action)
        {
            switch (action)
            {
                case ActionType.Abort:
                    Abort();
                    break;
                case ActionType.Download:
                    DownloadUpdate();
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

        public void SetCredentials(string username, string password)
        {
            _webClient.Credentials = string.IsNullOrEmpty(username) ? null : new NetworkCredential(username, password);
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
                Directory.Delete(Path.GetDirectoryName(file));
            }
            catch
            {
            }

            Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = null;
            Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
        }
    }
}
