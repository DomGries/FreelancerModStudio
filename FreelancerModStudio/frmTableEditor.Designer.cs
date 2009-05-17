namespace FreelancerModStudio
{
    partial class frmTableEditor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTableEditor));
            this.objectListView1 = new BrightIdeasSoftware.FastObjectListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAdd2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelete2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReDo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUnDo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuGoTo = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.objectListView1.ContextMenuStrip = this.contextMenuStrip1;
            resources.ApplyResources(this.objectListView1, "objectListView1");
            this.objectListView1.EmptyListMsgFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.HideSelection = false;
            this.objectListView1.ItemRenderer = null;
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.ShowGroups = false;
            this.objectListView1.UseAlternatingBackColors = true;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.VirtualMode = true;
            this.objectListView1.SelectionChanged += new System.EventHandler(this.objectListView1_SelectionChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAdd2,
            this.mnuDelete2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // mnuAdd2
            // 
            this.mnuAdd2.Image = global::FreelancerModStudio.Properties.Resources.Add;
            this.mnuAdd2.Name = "mnuAdd2";
            resources.ApplyResources(this.mnuAdd2, "mnuAdd2");
            this.mnuAdd2.Click += new System.EventHandler(this.mnuAdd_Click);
            // 
            // mnuDelete2
            // 
            resources.ApplyResources(this.mnuDelete2, "mnuDelete2");
            this.mnuDelete2.Image = global::FreelancerModStudio.Properties.Resources.Delete;
            this.mnuDelete2.Name = "mnuDelete2";
            this.mnuDelete2.Click += new System.EventHandler(this.mnuDelete_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.mnuEdit});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuClose,
            this.mnuSave,
            this.mnuSaveAs});
            this.fileToolStripMenuItem.MergeAction = System.Windows.Forms.MergeAction.MatchOnly;
            this.fileToolStripMenuItem.MergeIndex = 0;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // mnuClose
            // 
            this.mnuClose.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuClose.MergeIndex = 4;
            this.mnuClose.Name = "mnuClose";
            resources.ApplyResources(this.mnuClose, "mnuClose");
            this.mnuClose.Click += new System.EventHandler(this.mnuClose_Click);
            // 
            // mnuSave
            // 
            this.mnuSave.Image = global::FreelancerModStudio.Properties.Resources.Save;
            this.mnuSave.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuSave.MergeIndex = 7;
            this.mnuSave.Name = "mnuSave";
            resources.ApplyResources(this.mnuSave, "mnuSave");
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuSaveAs
            // 
            this.mnuSaveAs.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuSaveAs.MergeIndex = 7;
            this.mnuSaveAs.Name = "mnuSaveAs";
            resources.ApplyResources(this.mnuSaveAs, "mnuSaveAs");
            this.mnuSaveAs.Click += new System.EventHandler(this.mnuSaveAs_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuReDo,
            this.mnuUnDo,
            this.toolStripMenuItem3,
            this.mnuAdd,
            this.mnuDelete,
            this.toolStripMenuItem4,
            this.mnuSelectAll,
            this.toolStripMenuItem5,
            this.mnuGoTo});
            this.mnuEdit.MergeAction = System.Windows.Forms.MergeAction.Insert;
            this.mnuEdit.MergeIndex = 1;
            this.mnuEdit.Name = "mnuEdit";
            resources.ApplyResources(this.mnuEdit, "mnuEdit");
            // 
            // mnuReDo
            // 
            resources.ApplyResources(this.mnuReDo, "mnuReDo");
            this.mnuReDo.Image = global::FreelancerModStudio.Properties.Resources.ReDo;
            this.mnuReDo.Name = "mnuReDo";
            // 
            // mnuUnDo
            // 
            resources.ApplyResources(this.mnuUnDo, "mnuUnDo");
            this.mnuUnDo.Image = global::FreelancerModStudio.Properties.Resources.UnDo;
            this.mnuUnDo.Name = "mnuUnDo";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            // 
            // mnuAdd
            // 
            this.mnuAdd.Image = global::FreelancerModStudio.Properties.Resources.Add;
            this.mnuAdd.Name = "mnuAdd";
            resources.ApplyResources(this.mnuAdd, "mnuAdd");
            this.mnuAdd.Click += new System.EventHandler(this.mnuAdd_Click);
            // 
            // mnuDelete
            // 
            resources.ApplyResources(this.mnuDelete, "mnuDelete");
            this.mnuDelete.Image = global::FreelancerModStudio.Properties.Resources.Delete;
            this.mnuDelete.Name = "mnuDelete";
            this.mnuDelete.Click += new System.EventHandler(this.mnuDelete_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            // 
            // mnuSelectAll
            // 
            this.mnuSelectAll.Name = "mnuSelectAll";
            resources.ApplyResources(this.mnuSelectAll, "mnuSelectAll");
            this.mnuSelectAll.Click += new System.EventHandler(this.mnuSelectAll_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            // 
            // mnuGoTo
            // 
            resources.ApplyResources(this.mnuGoTo, "mnuGoTo");
            this.mnuGoTo.Image = global::FreelancerModStudio.Properties.Resources.GoTo;
            this.mnuGoTo.Name = "mnuGoTo";
            // 
            // frmTableEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectListView1);
            this.Controls.Add(this.menuStrip1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmTableEditor";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.TabText = "Default Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmDefaultEditor_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.FastObjectListView objectListView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuReDo;
        private System.Windows.Forms.ToolStripMenuItem mnuUnDo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem mnuGoTo;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuClose;
        private System.Windows.Forms.ToolStripMenuItem mnuSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuAdd;
        private System.Windows.Forms.ToolStripMenuItem mnuAdd2;
        private System.Windows.Forms.ToolStripMenuItem mnuDelete2;

    }
}