namespace FreelancerModStudio
{
    partial class frmNewMod
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmNewMod));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtSaveLocation = new System.Windows.Forms.TextBox();
            this.txtAuthor = new System.Windows.Forms.TextBox();
            this.txtHomepage = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.seperatorLine1 = new SeperatorLine();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label3.Font = null;
            this.label3.Name = "label3";
            // 
            // label4
            // 
            this.label4.AccessibleDescription = null;
            this.label4.AccessibleName = null;
            resources.ApplyResources(this.label4, "label4");
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label4.Font = null;
            this.label4.Name = "label4";
            // 
            // label5
            // 
            this.label5.AccessibleDescription = null;
            this.label5.AccessibleName = null;
            resources.ApplyResources(this.label5, "label5");
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label5.Font = null;
            this.label5.Name = "label5";
            // 
            // txtName
            // 
            this.txtName.AccessibleDescription = null;
            this.txtName.AccessibleName = null;
            resources.ApplyResources(this.txtName, "txtName");
            this.txtName.BackgroundImage = null;
            this.txtName.Font = null;
            this.txtName.Name = "txtName";
            this.txtName.TextChanged += new System.EventHandler(this.Path_TextChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.AccessibleDescription = null;
            this.btnBrowse.AccessibleName = null;
            resources.ApplyResources(this.btnBrowse, "btnBrowse");
            this.btnBrowse.BackgroundImage = null;
            this.btnBrowse.Font = null;
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtSaveLocation
            // 
            this.txtSaveLocation.AccessibleDescription = null;
            this.txtSaveLocation.AccessibleName = null;
            resources.ApplyResources(this.txtSaveLocation, "txtSaveLocation");
            this.txtSaveLocation.BackgroundImage = null;
            this.txtSaveLocation.Font = null;
            this.txtSaveLocation.Name = "txtSaveLocation";
            this.txtSaveLocation.TextChanged += new System.EventHandler(this.Path_TextChanged);
            // 
            // txtAuthor
            // 
            this.txtAuthor.AccessibleDescription = null;
            this.txtAuthor.AccessibleName = null;
            resources.ApplyResources(this.txtAuthor, "txtAuthor");
            this.txtAuthor.BackgroundImage = null;
            this.txtAuthor.Font = null;
            this.txtAuthor.Name = "txtAuthor";
            // 
            // txtHomepage
            // 
            this.txtHomepage.AccessibleDescription = null;
            this.txtHomepage.AccessibleName = null;
            resources.ApplyResources(this.txtHomepage, "txtHomepage");
            this.txtHomepage.BackgroundImage = null;
            this.txtHomepage.Font = null;
            this.txtHomepage.Name = "txtHomepage";
            // 
            // txtDescription
            // 
            this.txtDescription.AcceptsReturn = true;
            this.txtDescription.AccessibleDescription = null;
            this.txtDescription.AccessibleName = null;
            resources.ApplyResources(this.txtDescription, "txtDescription");
            this.txtDescription.BackgroundImage = null;
            this.txtDescription.Font = null;
            this.txtDescription.Name = "txtDescription";
            // 
            // btnCancel
            // 
            this.btnCancel.AccessibleDescription = null;
            this.btnCancel.AccessibleName = null;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackgroundImage = null;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = null;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.AccessibleDescription = null;
            this.btnOK.AccessibleName = null;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.BackgroundImage = null;
            this.btnOK.Font = null;
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // folderBrowserDialog1
            // 
            resources.ApplyResources(this.folderBrowserDialog1, "folderBrowserDialog1");
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // seperatorLine1
            // 
            this.seperatorLine1.AccessibleDescription = null;
            this.seperatorLine1.AccessibleName = null;
            resources.ApplyResources(this.seperatorLine1, "seperatorLine1");
            this.seperatorLine1.BackgroundImage = null;
            this.seperatorLine1.Font = null;
            this.seperatorLine1.Name = "seperatorLine1";
            // 
            // frmNewMod
            // 
            this.AcceptButton = this.btnOK;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.seperatorLine1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtHomepage);
            this.Controls.Add(this.txtAuthor);
            this.Controls.Add(this.txtSaveLocation);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmNewMod";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private SeperatorLine seperatorLine1;
        public System.Windows.Forms.TextBox txtName;
        public System.Windows.Forms.TextBox txtSaveLocation;
        public System.Windows.Forms.TextBox txtAuthor;
        public System.Windows.Forms.TextBox txtHomepage;
        public System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

    }
}