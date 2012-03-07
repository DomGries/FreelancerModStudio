using System;
using System.Windows.Forms;
using FreelancerModStudio.Properties;

namespace FreelancerModStudio.AutoUpdate
{
    public partial class frmAutoUpdate : Form
    {
        public frmAutoUpdate()
        {
            InitializeComponent();
            Icon = Resources.LogoIcon;
        }

        PageType _currentPage = PageType.Checking;

        public delegate void ActionRequiredType(ActionType value);
        public ActionRequiredType ActionRequired;

        void OnAction(ActionType action)
        {
            if (ActionRequired != null)
                ActionRequired(action);
        }

        public void SetPage(PageType page)
        {
            switch (page)
            {
                case PageType.Checking:
                    pnlDownload.Visible = true;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = false;
                    btnAbort.Visible = true;

                    pgbDownload.Style = ProgressBarStyle.Marquee;

                    lblHeader.Text = Strings.UpdatesCheckingHeader;
                    lblDescription.Text = String.Format(Strings.UpdatesCheckingDescription, Helper.Assembly.Title);

                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case PageType.Aviable:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesAviableHeader;
                    lblDescription.Text = String.Format(Strings.UpdatesAviableDescription, Helper.Assembly.Title);

                    btnNext.Text = Strings.UpdatesDownloadButton;
                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case PageType.NotAviable:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = false;
                    btnNext.Visible = false;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesNotAviableHeader;
                    lblDescription.Text = String.Format(Strings.UpdatesNotAviableDescription, Helper.Assembly.Title);

                    btnAbort.Text = Strings.UpdatesFinishButton;

                    Text = Strings.UpdatesFormText;

                    break;

                case PageType.Downloading:
                    pnlDownload.Visible = true;
                    lblDownloaded.Visible = true;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    pgbDownload.Style = ProgressBarStyle.Blocks;

                    lblHeader.Text = Strings.UpdatesDownloadingHeader;
                    lblDescription.Text = String.Format(Strings.UpdatesDownloadingDescription, Helper.Assembly.Title);

                    btnNext.Text = Strings.UpdatesHideButton;
                    btnAbort.Text = Strings.UpdatesAbortButton;

                    Text = String.Format(Strings.UpdatesFormDownloadText, 0);

                    break;

                case PageType.DownloadFinished:
                    pnlDownload.Visible = false;
                    lblDownloaded.Visible = true;
                    btnNext.Visible = true;
                    btnAbort.Visible = true;

                    lblHeader.Text = Strings.UpdatesDownloadedHeader;
                    lblDescription.Text = String.Format(Strings.UpdatesDownloadedDescription, Helper.Assembly.Title);

                    btnNext.Text = Strings.UpdatesInstallButton;
                    btnAbort.Text = Strings.UpdatesLaterButton;

                    Text = Strings.UpdatesFormText;

                    break;
            }

            _currentPage = page;
        }

        public void ChangeProgress(long bytes, long bytesTotal, int percent)
        {
            int kbRead = Convert.ToInt32(bytes / 1024);
            int kbTotal = Convert.ToInt32(bytesTotal / 1024);

            pgbDownload.Value = percent;
            lblDownloaded.Text = String.Format(Strings.UpdatesDownloadSpeed, (Convert.ToDouble(kbRead) / 1024).ToString("N1"), (Convert.ToDouble(kbTotal) / 1024).ToString("N1"));

            Text = String.Format(Strings.UpdatesFormDownloadText, percent);
        }

        void btnAbort_Click(object sender, EventArgs e)
        {
            Close();
        }

        void btnNext_Click(object sender, EventArgs e)
        {
            switch (_currentPage)
            {
                case PageType.Aviable:
                    OnAction(ActionType.Download);
                    break;

                case PageType.Downloading:
                    //hide form
                    _currentPage = PageType.DownloadFinished;
                    Close();
                    break;

                case PageType.DownloadFinished:
                    OnAction(ActionType.Install);
                    Close();
                    break;
            }
        }

        void frmAutoUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((_currentPage == PageType.Checking || _currentPage == PageType.Downloading))
                OnAction(ActionType.Abort);
        }
    }

    public enum ActionType { Abort, Download, Install }

    public enum PageType { Checking, Aviable, NotAviable, Downloading, DownloadFinished };
}