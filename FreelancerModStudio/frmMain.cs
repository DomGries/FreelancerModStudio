using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using System.Text;
using System.IO;

namespace FreelancerModStudio
{
    public partial class frmMain : Form
    {
        Mod mod;
        bool modChanged = false;

        frmProperties propertiesForm = new frmProperties();
        frmSolutionExplorer solutionExplorerForm = new frmSolutionExplorer();
        frmSystemEditor systemEditor = null;

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;

            this.GetSettings();

            this.propertiesForm.OptionsChanged += Properties_OptionsChanged;
            this.propertiesForm.ContentChanged += Content_DisplayChanged;

            //register event to restart app if update was downloaded and button 'Install' pressed
            Helper.Update.AutoUpdate.RestartingApplication += new EventHandler<CancelEventArgs>(this.AutoUpdate_RestartingApplication);
        }

        void frmMain_Load(object sender, EventArgs e)
        {
            //load layout
            bool layoutLoaded = false;
            string layoutFile = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.Windows.Forms.Application.ProductName), Properties.Resources.LayoutPath);
            if (System.IO.File.Exists(layoutFile))
            {
                try
                {
                    dockPanel1.LoadFromXml(layoutFile, new DeserializeDockContent(GetContentFromPersistString));
                    layoutLoaded = true;
                }
                catch { }
            }

            if (!layoutLoaded)
            {
                //this.solutionExplorerForm.Show(dockPanel1);
                this.propertiesForm.Show(dockPanel1);
            }

            //open files
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; i++)
            {
                if (System.IO.File.Exists(arguments[i]))
                    OpenFile(arguments[i]);
            }

            if (this.dockPanel1.DocumentsCount == 0)
                SetDocumentMenus(false);
        }

        IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(frmProperties).ToString())
                return propertiesForm;
            else if (persistString == typeof(frmSolutionExplorer).ToString())
                return propertiesForm;
            else
            {
                string[] parsedStrings = persistString.Split(new char[] { ',' });
                if (parsedStrings.Length != 3)
                    return null;

                if (parsedStrings[0] != typeof(frmTableEditor).ToString() || parsedStrings[1] == string.Empty || parsedStrings[2] == string.Empty)
                    return null;

                return DisplayFile(parsedStrings[1], Convert.ToInt32(parsedStrings[2]));
            }
        }

        void mnuAbout_Click(object sender, EventArgs e)
        {
            frmAbout frmAbout = new frmAbout();
            frmAbout.ShowDialog();
        }

        void mnuVisitForum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://the-starport.net/freelancer/forum/viewtopic.php?topic_id=2174");
        }

        void mnuReportIssue_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/freelancermodstudio/issues");
        }

        void mnuCloseAllDocuments_Click(object sender, EventArgs e)
        {
            CloseAllDocuments();
        }

        bool CloseAllDocuments()
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
                if (!child.IsDisposed)
                    return false;
            }

            return true;
        }

        bool CloseOtherDocuments()
        {
            int index = 0;
            while (index < this.dockPanel1.ActiveDocumentPane.Contents.Count)
            {
                IDockContent dockContent = this.dockPanel1.ActiveDocumentPane.Contents[index];

                if (dockContent != this.dockPanel1.ActiveDocumentPane.ActiveContent)
                {
                    dockContent.DockHandler.Close();
                    if (!dockContent.DockHandler.Form.IsDisposed)
                        return false;
                }
                else
                    index++;
            }

            return true;
        }

        void mnuNewWindow_Click(object sender, EventArgs e)
        {
        }

        void DefaultEditor_DataChanged(ChangedData data)
        {
            if (systemEditor != null)
            {
                if (data.Type == ChangedType.Add)
                    systemEditor.Add(data.NewBlocks);
                else if (data.Type == ChangedType.Delete)
                    systemEditor.Delete(data.NewBlocks);
                else
                    systemEditor.SetValues(data.NewBlocks);
            }
        }

        void DefaultEditor_SelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (data != null)
            {
                propertiesForm.ShowData(data, templateIndex);
                if (systemEditor != null)
                    systemEditor.Select(data[0].ID);
            }
            else
            {
                propertiesForm.ClearData();

                if (systemEditor != null)
                    systemEditor.Deselect();
            }
        }

        void DefaultEditor_DataVisibilityChanged(TableBlock block, bool visible)
        {
            if (systemEditor != null)
                systemEditor.SetVisibility(block.ID, visible);
        }

        void Properties_OptionsChanged(PropertyBlock[] blocks)
        {
            IDockContent activeDocument = this.dockPanel1.ActiveDocument;
            if (activeDocument != null && activeDocument is frmTableEditor)
            {
                frmTableEditor defaultEditor = (frmTableEditor)activeDocument;
                defaultEditor.SetBlocks(blocks);
            }
        }

        void mnuFullScreen_Click(object sender, EventArgs e)
        {
            this.FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
        }

        void SetSettings()
        {
            //save layout
            string layoutFile = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), System.Windows.Forms.Application.ProductName), Properties.Resources.LayoutPath);
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(layoutFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(layoutFile));

                dockPanel1.SaveAsXml(layoutFile);
            }
            catch { }

            if (!Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                Helper.Settings.Data.Data.Forms.Main.Maximized = (this.WindowState == FormWindowState.Maximized);

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                    this.WindowState = FormWindowState.Normal;

                Helper.Settings.Data.Data.Forms.Main.Location = this.Location;
                Helper.Settings.Data.Data.Forms.Main.Size = this.Size;
            }

            Helper.Settings.SetShortLanguage(Application.CurrentCulture.TwoLetterISOLanguageName);
        }

        void GetSettings()
        {
            if (Helper.Settings.Data.Data.Forms.Main.Size != new Size(0, 0))
            {
                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(0, 0), true) &&
                    Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                    this.Location = Helper.Settings.Data.Data.Forms.Main.Location;

                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(0, 0), true) &&
                    Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                    this.Size = Helper.Settings.Data.Data.Forms.Main.Size;
            }

            if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                this.WindowState = FormWindowState.Maximized;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
                this.FullScreen(true);

            this.uiCultureChanger1.ApplyCulture(new System.Globalization.CultureInfo(Helper.Settings.GetShortLanguage()));

            this.DisplayRecentFiles();
        }

        void FullScreen(bool value)
        {
            Helper.Settings.Data.Data.Forms.Main.FullScreen = value;
            this.mnuFullScreen.Checked = value;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                //show fullscreen
                ToolStripMenuItem fullScreenMenuItem = new ToolStripMenuItem(this.mnuFullScreen.Text, this.mnuFullScreen.Image, new EventHandler(this.mnuFullScreen_Click));
                fullScreenMenuItem.Checked = true;
                this.MainMenuStrip.Items.Add(fullScreenMenuItem);

                Helper.Settings.Data.Data.Forms.Main.Location = this.Location;
                Helper.Settings.Data.Data.Forms.Main.Size = this.Size;

                Helper.Settings.Data.Data.Forms.Main.Maximized = (this.WindowState == FormWindowState.Maximized);

                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                //eixt fullscreen
                this.MainMenuStrip.Items.RemoveAt(this.MainMenuStrip.Items.Count - 1);

                this.FormBorderStyle = FormBorderStyle.Sizable;

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                    this.WindowState = FormWindowState.Maximized;
                else
                    this.WindowState = FormWindowState.Normal;
            }
        }

        void RemoveFromRecentFiles(string file)
        {
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; i++)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File == file)
                {
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(i);
                    break;
                }
            }
            this.DisplayRecentFiles();
        }

        void AddToRecentFiles(string file, int templateIndex)
        {
            //remove double files
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; i++)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File.ToLower() == file.ToLower())
                {
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(i);
                    i--;
                }
            }

            //insert new recentfile at first place
            Helper.Settings.Data.Data.Forms.Main.RecentFiles.Insert(0, new Settings.RecentFile(file, templateIndex));

            //remove last recentfile to keep ajusted amount of recentfiles
            if (Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > Helper.Settings.Data.Data.General.RecentFilesCount)
                Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1);

            this.DisplayRecentFiles();
        }

        void DisplayRecentFiles()
        {
            int firstItemIndex = this.mnuOpen.DropDownItems.IndexOf(this.mnuRecentFilesSeperator) + 1;

            //remove all recent menuitems
            for (int i = firstItemIndex; i < this.mnuOpen.DropDownItems.Count; i++)
            {
                this.mnuOpen.DropDownItems.RemoveAt(this.mnuOpen.DropDownItems.Count - 1);
                i--;
            }

            if (Helper.Settings.Data.Data.General.RecentFilesCount > 0 && Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > 0)
            {
                //add recent menuitems
                for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; i++)
                {
                    if (i < Helper.Settings.Data.Data.General.RecentFilesCount)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem(System.IO.Path.GetFileName(Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File), null, new EventHandler(mnuLoadRecentFile_Click));

                        menuItem.Tag = Helper.Settings.Data.Data.Forms.Main.RecentFiles[i];

                        if (i == 0)
                            menuItem.ShortcutKeys = Keys.Control & Keys.Shift & Keys.O;

                        this.mnuOpen.DropDownItems.Add(menuItem);
                    }
                }
                this.mnuRecentFilesSeperator.Visible = true;
            }
            else
                this.mnuRecentFilesSeperator.Visible = false;
        }

        void mnuLoadRecentFile_Click(object sender, EventArgs e)
        {
            Settings.RecentFile recentFile = (Settings.RecentFile)((ToolStripMenuItem)sender).Tag;
            OpenRecentFile(recentFile.File, recentFile.TemplateIndex);
        }

        void OpenRecentFile(string file, int templateIndex)
        {
            try
            {
                OpenFile(file, templateIndex);
            }
            catch
            {
                if (MessageBox.Show(String.Format(Properties.Strings.FileErrorOpenRecent, file), Helper.Assembly.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    RemoveFromRecentFiles(file);
            }
        }

        void OpenFile(string file)
        {
            int templateIndex = -1;
            int documentIndex = FileOpened(file);
            if (documentIndex == -1)
            {
                templateIndex = FileManager.GetTemplateIndex(file);
                if (templateIndex == -1)
                {
                    //let the user choose the ini file type
                    frmFileType fileTypeForm = new frmFileType(file);
                    if (fileTypeForm.ShowDialog() == DialogResult.OK && fileTypeForm.FileTypeIndex >= 0)
                        templateIndex = fileTypeForm.FileTypeIndex;
                    else
                        return;
                }

            }

            try
            {
                OpenFile(file, templateIndex);
            }
            catch (Exception ex)
            {
                string error = Environment.NewLine + Environment.NewLine + ex.Message;
                MessageBox.Show(String.Format(Properties.Strings.FileErrorOpen, file, error), Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenFile(string file, int templateIndex)
        {
            int documentIndex = FileOpened(file);
            if (documentIndex != -1)
                dockPanel1.ActiveDocumentPane.ActiveContent = dockPanel1.DocumentsToArray()[documentIndex];
            else
            {
                try
                {
                    DisplayFile(file, templateIndex);
                    AddToRecentFiles(file, templateIndex);
                }
                catch { }
            }
        }

        frmTableEditor DisplayFile(string file, int templateIndex)
        {
            frmTableEditor defaultEditor = new frmTableEditor(templateIndex, file);
            defaultEditor.ShowData();

            defaultEditor.DataChanged += DefaultEditor_DataChanged;
            defaultEditor.SelectionChanged += DefaultEditor_SelectionChanged;
            defaultEditor.DataVisibilityChanged += DefaultEditor_DataVisibilityChanged;
            defaultEditor.ContentChanged += Content_DisplayChanged;
            defaultEditor.DocumentChanged += Document_DisplayChanged;
            defaultEditor.Show(this.dockPanel1, DockState.Document);

            return defaultEditor;
        }

        void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.CancelModClose())
                e.Cancel = true;
            else
                this.SetSettings();
        }

        bool CancelModClose()
        {
            if (mod == null || mod.Data.About == null || mod.Data.About.Name == null || !this.modChanged)
                return false;

            DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.FileCloseSave, mod.Data.About.Name), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Cancel)
                return true;
            else if (dialogResult == DialogResult.Yes)
            {
                //TODO: save current mod
            }

            return false;
        }

        void LoadMod(string file)
        {
            this.modChanged = false;
            //TODO:Load
        }

        void CreateMod(Mod.About about, string saveLocation)
        {
            this.mod = new Mod(about);

            string path = System.IO.Path.Combine(saveLocation, about.Name);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            this.mod.Save(System.IO.Path.Combine(path, about.Name + Properties.Resources.ModExtension));
            this.solutionExplorerForm.ShowProject(this.mod);

            this.modChanged = false;
        }

        void mnuNewMod_Click(object sender, EventArgs e)
        {
            frmNewMod frmNewMod = new frmNewMod();

            //get saved size
            if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.NewMod.Size, frmNewMod.MinimumSize, true) &&
                Helper.Compare.Size(Helper.Settings.Data.Data.Forms.NewMod.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                frmNewMod.Size = Helper.Settings.Data.Data.Forms.NewMod.Size;

            //get saved mod save location
            frmNewMod.txtSaveLocation.Text = Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation;

            //set default mod name
            if (System.IO.Directory.Exists(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation))
            {
                for (int index = 1; ; index++)
                {
                    string modName = String.Format(Properties.Strings.ModNewName, index);
                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation, modName)))
                    {
                        frmNewMod.txtName.Text = modName;
                        break;
                    }
                }
            }
            else
                frmNewMod.txtName.Text = String.Format(Properties.Strings.ModNewName, 1);

            //show window
            if (frmNewMod.ShowDialog() == DialogResult.OK)
            {
                if (!this.CancelModClose())
                {
                    //create mod
                    Mod.About about = new Mod.About()
                    {
                        Name = frmNewMod.txtName.Text,
                        Author = frmNewMod.txtAuthor.Text,
                        Version = Properties.Resources.DefaultModVersion,
                        HomePage = frmNewMod.txtHomepage.Text,
                        Description = frmNewMod.txtDescription.Text
                    };
                    this.CreateMod(about, frmNewMod.txtSaveLocation.Text.Trim());

                    //set mod save location
                    Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation = frmNewMod.txtSaveLocation.Text.Trim();
                }
            }

            //set size
            Helper.Settings.Data.Data.Forms.NewMod.Size = frmNewMod.Size;
        }

        void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void mnuCheckUpdate_Click(object sender, EventArgs e)
        {
            Helper.Update.Check(false, false);
        }

        void AutoUpdate_RestartingApplication(object sender, CancelEventArgs e)
        {
            bool canceled = false;

            //close all MdiChildren and check if user canceled one
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
                if (!child.IsDisposed)
                    canceled = true;
            }

            if (canceled)
                e.Cancel = true;
            else
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    this.Close();
                });
            }
        }

        void mnuOpenMod_Click(object sender, EventArgs e)
        {

        }

        void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            //show properties of document if active document changed
            IDockContent activeDocument = dockPanel1.ActiveDocument;
            if (activeDocument != null && activeDocument is frmTableEditor)
            {
                frmTableEditor defaultEditor = (frmTableEditor)activeDocument;
                ShowSystemEditor(defaultEditor);

                DefaultEditor_SelectionChanged(defaultEditor.GetSelectedBlocks(), defaultEditor.Data.TemplateIndex);
                Document_DisplayChanged((DocumentInterface)activeDocument);
            }
            else
                DefaultEditor_SelectionChanged(null, 0);
        }

        private void ShowSystemEditor(frmTableEditor editor)
        {
            if (systemEditor != null)
            {
                if (editor.IsUniverse)
                    systemEditor.ShowData(editor.Data, editor.File, editor.Archtype);
                else
                    systemEditor.ShowData(editor.Data, editor.File, null);

                //select initially
                List<TableBlock> blocks = editor.GetSelectedBlocks();
                if (blocks != null)
                    systemEditor.Select(blocks[0].ID);
            }
        }

        void mnuOptions_Click(object sender, EventArgs e)
        {
            frmOptions optionsForm = new frmOptions();
            optionsForm.ShowDialog();

            SettingsChanged();
        }

        void mnuOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpener = new OpenFileDialog();
            fileOpener.Filter = Properties.Strings.FileDialogFilter;
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileOpener.FileNames)
                    OpenFile(file);
            }
        }

        void mnuSaveAll_Click(object sender, EventArgs e)
        {
            foreach (IDockContent document in dockPanel1.Documents)
            {
                if (document is frmTableEditor)
                    ((frmTableEditor)document).Save();
            }
        }

        void SettingsChanged()
        {
            List<frmTableEditor> editors = new List<frmTableEditor>();
            foreach (IDockContent document in dockPanel1.Contents)
            {
                if (document is frmTableEditor)
                {
                    editors.Add((frmTableEditor)document);
                    this.uiCultureChanger1.AddForm((Form)document);
                }
            }

            this.uiCultureChanger1.ApplyCulture(new System.Globalization.CultureInfo(Helper.Settings.GetShortLanguage()));

            //refresh settings after language change
            foreach (frmTableEditor editor in editors)
                editor.RefreshSettings();

            if (propertiesForm != null)
                propertiesForm.RefreshSettings();

            if (solutionExplorerForm != null)
                solutionExplorerForm.RefreshSettings();

            IDockContent activeDocument = dockPanel1.ActiveDocument;
            if (activeDocument != null)
                Document_DisplayChanged((DocumentInterface)activeDocument);
        }

        void SetDocumentMenus(bool value)
        {
            mnuSave.Visible = value;
            mnuSaveAs.Visible = value;
            mnuSaveAll.Visible = value;
            mnuSaveSeperator.Visible = value;
            mnuWindowsSeperator.Visible = value;
            mnuClose.Visible = value;

            mnuSave.Enabled = value;
            mnuSaveAs.Enabled = value;
            mnuSaveAll.Enabled = value;
            mnuClose.Enabled = value;
            mnuCloseAllDocuments.Enabled = value;
        }

        int FileOpened(string file)
        {
            int i = 0;
            foreach (IDockContent document in dockPanel1.Documents)
            {
                if (document is frmTableEditor)
                {
                    if (((frmTableEditor)document).File.ToLower() == file.ToLower())
                        return i;
                }
                i++;
            }
            return -1;
        }

        void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                OpenFile(file);
        }

        void mnuSolutionExplorer_Click(object sender, EventArgs e)
        {
            solutionExplorerForm.Show(dockPanel1);
        }

        void mnuProperties_Click(object sender, EventArgs e)
        {
            propertiesForm.Show(dockPanel1, DockState.DockLeft);
        }

        void mnuNewFile_Click(object sender, EventArgs e)
        {
            int templateIndex = -1;

            //let the user choose the ini file type
            frmFileType fileTypeForm = new frmFileType(null);
            if (fileTypeForm.ShowDialog() == DialogResult.OK && fileTypeForm.FileTypeIndex >= 0)
                templateIndex = fileTypeForm.FileTypeIndex;
            else
                return;

            DisplayFile(null, templateIndex);
        }

        void mnuSave_Click(object sender, EventArgs e)
        {
            ((DocumentInterface)this.dockPanel1.ActiveDocument).Save();
        }

        void mnuSaveAs_Click(object sender, EventArgs e)
        {
            frmTableEditor tableEditor = ((frmTableEditor)this.dockPanel1.ActiveDocument);
            tableEditor.SaveAs();

            AddToRecentFiles(tableEditor.File, tableEditor.Data.TemplateIndex);
        }

        ContentInterface GetContent()
        {
            ContentInterface content = this.dockPanel1.ActiveContent as ContentInterface;
            if (content != null && content.UseDocument())
            {
                if (this.dockPanel1.ActiveDocument != null)
                    return (ContentInterface)this.dockPanel1.ActiveDocument;
                else
                    return null;
            }

            return content;
        }

        DocumentInterface GetDocument()
        {
            return this.dockPanel1.ActiveDocument as DocumentInterface;
        }

        void mnuCut_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
                content.Cut();
        }

        void mnuCopy_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
                content.Copy();
        }

        void mnuPaste_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
                content.Paste();
        }

        void mnuUndo_Click(object sender, EventArgs e)
        {
            DocumentInterface content = GetDocument();
            if (content != null)
                content.Undo();
        }

        void mnuRedo_Click(object sender, EventArgs e)
        {
            DocumentInterface content = GetDocument();
            if (content != null)
                content.Redo();
        }

        void mnuClose_Click(object sender, EventArgs e)
        {
            this.dockPanel1.ActiveContent.DockHandler.Close();
        }

        void mnuAdd_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
            {
                int index = 0;
                if (((ToolStripMenuItem)sender).Tag != null)
                    index = (int)((ToolStripMenuItem)sender).Tag;

                content.Add(index);
            }
        }

        void mnuDelete_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
                content.Delete();
        }

        void mnuSelectAll_Click(object sender, EventArgs e)
        {
            ContentInterface content = GetContent();
            if (content != null)
                content.SelectAll();
        }

        private void mnuChangeVisibility_Click(object sender, EventArgs e)
        {
            DocumentInterface content = GetDocument();
            if (content != null)
                content.ChangeVisibility();
        }

        void mnuGoTo_Click(object sender, EventArgs e)
        {

        }

        void dockPanel1_ContentAdded(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmTableEditor)
                SetDocumentMenus(true);
        }

        void dockPanel1_ContentRemoved(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmSystemEditor)
            {
                //dispose system editor
                systemEditor.Dispose();
                systemEditor = null;
            }
            else if (e.Content is frmTableEditor)
            {
                foreach (IDockContent document in dockPanel1.Documents)
                {
                    //there is at least one editor left in documents pane
                    if (document is frmTableEditor)
                        return;
                }

                //no editors found
                SetDocumentMenus(false);

                if (systemEditor != null)
                    systemEditor.Clear();
            }
        }

        void dockPanel1_ActiveContentChanged(object sender, EventArgs e)
        {
            Content_DisplayChanged(GetContent());
        }

        void Document_DisplayChanged(DocumentInterface document)
        {
            if (document == null)
            {
                SetDocumentMenus(false);
                return;
            }

            if (document.CanSave())
            {
                this.mnuSave.Text = String.Format(FreelancerModStudio.Properties.Strings.FileEditorSave, document.GetTitle());
                this.mnuSaveAs.Text = String.Format(FreelancerModStudio.Properties.Strings.FileEditorSaveAs, document.GetTitle());
            }

            this.mnuUndo.Enabled = document.CanUndo();
            this.mnuRedo.Enabled = document.CanRedo();
            this.mnuChangeVisibility.Visible = this.mnuChangeVisibility.Enabled = document.CanChangeVisibility();
        }

        void Content_DisplayChanged(ContentInterface content)
        {
            if (content == null)
            {
                this.mnuCopy.Enabled = false;
                this.mnuCut.Enabled = false;
                this.mnuPaste.Enabled = false;
                this.mnuAdd.Enabled = false;
                this.mnuDelete.Enabled = false;
                this.mnuSelectAll.Enabled = false;
                this.mnuChangeVisibility.Enabled = false;

                this.mnuAdd.Click -= this.mnuAdd_Click;
                this.mnuAdd.DropDown = new ToolStripDropDown();
            }
            else
            {
                this.mnuCopy.Enabled = content.CanCopy();
                this.mnuCut.Enabled = content.CanCut();
                this.mnuPaste.Enabled = content.CanPaste();
                this.mnuAdd.Enabled = content.CanAdd();
                this.mnuDelete.Enabled = content.CanDelete();
                this.mnuSelectAll.Enabled = content.CanSelectAll();
                this.mnuChangeVisibility.Enabled = true;

                this.mnuAdd.Click -= this.mnuAdd_Click;

                if (content.CanAddMultiple())
                    this.mnuAdd.DropDown = content.MultipleAddDropDown();
                else
                {
                    this.mnuAdd.Click += this.mnuAdd_Click;
                    this.mnuAdd.DropDown = new ToolStripDropDown();
                }
            }
        }

        void mnu3dEditor_Click(object sender, EventArgs e)
        {
            if (systemEditor == null)
            {
                systemEditor = new frmSystemEditor();
                systemEditor.SelectionChanged += systemEditor_SelectionChanged;
                systemEditor.FileOpen += systemEditor_FileOpen;

                systemEditor.Show(dockPanel1);
            }
            else
                systemEditor.Show();

            if (dockPanel1.ActiveDocument != null && dockPanel1.ActiveDocument is frmTableEditor)
                ShowSystemEditor((frmTableEditor)dockPanel1.ActiveDocument);
        }

        void systemEditor_FileOpen(string path)
        {
            if (dockPanel1.ActiveDocument != null && dockPanel1.ActiveDocument is frmTableEditor)
            {
                frmTableEditor editor = (frmTableEditor)dockPanel1.ActiveDocument;
                string file = "";

                try
                {
                    file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(editor.File), path);
                    if (System.IO.File.Exists(file))
                        OpenFile(file, Helper.Template.Data.Files.IndexOf("System"));
                    else
                        throw new FileNotFoundException();
                }
                catch (Exception ex)
                {
                    string error = Environment.NewLine + Environment.NewLine + ex.Message;
                    MessageBox.Show(String.Format(Properties.Strings.FileErrorOpen, file, error), Helper.Assembly.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void systemEditor_SelectionChanged(TableBlock block)
        {
            if (dockPanel1.ActiveDocument != null && dockPanel1.ActiveDocument is frmTableEditor)
                ((frmTableEditor)dockPanel1.ActiveDocument).Select(block);
        }
    }
}