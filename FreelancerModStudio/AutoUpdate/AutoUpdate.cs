using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace FreelancerModStudio.AutoUpdate
{
    public class AutoUpdate
    {
        Uri mUpdateFileUri;
        public Uri CheckFileUri { get; set; }

        bool mSilentSetup;
        public bool SilentDownload { get; set; }
        public bool SilentCheck { get; set; }
        StatusType mStatus = StatusType.Waiting;

        WebClient mWebClient;

        frmAutoUpdate mUpdaterForm;

        public event EventHandler<CancelEventArgs> RestartingApplication;

        protected virtual void OnRestartingApplication(CancelEventArgs e)
        {
            if (RestartingApplication != null)
                RestartingApplication(this, e);
        }

        delegate void ProgressChangedInvoker(long bytes, long bytesTotal, int percent);
        delegate void SetStatusInvoker(PageType status);

        public AutoUpdate()
        {
            mWebClient = new WebClient();
        }

        public AutoUpdate(string proxy, string username, string password, Uri checkFileUri)
        {
            mWebClient = new WebClient();

            SetCredentials(username, password);
            SetProxy(proxy);

            CheckFileUri = checkFileUri;
        }

        public void Check()
        {
            if (mStatus == StatusType.Waiting)
            {
                mStatus = StatusType.Checking;

                //display checking form
                if (!SilentCheck)
                    SetPage(PageType.Checking, false);

                //set event handlers
                mWebClient.DownloadStringCompleted += Download_CheckFile_Completed;

                //download the checkfile
                mWebClient.DownloadStringAsync(CheckFileUri);
            }
            else
            {
                if (!SilentCheck)
                {
                    if (mStatus == StatusType.Checking)
                        SetPage(PageType.Checking, false);

                    else if (mStatus == StatusType.UpdateAviable)
                        SetPage(PageType.Aviable, false);

                    else if (mStatus == StatusType.UpdateNotAviable)
                        SetPage(PageType.NotAviable, false);

                    else if (mStatus == StatusType.Downloading)
                        SetPage(PageType.Downloading, false);

                    else if (mStatus == StatusType.DownloadFinished)
                        SetPage(PageType.DownloadFinished, false);
                }
            }
        }

        void UpdateAviable(bool value)
        {
            if (value)
            {
                if (SilentDownload && SilentCheck)
                    DownloadUpdate();
                else
                {
                    mStatus = StatusType.UpdateAviable;
                    SetPage(PageType.Aviable, false);
                }
            }
            else
            {
                mStatus = StatusType.UpdateNotAviable;
                if (!SilentCheck)
                    SetPage(PageType.NotAviable, false);
            }
        }

        void SetPage(PageType value, bool wait)
        {
            if (mUpdaterForm == null || mUpdaterForm.IsDisposed || !mUpdaterForm.IsHandleCreated)
            {
                mUpdaterForm = new frmAutoUpdate();
                mUpdaterForm.ActionRequired += AutoUpdateForm_ActionRequired;
                mUpdaterForm.SetPage(value);

                if (wait)
                    mUpdaterForm.ShowDialog();
                else
                    mUpdaterForm.Show();
            }
            else
                mUpdaterForm.Invoke(new SetStatusInvoker(mUpdaterForm.SetPage), value);
        }

        bool IsNewer(string fileContent)
        {
            try
            {
                UpdateInformation updateInformation = UpdateInformationParser.Parse(fileContent);

                int[] updateVersion = {updateInformation.Version.Major, updateInformation.Version.Minor,
                                               updateInformation.Version.Build, updateInformation.Version.Revision};
                int[] currentVersion = {Helper.Assembly.Version.Major, Helper.Assembly.Version.Minor,
                                               Helper.Assembly.Version.Build, Helper.Assembly.Version.Revision};

                //check if version number is higher
                for (int index = 0; index < updateVersion.Length; index++)
                {
                    if (updateVersion[index] > currentVersion[index])
                    {
                        mUpdateFileUri = updateInformation.FileUri;
                        mSilentSetup = updateInformation.Silent;

                        return true;
                    }
                    if (updateVersion[index] < currentVersion[index])
                        return false;
                }
            }
            catch (Exception ex)
            {
                if (!SilentCheck)
                    Helper.Exceptions.Show(ex);
            }

            return false;
        }

        void DownloadUpdate()
        {
            mStatus = StatusType.Downloading;

            //display download form
            if (!SilentDownload || !SilentCheck)
                SetPage(PageType.Downloading, false);

            string destFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.Windows.Forms.Application.ProductName), Path.Combine(Properties.Resources.UpdateDownloadPath, Path.GetFileName(mUpdateFileUri.AbsolutePath)));

            //create update directory if not existing
            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            //set event handlers
            mWebClient.DownloadProgressChanged += Download_Update_ProgressChanged;
            mWebClient.DownloadFileCompleted += Download_Update_Completed;

            //download the update
            mWebClient.DownloadFileAsync(mUpdateFileUri, destFile);
        }

        void Download_CheckFile_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            //delete event handlers
            mWebClient.DownloadStringCompleted -= Download_CheckFile_Completed;

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
                    mStatus = StatusType.Waiting;

                    if (!SilentCheck)
                        Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                }
            }
            else
                mStatus = StatusType.Waiting;
        }

        void Download_Update_Completed(object sender, AsyncCompletedEventArgs e)
        {
            //delete event handlers
            mWebClient.DownloadProgressChanged -= Download_Update_ProgressChanged;
            mWebClient.DownloadFileCompleted -= Download_Update_Completed;

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    //download update completed
                    mStatus = StatusType.DownloadFinished;
                    SetPage(PageType.DownloadFinished, SilentCheck);

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(mUpdateFileUri.AbsolutePath);
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = mSilentSetup;
                }
                else
                {
                    //exception occured while downloading
                    mStatus = StatusType.UpdateAviable;

                    if (!SilentCheck)
                        Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                }
            }
            else
                mStatus = StatusType.UpdateAviable;
        }

        void Download_Update_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!SilentDownload && mUpdaterForm != null && !mUpdaterForm.IsDisposed && mUpdaterForm.IsHandleCreated)
                mUpdaterForm.BeginInvoke(new ProgressChangedInvoker(mUpdaterForm.ChangeProgress), new object[] { e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage });
        }

        void Abort()
        {
            mWebClient.CancelAsync();
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
                    Install();
                    break;
            }
        }

        public bool Install()
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName != null)
            {
                string file = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.Windows.Forms.Application.ProductName), Path.Combine(Properties.Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

                //start file (setup) and exit app
                if (File.Exists(file))
                {
                    CancelEventArgs cancelEventArgs = new CancelEventArgs();
                    OnRestartingApplication(cancelEventArgs);

                    if (cancelEventArgs.Cancel)
                        return false;

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = true;

                    //save settings because application was already closed except for this thread
                    Helper.Settings.Save();

                    string arguments = "";
                    if (Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall)
                        arguments = "/SILENT";

                    System.Diagnostics.Process.Start(file, arguments);
                    return true;
                }
            }

            return false;
        }

        public void SetProxy(string proxy)
        {
            mWebClient.Proxy = string.IsNullOrEmpty(proxy) ? null : new WebProxy(proxy);
        }

        public void SetCredentials(string username, string password)
        {
            if (!string.IsNullOrEmpty(username))
                mWebClient.Credentials = new NetworkCredential(username, password);
        }

        public static void RemoveUpdate()
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName != null)
            {
                string file = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.Combine(Properties.Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }

                //don't throw an exception if directory can't be removed (for example if there are still files in it)
                try
                {
                    Directory.Delete(Path.GetDirectoryName(file));
                }
                catch { }

                Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = null;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
            }
        }
    }

    class UpdateInformationParser
    {
        public static UpdateInformation Parse(string content)
        {
            UpdateInformation updateInformation = new UpdateInformation();

            string[] line = content.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (line.Length < 3)
                throw new Exception();

            string[] version = line[0].Split('.');
            if (version.Length < 4)
                throw new Exception();

            updateInformation.Version = new Version(Convert.ToInt32(version[0]), Convert.ToInt32(version[1]), Convert.ToInt32(version[2]), Convert.ToInt32(version[3]));
            updateInformation.FileUri = new Uri(line[1].Trim());
            updateInformation.Silent = (line[2].Trim() == "1");

            return updateInformation;
        }
    }

    class UpdateInformation
    {
        public Version Version;
        public Uri FileUri;
        public bool Silent;
    }

    enum StatusType
    {
        Waiting,
        Checking,
        UpdateAviable,
        UpdateNotAviable,
        Downloading,
        DownloadFinished
    }
}