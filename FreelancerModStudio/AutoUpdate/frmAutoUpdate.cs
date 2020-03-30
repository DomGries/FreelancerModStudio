namespace FreelancerModStudio.AutoUpdate
{
    using System;
    using System.Windows.Forms;

    using FreelancerModStudio.Properties;

    public partial class FrmAutoUpdate : Form, IAutoUpdateUi
    {
        private StatusType currentPage;

        public FrmAutoUpdate()
        {
            this.InitializeComponent();
            this.Icon = Resources.LogoIcon;
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

                    this.lblHeader.Text = Strings.UpdatesCheckingHeader;
                    this.lblDescription.Text = string.Format(Strings.UpdatesCheckingDescription, Helper.Assembly.Name);

                    this.btnAbort.Text = Strings.UpdatesAbortButton;

                    this.Text = Strings.UpdatesFormText;

                    break;

                case StatusType.UpdateAvailable:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = Strings.UpdatesAviableHeader;
                    this.lblDescription.Text = string.Format(Strings.UpdatesAviableDescription, Helper.Assembly.Name);

                    this.btnNext.Text = Strings.UpdatesDownloadButton;
                    this.btnAbort.Text = Strings.UpdatesAbortButton;

                    this.Text = Strings.UpdatesFormText;

                    break;

                case StatusType.UpdateNotAvailable:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = Strings.UpdatesNotAviableHeader;
                    this.lblDescription.Text = string.Format(Strings.UpdatesNotAviableDescription, Helper.Assembly.Name);

                    this.btnAbort.Text = Strings.UpdatesFinishButton;

                    this.Text = Strings.UpdatesFormText;

                    break;

                case StatusType.Downloading:
                    this.pnlDownload.Visible = true;
                    this.lblDownloaded.Visible = true;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.pgbDownload.Style = ProgressBarStyle.Blocks;

                    this.lblHeader.Text = Strings.UpdatesDownloadingHeader;
                    this.lblDescription.Text = string.Format(Strings.UpdatesDownloadingDescription, Helper.Assembly.Name);

                    this.btnNext.Text = Strings.UpdatesHideButton;
                    this.btnAbort.Text = Strings.UpdatesAbortButton;

                    this.Text = Strings.UpdatesFormText;

                    break;

                case StatusType.DownloadFinished:
                    this.pnlDownload.Visible = false;
                    this.lblDownloaded.Visible = true;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = Strings.UpdatesDownloadedHeader;
                    this.lblDescription.Text = string.Format(Strings.UpdatesDownloadedDescription, Helper.Assembly.Name);

                    this.btnNext.Text = Strings.UpdatesInstallButton;
                    this.btnAbort.Text = Strings.UpdatesLaterButton;

                    this.Text = Strings.UpdatesFormText;

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
                    this.lblDownloaded.Text = string.Format(Strings.UpdatesDownloadSpeed, ((double)bytes/1048576).ToString("N1"), ((double)bytesTotal/1048576).ToString("N1"));

                    this.Text = string.Format(Strings.UpdatesFormDownloadText, percent);
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
