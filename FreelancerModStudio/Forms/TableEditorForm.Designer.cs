namespace FreelancerModStudio
{
    using System.ComponentModel;
    using System.Windows.Forms;

    using BrightIdeasSoftware;

    partial class FrmTableEditor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTableEditor));
            this.objectListView1 = new BrightIdeasSoftware.FastObjectListView();
            this.TableContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFromTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.nullToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCreateTemplateFrom = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).BeginInit();
            this.TableContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // objectListView1
            // 
            this.objectListView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.objectListView1.ContextMenuStrip = this.TableContextMenu;
            resources.ApplyResources(this.objectListView1, "objectListView1");
            this.objectListView1.EmptyListMsgFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.objectListView1.FullRowSelect = true;
            this.objectListView1.GridLines = true;
            this.objectListView1.HideSelection = false;
            this.objectListView1.IsSimpleDragSource = true;
            this.objectListView1.IsSimpleDropSink = true;
            this.objectListView1.Name = "objectListView1";
            this.objectListView1.ShowGroups = false;
            this.objectListView1.ShowImagesOnSubItems = true;
            this.objectListView1.UseCompatibleStateImageBehavior = false;
            this.objectListView1.View = System.Windows.Forms.View.Details;
            this.objectListView1.VirtualMode = true;
            this.objectListView1.CanDrop += new System.EventHandler<BrightIdeasSoftware.OlvDropEventArgs>(this.ObjectListView1CanDrop);
            this.objectListView1.Dropped += new System.EventHandler<BrightIdeasSoftware.OlvDropEventArgs>(this.ObjectListView1Dropped);
            this.objectListView1.SelectionChanged += new System.EventHandler(this.ObjectListView1SelectionChanged);
            // 
            // TableContextMenu
            // 
            this.TableContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAdd,
            this.mnuDelete,
            this.toolStripSeparator2,
            this.mnuCut,
            this.mnuCopy,
            this.mnuPaste,
            this.mnuCreateTemplateFrom});
            this.TableContextMenu.Name = "TableContextMenu";
            resources.ApplyResources(this.TableContextMenu, "TableContextMenu");
            this.TableContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // mnuAdd
            // 
            this.mnuAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFromTemplate});
            this.mnuAdd.Image = global::FreelancerModStudio.Properties.Resources.Add;
            this.mnuAdd.Name = "mnuAdd";
            resources.ApplyResources(this.mnuAdd, "mnuAdd");
            // 
            // mnuFromTemplate
            // 
            this.mnuFromTemplate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nullToolStripMenuItem});
            this.mnuFromTemplate.Image = global::FreelancerModStudio.Properties.Resources.ReportIssue;
            this.mnuFromTemplate.Name = "mnuFromTemplate";
            resources.ApplyResources(this.mnuFromTemplate, "mnuFromTemplate");
            // 
            // nullToolStripMenuItem
            // 
            this.nullToolStripMenuItem.Name = "nullToolStripMenuItem";
            resources.ApplyResources(this.nullToolStripMenuItem, "nullToolStripMenuItem");
            // 
            // mnuDelete
            // 
            resources.ApplyResources(this.mnuDelete, "mnuDelete");
            this.mnuDelete.Image = global::FreelancerModStudio.Properties.Resources.Delete;
            this.mnuDelete.Name = "mnuDelete";
            this.mnuDelete.Click += new System.EventHandler(this.MnuDeleteClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // mnuCut
            // 
            resources.ApplyResources(this.mnuCut, "mnuCut");
            this.mnuCut.Image = global::FreelancerModStudio.Properties.Resources.Cut;
            this.mnuCut.Name = "mnuCut";
            this.mnuCut.Click += new System.EventHandler(this.MnuCutClick);
            // 
            // mnuCopy
            // 
            resources.ApplyResources(this.mnuCopy, "mnuCopy");
            this.mnuCopy.Image = global::FreelancerModStudio.Properties.Resources.Copy;
            this.mnuCopy.Name = "mnuCopy";
            this.mnuCopy.Click += new System.EventHandler(this.MnuCopyClick);
            // 
            // mnuPaste
            // 
            resources.ApplyResources(this.mnuPaste, "mnuPaste");
            this.mnuPaste.Image = global::FreelancerModStudio.Properties.Resources.Paste;
            this.mnuPaste.Name = "mnuPaste";
            this.mnuPaste.Click += new System.EventHandler(this.MnuPasteClick);
            // 
            // mnuCreateTemplateFrom
            // 
            resources.ApplyResources(this.mnuCreateTemplateFrom, "mnuCreateTemplateFrom");
            this.mnuCreateTemplateFrom.Image = global::FreelancerModStudio.Properties.Resources.OpenMod;
            this.mnuCreateTemplateFrom.Name = "mnuCreateTemplateFrom";
            this.mnuCreateTemplateFrom.Click += new System.EventHandler(this.mnuCreateTemplateFrom_Click);
            // 
            // FrmTableEditor
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.objectListView1);
            this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
            this.Name = "FrmTableEditor";
            this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document;
            this.TabText = "Default Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmDefaultEditorFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.objectListView1)).EndInit();
            this.TableContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private FastObjectListView objectListView1;
        public ContextMenuStrip TableContextMenu;
        public ToolStripMenuItem mnuAdd;
        private ToolStripMenuItem mnuDelete;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuCut;
        private ToolStripMenuItem mnuCopy;
        private ToolStripMenuItem mnuPaste;
        public ToolStripMenuItem mnuFromTemplate;
        private ToolStripMenuItem mnuCreateTemplateFrom;
        private ToolStripMenuItem nullToolStripMenuItem;
    }
}