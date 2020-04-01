namespace PatchTime
{
    using System;
    using System.Windows.Forms;

    using PatchTime.Properties;

    public partial class FrmAutoUpdate : Form, IAutoUpdateUi
    {
        private StatusType currentPage;

        public FrmAutoUpdate()
        {
            this.InitializeComponent();
            this.Icon = Resources.Starflier;
        }

        private delegate void SetStatusInvoker(StatusType status);

        public event ActionRequired ActionRequired;

        private void OnAction(ActionType action)
        {
            this.ActionRequired?.Invoke(action);
        }

        public void ShowUi()
        {
            this.ShowDialog();
        }

        public void SetPage(StatusType page, bool async)
        {
            if (async)
            {
                this.Invoke(new SetStatusInvoker(this.SetPage), page);
            }
            else
            {
                this.SetPage(page);
            }
        }

        public void SetPage(StatusType page)
        {
            switch (page)
            {
                case StatusType.Waiting:
                case StatusType.Checking:
                    this.pnlDownload.Visible = true;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.pgbDownload.Style = ProgressBarStyle.Marquee;

                    this.lblHeader.Text = "Checking For Updates";
                    this.lblDescription.Text = "Checking remote server for updates...";

                    this.btnAbort.Text = "Abort";

                    this.Text = "Software Update";

                    break;

                case StatusType.UpdateAvailable:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = "New Updates Found";
                    this.lblDescription.Text = "Updates have been found from remote server.\nNote: Downloading updates will close FLModStudio. Save any changes before continuing.";

                    this.btnNext.Text = "Download";
                    this.btnAbort.Text = "Abort";

                    this.Text = "Software Update";

                    break;

                case StatusType.UpdateNotAvailable:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = "No Updates Found";
                    this.lblDescription.Text = "No updates were found";
                    this.btnAbort.Text = "Abort";

                    this.Text = "Software Update";

                    break;

                case StatusType.Downloading:
                    this.pnlDownload.Visible = true;
                    this.lblDownloaded.Visible = true;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.pgbDownload.Style = ProgressBarStyle.Blocks;

                    this.lblHeader.Text = "Downloading Updates";
                    this.lblDescription.Text = "Downloading new updates...";

                    this.btnAbort.Text = "Abort";

                    this.Text = "Software Update";

                    break;

                case StatusType.DownloadFinished:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = true;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = "Download Complete";
                    this.lblDescription.Text = "Updates have finished downloading.";

                    this.btnNext.Text = "Launch Updated Application";

                    this.Text = "Software Update";

                    break;
            }

            this.currentPage = page;
        }

        public void SetProgress(long bytes, long bytesTotal, int percent)
        {
            this.BeginInvoke((MethodInvoker)delegate
                {
                    this.pgbDownload.Value = percent;

                    // 1MB = 1048576 (1024 * 1024) bytes
                    this.lblDownloaded.Text = string.Format("{0} of {1} MB", ((double)bytes/1048576).ToString("N1"), ((double)bytesTotal/1048576).ToString("N1"));

                    this.Text = string.Format("{0}% - Software Update", percent);
                });
        }

        private void BtnAbortClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnNextClick(object sender, EventArgs e)
        {
            switch (this.currentPage)
            {
                case StatusType.UpdateAvailable:
                    this.OnAction(ActionType.Download);
                    break;
                case StatusType.Downloading:
                    // hide form
                    this.Close();
                    break;
                case StatusType.DownloadFinished:
                    this.OnAction(ActionType.Install);
                    this.Close();
                    break;
            }
        }

        private void FrmAutoUpdateFormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.currentPage == StatusType.Checking || this.currentPage == StatusType.Downloading)
            {
                this.OnAction(ActionType.CloseAndAbort);
            }
            else
            {
                this.OnAction(ActionType.Close);
            }
        }
    }
}
