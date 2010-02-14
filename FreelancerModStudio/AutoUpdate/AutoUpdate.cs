using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

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
            if (this.RestartingApplication != null)
                this.RestartingApplication(this, e);
        }

        delegate void ProgressChangedInvoker(long bytes, long bytesTotal, int percent);
        delegate void SetStatusInvoker(PageType status);

        public AutoUpdate()
        {
            this.mWebClient = new WebClient();
        }

        public AutoUpdate(string proxy, string username, string password, Uri checkFileUri)
        {
            this.mWebClient = new WebClient();

            this.SetCredentials(username, password);
            this.SetProxy(proxy);

            this.CheckFileUri = checkFileUri;
        }

        public void Check()
        {
            if (this.mStatus == StatusType.Waiting)
            {
                this.mStatus = StatusType.Checking;

                //display checking form
                if (!this.SilentCheck)
                    this.SetPage(PageType.Checking, false);

                //set event handlers
                this.mWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.Download_CheckFile_Completed);

                //download the checkfile
                this.mWebClient.DownloadStringAsync(this.CheckFileUri);
            }
            else
            {
                if (!this.SilentCheck)
                {
                    if (this.mStatus == StatusType.Checking)
                        this.SetPage(PageType.Checking, false);

                    else if (this.mStatus == StatusType.UpdateAviable)
                        this.SetPage(PageType.Aviable, false);

                    else if (this.mStatus == StatusType.UpdateNotAviable)
                        this.SetPage(PageType.NotAviable, false);

                    else if (this.mStatus == StatusType.Downloading)
                        this.SetPage(PageType.Downloading, false);

                    else if (this.mStatus == StatusType.DownloadFinished)
                        this.SetPage(PageType.DownloadFinished, false);
                }
            }
        }

        void UpdateAviable(bool value)
        {
            if (value)
            {
                if (this.SilentDownload && this.SilentCheck)
                    this.DownloadUpdate();
                else
                {
                    this.mStatus = StatusType.UpdateAviable;
                    this.SetPage(PageType.Aviable, false);
                }
            }
            else
            {
                this.mStatus = StatusType.UpdateNotAviable;
                if (!this.SilentCheck)
                    this.SetPage(PageType.NotAviable, false);
            }
        }

        void SetPage(PageType value, bool wait)
        {
            if (this.mUpdaterForm == null || this.mUpdaterForm.IsDisposed || !this.mUpdaterForm.IsHandleCreated)
            {
                this.mUpdaterForm = new frmAutoUpdate();
                this.mUpdaterForm.ActionRequired += this.AutoUpdateForm_ActionRequired;
                this.mUpdaterForm.SetPage(value);

                if (wait)
                    this.mUpdaterForm.ShowDialog();
                else
                    this.mUpdaterForm.Show();
            }
            else
                this.mUpdaterForm.Invoke(new SetStatusInvoker(this.mUpdaterForm.SetPage), value);
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
                        this.mUpdateFileUri = updateInformation.FileUri;
                        this.mSilentSetup = updateInformation.Silent;

                        return true;
                    }
                    else if (updateVersion[index] < currentVersion[index])
                        return false;
                }
            }
            catch (Exception ex)
            {
                if (!this.SilentCheck)
                    Helper.Exceptions.Show(ex);
            }

            return false;
        }

        void DownloadUpdate()
        {
            this.mStatus = StatusType.Downloading;

            //display download form
            if (!this.SilentDownload || !this.SilentCheck)
                this.SetPage(PageType.Downloading, false);

            string destFile = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.Combine(FreelancerModStudio.Properties.Resources.UpdateDownloadPath, Path.GetFileName(this.mUpdateFileUri.AbsolutePath)));

            //create update directory if not existing
            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            //set event handlers
            this.mWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.Download_Update_ProgressChanged);
            this.mWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.Download_Update_Completed);

            //download the update
            this.mWebClient.DownloadFileAsync(this.mUpdateFileUri, destFile);
        }

        void Download_CheckFile_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            //delete event handlers
            this.mWebClient.DownloadStringCompleted -= new DownloadStringCompletedEventHandler(this.Download_CheckFile_Completed);

            if (!e.Cancelled)
            {
                Helper.Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;

                if (e.Error == null)
                {
                    //check if file is newer
                    this.UpdateAviable(this.IsNewer(e.Result));
                }
                else
                {
                    //exception occured while downloading
                    this.mStatus = StatusType.Waiting;

                    if (!this.SilentCheck)
                        Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                }
            }
            else
                this.mStatus = StatusType.Waiting;
        }

        void Download_Update_Completed(object sender, AsyncCompletedEventArgs e)
        {
            //delete event handlers
            this.mWebClient.DownloadProgressChanged -= new DownloadProgressChangedEventHandler(this.Download_Update_ProgressChanged);
            this.mWebClient.DownloadFileCompleted -= new AsyncCompletedEventHandler(this.Download_Update_Completed);

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    //download update completed
                    this.mStatus = StatusType.DownloadFinished;
                    this.SetPage(PageType.DownloadFinished, this.SilentCheck);

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(this.mUpdateFileUri.AbsolutePath);
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = this.mSilentSetup;
                }
                else
                {
                    //exception occured while downloading
                    this.mStatus = StatusType.UpdateAviable;

                    if (!this.SilentCheck)
                        Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
                }
            }
            else
                this.mStatus = StatusType.UpdateAviable;
        }

        void Download_Update_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!this.SilentDownload && this.mUpdaterForm != null && !this.mUpdaterForm.IsDisposed && this.mUpdaterForm.IsHandleCreated)
                this.mUpdaterForm.BeginInvoke(new ProgressChangedInvoker(this.mUpdaterForm.ChangeProgress), new object[] { e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage });
        }

        void Abort()
        {
            this.mWebClient.CancelAsync();
        }

        void AutoUpdateForm_ActionRequired(ActionType action)
        {
            switch (action)
            {
                case ActionType.Abort:
                    this.Abort();
                    break;

                case ActionType.Download:
                    this.DownloadUpdate();
                    break;

                case ActionType.Install:
                    this.Install();
                    break;
            }
        }

        public bool Install()
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName != null)
            {
                string file = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.Combine(FreelancerModStudio.Properties.Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

                //start file (setup) and exit app
                if (File.Exists(file))
                {
                    CancelEventArgs cancelEventArgs = new CancelEventArgs();
                    this.OnRestartingApplication(cancelEventArgs);

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
            if (string.IsNullOrEmpty(proxy))
                this.mWebClient.Proxy = null;
            else
                this.mWebClient.Proxy = new WebProxy(proxy);
        }

        public void SetCredentials(string username, string password)
        {
            if (!string.IsNullOrEmpty(username))
                this.mWebClient.Credentials = new NetworkCredential(username, password);
        }

        public static void RemoveUpdate()
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName != null)
            {
                string file = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.Combine(FreelancerModStudio.Properties.Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

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

            string[] line = content.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

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