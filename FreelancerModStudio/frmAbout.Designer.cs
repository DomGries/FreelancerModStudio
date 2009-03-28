namespace FreelancerModStudio
{
    partial class frmAbout
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAbout));
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.lblCompanyName = new System.Windows.Forms.Label();
			this.lblCopyright = new System.Windows.Forms.Label();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblProductName = new System.Windows.Forms.Label();
			this.txtDescription = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
			this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.lblCompanyName, 1, 4);
			this.tableLayoutPanel.Controls.Add(this.lblCopyright, 1, 3);
			this.tableLayoutPanel.Controls.Add(this.lblVersion, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.lblProductName, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.txtDescription, 0, 6);
			this.tableLayoutPanel.Controls.Add(this.btnOK, 1, 7);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			// 
			// logoPictureBox
			// 
			resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
			this.logoPictureBox.Image = global::FreelancerModStudio.Properties.Resources.LogoAbout;
			this.logoPictureBox.Name = "logoPictureBox";
			this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 5);
			this.logoPictureBox.TabStop = false;
			// 
			// lblCompanyName
			// 
			resources.ApplyResources(this.lblCompanyName, "lblCompanyName");
			this.lblCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
			this.lblCompanyName.Name = "lblCompanyName";
			// 
			// lblCopyright
			// 
			resources.ApplyResources(this.lblCopyright, "lblCopyright");
			this.lblCopyright.MaximumSize = new System.Drawing.Size(0, 17);
			this.lblCopyright.Name = "lblCopyright";
			// 
			// lblVersion
			// 
			resources.ApplyResources(this.lblVersion, "lblVersion");
			this.lblVersion.MaximumSize = new System.Drawing.Size(0, 17);
			this.lblVersion.Name = "lblVersion";
			// 
			// lblProductName
			// 
			resources.ApplyResources(this.lblProductName, "lblProductName");
			this.lblProductName.MaximumSize = new System.Drawing.Size(0, 17);
			this.lblProductName.Name = "lblProductName";
			// 
			// txtDescription
			// 
			this.txtDescription.BackColor = System.Drawing.SystemColors.Control;
			this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel.SetColumnSpan(this.txtDescription, 2);
			resources.ApplyResources(this.txtDescription, "txtDescription");
			this.txtDescription.Name = "txtDescription";
			this.txtDescription.ReadOnly = true;
			this.txtDescription.TabStop = false;
			// 
			// btnOK
			// 
			resources.ApplyResources(this.btnOK, "btnOK");
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOK.Name = "btnOK";
			// 
			// frmAbout
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmAbout";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Button btnOK;
    }
}
