using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace FreelancerModStudio.AutoUpdate
{
    //HACK: if automatic updater is still running from already closed Freelancer Mod Studio (Main form closed but updater thread is still running) there will be some strange side effects
    public class AutoUpdate
    {
        private Uri mUpdateFileUri;
        private Uri mCheckFileUri;

        private bool mSilentSetup;
        private bool mSilentDownload;
        private bool mSilentCheck;

        private WebClient mWebClient;

        private frmAutoUpdate mFrmAutoUpdate;

        private Thread mCheckUpdateThread;
        private Thread mAutoUpdateFormThread;

        public event EventHandler<CancelEventArgs> RestartingApplication;

        protected virtual void OnRestartingApplication(CancelEventArgs e)
        {
            if (this.RestartingApplication != null)
                this.RestartingApplication(this, e);
        }

        private delegate void ProgressChangedInvoker(long bytes, long bytesTotal, int percent);
        private delegate void SetStatusInvoker(frmAutoUpdate.PageType status);

        public AutoUpdate(string proxy, string username, string password, Uri checkFileUri, bool silentCheck, bool silentDownload)
        {
            //TODO: kill silent automatic updater in background if it is running
            this.mWebClient = new WebClient();

            if (!string.IsNullOrEmpty(username))
                this.mWebClient.Credentials = new NetworkCredential(username, password);

            if (!string.IsNullOrEmpty(proxy))
                this.mWebClient.Proxy = new WebProxy(proxy);

            //set event handlers
            this.mWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.Download_ProgressChanged);
            this.mWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.Download_Completed);

            //set local viariable
            this.mCheckFileUri = checkFileUri;
            this.mSilentCheck = silentCheck;
            this.mSilentDownload = silentDownload;
        }

        public void Check()
        {
            ThreadPriority priority = ThreadPriority.Normal;
            if (this.mSilentCheck)
                priority = ThreadPriority.Lowest;

            Helper.Thread.Start(ref this.mCheckUpdateThread, new ThreadStart(this.CheckUpdate), priority, true);
        }

        private void CheckUpdate()
        {
            if (!this.mSilentDownload || !this.mSilentCheck)
                this.SetPage(frmAutoUpdate.PageType.Checking);

            Helper.Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;

            //download the checkfile
            string content = "";

            try
            {
                content = this.mWebClient.DownloadString(this.mCheckFileUri);
            }
            catch { }

            //check if file is newer
            this.UpdateAviable(this.IsNewer(content));
        }

        private void UpdateAviable(bool value)
        {
            if (value)
            {
                if (this.mSilentDownload && this.mSilentCheck)
                    this.DownloadUpdate();
                else
                    this.SetPage(frmAutoUpdate.PageType.Aviable);
            }
            else
            {
                if (!this.mSilentDownload || !this.mSilentCheck)
                    this.SetPage(frmAutoUpdate.PageType.NotAviable);
            }
        }

        private void SetPage(frmAutoUpdate.PageType value)
        {
            if (this.mAutoUpdateFormThread == null)
            {
                this.mFrmAutoUpdate = new frmAutoUpdate();
                this.mFrmAutoUpdate.SetCurrentPage(value);
                this.mFrmAutoUpdate.ActionRequired += new EventHandler<frmAutoUpdate.ActionEventArgs>(this.AutoUpdateForm_ActionRequired);

                Helper.Thread.Start(ref this.mAutoUpdateFormThread, this.ShowAutoUpdateForm, ThreadPriority.Normal, true);
            }
            else
                this.mFrmAutoUpdate.Invoke(new SetStatusInvoker(this.mFrmAutoUpdate.SetCurrentPage), value);
        }

        private void ShowAutoUpdateForm()
        {
            this.mFrmAutoUpdate.ShowDialog();
        }

        private bool IsNewer(string fileContent)
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
            catch { }

            return false;
        }

        private void DownloadUpdate()
        {
            if (!this.mSilentDownload || !this.mSilentCheck)
                this.SetPage(frmAutoUpdate.PageType.Downloading);

            string destFile = Path.Combine(System.Windows.Forms.Application.StartupPath, Path.Combine(FreelancerModStudio.Properties.Resources.UpdateDownloadPath, Path.GetFileName(this.mUpdateFileUri.AbsolutePath)));

            if (!Directory.Exists(Path.GetDirectoryName(destFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

            //download the update
            this.mWebClient.DownloadFileAsync(this.mUpdateFileUri, destFile);
        }

        private void Download_Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //exception occured while downloading
                if (this.mFrmAutoUpdate != null)
                    this.Abort();

                Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
            }
            else
            {
                //download update completed
                if (!this.mSilentDownload || !this.mSilentCheck)
                    this.SetPage(frmAutoUpdate.PageType.DownloadFinished);

                Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(this.mUpdateFileUri.AbsolutePath);
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = this.mSilentSetup;
                Helper.Settings.Save();
            }
        }

        private void Download_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!this.mSilentDownload)
                this.mFrmAutoUpdate.Invoke(new ProgressChangedInvoker(this.mFrmAutoUpdate.ChangeProgress), new object[] { e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage });
        }

        private void Abort()
        {
            this.mWebClient.CancelAsync();
            this.mCheckUpdateThread.Join();

            //close the auto update form
            Helper.Thread.Abort(ref this.mAutoUpdateFormThread, true);
            this.mAutoUpdateFormThread = null;
        }

        private void AutoUpdateForm_ActionRequired(object sender, frmAutoUpdate.ActionEventArgs e)
        {
            switch (e.Action)
            {
                case frmAutoUpdate.ActionType.Abort:
                    this.Abort();
                    break;

                case frmAutoUpdate.ActionType.Download:
                    this.DownloadUpdate();
                    break;

                case frmAutoUpdate.ActionType.Install:
                    this.Install();
                    break;
            }
        }

        public void Install()
        {
            if (Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName != null)
            {
                string file = Path.Combine(System.Windows.Forms.Application.StartupPath, String.Format(FreelancerModStudio.Properties.Resources.UpdateDownloadPath, Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName));

                //start file (setup) and exit app
                if (File.Exists(file))
                {
                    CancelEventArgs cancelEventArgs = new CancelEventArgs();
                    this.OnRestartingApplication(cancelEventArgs);

                    if (cancelEventArgs.Cancel)
                        return;

                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = false;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = true;
                    Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = false;
                    Helper.Settings.Save();

                    string arguments = "";

                    if (Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall)
                        //TODO: get argument command via update.txt
                        arguments = "/SILENT";

                    System.Diagnostics.Process.Start(file, arguments);
                    Environment.Exit(0);
                }
            }
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

        public enum DownloadFileType { CheckFile, Update }
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
}