﻿namespace PatchTime
{
    using System.ComponentModel;
    using System.Windows.Forms;

    partial class FrmAutoUpdate
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
        {
            ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(FrmAutoUpdate));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnAbort = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblDescription = new System.Windows.Forms.Label();
            this.pnlDownload = new System.Windows.Forms.Panel();
            this.pgbDownload = new System.Windows.Forms.ProgressBar();
            this.lblDownloaded = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnlDownload.SuspendLayout();
            this.SuspendLayout();

            // panel1
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.lblHeader);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";

            // lblHeader
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblHeader.Name = "lblHeader";

            // panel2
            this.panel2.Controls.Add(this.btnNext);
            this.panel2.Controls.Add(this.btnAbort);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";

            // btnNext
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.BtnNextClick);

            // btnAbort
            resources.ApplyResources(this.btnAbort, "btnAbort");
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.BtnAbortClick);

            // panel3
            this.panel3.Controls.Add(this.lblDescription);
            this.panel3.Controls.Add(this.pnlDownload);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";

            // lblDescription
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDescription.Name = "lblDescription";

            // pnlDownload
            this.pnlDownload.Controls.Add(this.pgbDownload);
            this.pnlDownload.Controls.Add(this.lblDownloaded);
            resources.ApplyResources(this.pnlDownload, "pnlDownload");
            this.pnlDownload.Name = "pnlDownload";

            // pgbDownload
            resources.ApplyResources(this.pgbDownload, "pgbDownload");
            this.pgbDownload.Name = "pgbDownload";

            // lblDownloaded
            this.lblDownloaded.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblDownloaded, "lblDownloaded");
            this.lblDownloaded.Name = "lblDownloaded";

            // frmAutoUpdate
            this.AcceptButton = this.btnNext;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAbort;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmAutoUpdate";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmAutoUpdateFormClosed);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.pnlDownload.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

		private Panel panel1;
		private Panel panel2;
		private Panel panel3;
		private Button btnAbort;
		private Button btnNext;
		private ProgressBar pgbDownload;
		private Label lblHeader;
		private Label lblDescription;
		private Panel pnlDownload;
        private Label lblDownloaded;
	}
}