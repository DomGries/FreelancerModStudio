using System;
using System.Windows.Forms;
using FreelancerModStudio.Properties;

namespace FreelancerModStudio.AutoUpdate
{
    public partial class frmAutoUpdate : Form, IAutoUpdateUI
    {
        StatusType _currentPage;

        public frmAutoUpdate()
        {
            InitializeComponent();
            Icon = Resources.LogoIcon;
        }

        delegate void SetStatusInvoker(StatusType status);

        public event ActionRequired ActionRequired;

        void OnAction(ActionType action)
        {
            if (ActionRequired != null)
            {
                ActionRequired(action);
            }
        }

        public void ShowUI()
        {
            ShowDialog();
        }

        public void SetPage(StatusType page, bool async)
        {
            if (async)
            {
                Invoke(new SetStatusInvoker(SetPage), page);
            }
            else
            {
                SetPage(page);
            }
        }

        public void SetPage(StatusType page)
        {
            switch (page)
            {
                case StatusType.Waiting:
                case StatusType.Checking:
                    pnlDownload.Visible = true;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = false;
                    btnAbort.Visible = true;

                    pgbDownload.Style = ProgressBarStyle.Marquee;

                    lblHeader.Text = Strings.UpdatesCheckingHeader;
                    lblDescription.Text = string.Format(Strings.UpdatesCheckingDescription, Helper.Assembly.Name);

                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case StatusType.UpdateAvailable:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesAviableHeader;
                    lblDescription.Text = string.Format(Strings.UpdatesAviableDescription, Helper.Assembly.Name);

                    btnNext.Text = Strings.UpdatesDownloadButton;
                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case StatusType.UpdateNotAvailable:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = false;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesNotAviableHeader;
                    lblDescription.Text = string.Format(Strings.UpdatesNotAviableDescription, Helper.Assembly.Name);

                    btnAbort.Text = Strings.UpdatesFinishButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case StatusType.Downloading:
                    pnlDownload.Visible = true;
                    lblDownloaded.Visible = true;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    pgbDownload.Style = ProgressBarStyle.Blocks;

                    lblHeader.Text = Strings.UpdatesDownloadingHeader;
                    lblDescription.Text = string.Format(Strings.UpdatesDownloadingDescription, Helper.Assembly.Name);

                    btnNext.Text = Strings.UpdatesHideButton;
                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case StatusType.DownloadFinished:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = true;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesDownloadedHeader;
                    lblDescription.Text = string.Format(Strings.UpdatesDownloadedDescription, Helper.Assembly.Name);

                    btnNext.Text = Strings.UpdatesInstallButton;
                    btnAbort.Text = Strings.UpdatesLaterButton;

                    Text = Strings.UpdatesFormText;

                    break;
            }

            _currentPage = page;
        }

        public void SetProgress(long bytes, long bytesTotal, int percent)
        {
            BeginInvoke((MethodInvoker)delegate
                {
                    pgbDownload.Value = percent;

                    // 1MB = 1048576 (1024 * 1024) bytes
                    lblDownloaded.Text = string.Format(Strings.UpdatesDownloadSpeed, ((double)bytes/1048576).ToString("N1"), ((double)bytesTotal/1048576).ToString("N1"));

                    Text = string.Format(Strings.UpdatesFormDownloadText, percent);
                });
        }

        void btnAbort_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            switch (_currentPage)
            {
                case StatusType.UpdateAvailable:
                    OnAction(ActionType.Download);
                    break;
                case StatusType.Downloading:
                    //hide form
                    Close();
                    break;
                case StatusType.DownloadFinished:
                    OnAction(ActionType.Install);
                    Close();
                    break;
            }
        }

        void frmAutoUpdate_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_currentPage == StatusType.Checking ||
                _currentPage == StatusType.Downloading)
            {
                OnAction(ActionType.CloseAndAbort);
            }
            else
            {
                OnAction(ActionType.Close);
            }
        }
    }
}
