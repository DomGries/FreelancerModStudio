using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;

namespace FreelancerModStudio.AutoUpdate
{
    //todo: if automatic updater is still running from already closed Freelancer Mod Studio (Main form closed but updater thread is still running) there will be some strange side effects
    public class AutoUpdate
    {
        private Uri mUpdateFileUri;
        public Uri CheckFileUri { get; set; }

        private bool mSilentSetup;
        public bool SilentDownload { get; set; }
        public bool SilentCheck { get; set; }

        private WebClient mWebClient;

        private frmAutoUpdate mUpdaterForm;
        private Thread mUpdaterFormThread;

        public event EventHandler<CancelEventArgs> RestartingApplication;

        protected virtual void OnRestartingApplication(CancelEventArgs e)
        {
            if (this.RestartingApplication != null)
                this.RestartingApplication(this, e);
        }

        private delegate void ProgressChangedInvoker(long bytes, long bytesTotal, int percent);
        private delegate void SetStatusInvoker(frmAutoUpdate.PageType status);

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
            //display checking form
            if (!this.SilentCheck)
                this.SetPage(frmAutoUpdate.PageType.Checking);

            //set event handlers
            this.mWebClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(this.Download_CheckFile_Completed);

            //download the checkfile
            this.mWebClient.DownloadStringAsync(this.CheckFileUri);
        }

        private void UpdateAviable(bool value)
        {
            if (value)
            {
                if (this.SilentDownload && this.SilentCheck)
                    this.DownloadUpdate();
                else
                    this.SetPage(frmAutoUpdate.PageType.Aviable);
            }
            else
            {
                if (!this.SilentCheck)
                    this.SetPage(frmAutoUpdate.PageType.NotAviable);
            }
        }

        private void SetPage(frmAutoUpdate.PageType value)
        {
            if (!Helper.Thread.IsRunning(ref this.mUpdaterFormThread))
            {
                this.mUpdaterForm = new frmAutoUpdate();
                this.mUpdaterForm.SetCurrentPage(value);
                this.mUpdaterForm.ActionRequired += this.AutoUpdateForm_ActionRequired;

                Helper.Thread.Start(ref this.mUpdaterFormThread, this.ShowAutoUpdateForm, ThreadPriority.Normal, true);
            }
            else
                this.mUpdaterForm.Invoke(new SetStatusInvoker(this.mUpdaterForm.SetCurrentPage), value);
        }

        private void ShowAutoUpdateForm()
        {
            this.mUpdaterForm.ShowDialog();
        }

        private bool IsNewer(string fileContent)
        {
            this.mUpdateFileUri = new Uri(@"http://www.brilliantpaper.com/files/Reference01.jpg");
            //hack:1
            return true;
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
                Helper.Exceptions.Show(ex);
            }

            return false;
        }

        private void DownloadUpdate()
        {
            //display download form
            if (!this.SilentDownload || !this.SilentCheck)
                this.SetPage(frmAutoUpdate.PageType.Downloading);

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

        private void Download_CheckFile_Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || this.SilentCheck)
                return;

            if (e.Error != null)
            {
                //exception occured while downloading
                //if (this.mUpdaterForm != null)
                //    this.mUpdaterForm.Close();

                Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
            }
            else
            {
                Helper.Settings.Data.Data.General.AutoUpdate.LastCheck = DateTime.Now;

                //check if file is newer
                this.UpdateAviable(this.IsNewer(e.Result));
            }
        }

        private void Download_Update_Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                //exception occured while downloading
                //if (this.mUpdaterForm != null)
                //    this.mUpdaterForm.Close();

                Helper.Exceptions.Show(new Exception(String.Format(Properties.Strings.UpdatesDownloadException, Helper.Assembly.Title), e.Error));
            }
            else
            {
                //download update completed
                //if (!this.mSilentDownload || !this.mSilentCheck)
                this.SetPage(frmAutoUpdate.PageType.DownloadFinished);

                Helper.Settings.Data.Data.General.AutoUpdate.Update.FileName = Path.GetFileName(this.mUpdateFileUri.AbsolutePath);
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Installed = false;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.Downloaded = true;
                Helper.Settings.Data.Data.General.AutoUpdate.Update.SilentInstall = this.mSilentSetup;
            }
        }

        private void Download_Update_ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!this.SilentDownload)
                this.mUpdaterForm.Invoke(new ProgressChangedInvoker(this.mUpdaterForm.ChangeProgress), new object[] { e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage });
        }

        private void Abort()
        {
            this.mWebClient.CancelAsync();
        }

        private void AutoUpdateForm_ActionRequired(frmAutoUpdate.ActionType action)
        {
            switch (action)
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
}