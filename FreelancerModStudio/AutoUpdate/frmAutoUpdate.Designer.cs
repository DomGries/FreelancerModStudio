namespace FreelancerModStudio.AutoUpdate
{
	partial class frmAutoUpdate
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAutoUpdate));
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
            this.seperatorLine1 = new SeperatorLine();
            this.seperatorLine2 = new SeperatorLine();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.pnlDownload.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Window;
            this.panel1.Controls.Add(this.lblHeader);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // lblHeader
            // 
            resources.ApplyResources(this.lblHeader, "lblHeader");
            this.lblHeader.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblHeader.Name = "lblHeader";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnNext);
            this.panel2.Controls.Add(this.btnAbort);
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // btnNext
            // 
            resources.ApplyResources(this.btnNext, "btnNext");
            this.btnNext.Name = "btnNext";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnAbort
            // 
            resources.ApplyResources(this.btnAbort, "btnAbort");
            this.btnAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnAbort.Name = "btnAbort";
            this.btnAbort.UseVisualStyleBackColor = true;
            this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.lblDescription);
            this.panel3.Controls.Add(this.pnlDownload);
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.Name = "panel3";
            // 
            // lblDescription
            // 
            resources.ApplyResources(this.lblDescription, "lblDescription");
            this.lblDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.lblDescription.Name = "lblDescription";
            // 
            // pnlDownload
            // 
            this.pnlDownload.Controls.Add(this.pgbDownload);
            this.pnlDownload.Controls.Add(this.lblDownloaded);
            resources.ApplyResources(this.pnlDownload, "pnlDownload");
            this.pnlDownload.Name = "pnlDownload";
            // 
            // pgbDownload
            // 
            resources.ApplyResources(this.pgbDownload, "pgbDownload");
            this.pgbDownload.Name = "pgbDownload";
            // 
            // lblDownloaded
            // 
            this.lblDownloaded.FlatStyle = System.Windows.Forms.FlatStyle.System;
            resources.ApplyResources(this.lblDownloaded, "lblDownloaded");
            this.lblDownloaded.Name = "lblDownloaded";
            // 
            // seperatorLine1
            // 
            resources.ApplyResources(this.seperatorLine1, "seperatorLine1");
            this.seperatorLine1.Name = "seperatorLine1";
            // 
            // seperatorLine2
            // 
            resources.ApplyResources(this.seperatorLine2, "seperatorLine2");
            this.seperatorLine2.Name = "seperatorLine2";
            // 
            // frmAutoUpdate
            // 
            this.AcceptButton = this.btnNext;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnAbort;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.seperatorLine1);
            this.Controls.Add(this.seperatorLine2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmAutoUpdate";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmAutoUpdate_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.pnlDownload.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private SeperatorLine seperatorLine1;
		private SeperatorLine seperatorLine2;
		private System.Windows.Forms.Button btnAbort;
		private System.Windows.Forms.Button btnNext;
		private System.Windows.Forms.ProgressBar pgbDownload;
		private System.Windows.Forms.Label lblHeader;
		private System.Windows.Forms.Label lblDescription;
		private System.Windows.Forms.Panel pnlDownload;
        private System.Windows.Forms.Label lblDownloaded;
	}
}