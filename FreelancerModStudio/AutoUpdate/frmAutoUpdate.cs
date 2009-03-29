using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace FreelancerModStudio.AutoUpdate
{
    public partial class frmAutoUpdate : Form
    {
        public frmAutoUpdate()
        {
            this.InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;
            this.pictureBox1.Image = Properties.Resources.WebSearch_Large;
        }

        private PageType mCurrentPage = PageType.Checking;

        public delegate void ActionRequiredType(ActionType value);
        public ActionRequiredType ActionRequired;

        public enum ActionType { Abort, Download, Install }

        private void OnAction(ActionType action)
        {
            if (this.ActionRequired != null)
                this.ActionRequired(action);
        }

        public void SetCurrentPage(PageType page)
        {
            switch (page)
            {
                case PageType.Checking:
                    this.pnlDownload.Visible = true;
                    this.lblDownloaded.Visible = false;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.pgbDownload.Style = ProgressBarStyle.Marquee;

                    this.lblHeader.Text = FreelancerModStudio.Properties.Strings.UpdatesCheckingHeader;
                    this.lblDescription.Text = String.Format(FreelancerModStudio.Properties.Strings.UpdatesCheckingDescription, Helper.Assembly.Title);

                    this.btnAbort.Text = FreelancerModStudio.Properties.Strings.UpdatesAbortButton;

                    this.Text = Properties.Strings.UpdatesFormText;

                    break;

                case PageType.Aviable:
                    this.pnlDownload.Visible = false;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = FreelancerModStudio.Properties.Strings.UpdatesAviableHeader;
                    this.lblDescription.Text = String.Format(FreelancerModStudio.Properties.Strings.UpdatesAviableDescription, Helper.Assembly.Title);

                    this.btnNext.Text = FreelancerModStudio.Properties.Strings.UpdatesDownloadButton;
                    this.btnAbort.Text = FreelancerModStudio.Properties.Strings.UpdatesAbortButton;

                    this.Text = Properties.Strings.UpdatesFormText;

                    break;

                case PageType.NotAviable:
                    this.pnlDownload.Visible = false;
                    this.btnNext.Visible = false;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = FreelancerModStudio.Properties.Strings.UpdatesNotAviableHeader;
                    this.lblDescription.Text = String.Format(FreelancerModStudio.Properties.Strings.UpdatesNotAviableDescription, Helper.Assembly.Title);

                    this.btnAbort.Text = FreelancerModStudio.Properties.Strings.UpdatesFinishButton;

                    this.Text = Properties.Strings.UpdatesFormText;

                    break;

                case PageType.Downloading:
                    this.pnlDownload.Visible = true;
                    this.lblDownloaded.Visible = true;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.pgbDownload.Style = ProgressBarStyle.Blocks;

                    this.lblHeader.Text = FreelancerModStudio.Properties.Strings.UpdatesDownloadingHeader;
                    this.lblDescription.Text = String.Format(FreelancerModStudio.Properties.Strings.UpdatesDownloadingDescription, Helper.Assembly.Title);

                    this.btnNext.Text = FreelancerModStudio.Properties.Strings.UpdatesHideButton;
                    this.btnAbort.Text = FreelancerModStudio.Properties.Strings.UpdatesAbortButton;

                    this.Text = String.Format(Properties.Strings.UpdatesFormDownloadText, 0);

                    break;

                case PageType.DownloadFinished:
                    this.pnlDownload.Visible = false;
                    this.btnNext.Visible = true;
                    this.btnAbort.Visible = true;

                    this.lblHeader.Text = FreelancerModStudio.Properties.Strings.UpdatesDownloadedHeader;
                    this.lblDescription.Text = String.Format(FreelancerModStudio.Properties.Strings.UpdatesDownloadedDescription, Helper.Assembly.Title);

                    this.btnNext.Text = FreelancerModStudio.Properties.Strings.UpdatesInstallButton;
                    this.btnAbort.Text = FreelancerModStudio.Properties.Strings.UpdatesLaterButton;

                    this.Text = Properties.Strings.UpdatesFormText;

                    break;
            }
            this.mCurrentPage = page;
        }

        public void ChangeProgress(long bytes, long bytesTotal, int percent)
        {
            int kbRead = Convert.ToInt32(bytes / 1024);
            int kbTotal = Convert.ToInt32(bytesTotal / 1024);

            this.pgbDownload.Value = percent;
            this.lblDownloaded.Text = String.Format(FreelancerModStudio.Properties.Strings.DownloadSpeed, (Convert.ToDouble(kbRead) / 1024).ToString("N1"), (Convert.ToDouble(kbTotal) / 1024).ToString("N1"));

            this.Text = String.Format(Properties.Strings.UpdatesFormDownloadText, percent);
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            switch (this.mCurrentPage)
            {
                case PageType.Aviable:
                    this.OnAction(ActionType.Download);

                    break;

                case PageType.Downloading:
                    //hide form
                    this.mCurrentPage = PageType.DownloadFinished;
                    this.Close();

                    break;

                case PageType.DownloadFinished:
                    this.OnAction(ActionType.Install);
                    this.Close();

                    break;
            }
        }

        public enum PageType { Checking, Aviable, NotAviable, Downloading, DownloadFinished };

        private void frmAutoUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((this.mCurrentPage == PageType.Checking || this.mCurrentPage == PageType.Downloading))
                this.OnAction(ActionType.Abort);
        }
    }
}