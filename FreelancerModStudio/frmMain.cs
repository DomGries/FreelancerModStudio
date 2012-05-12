using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FreelancerModStudio.Controls;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmMain : Form
    {
        Mod mod;
        bool modChanged;

        frmProperties propertiesForm;
        //frmSolutionExplorer solutionExplorerForm = null;
        frmSystemEditor systemEditor;

        public frmMain()
        {
            InitializeComponent();
            Icon = Properties.Resources.LogoIcon;

            GetSettings();

            //initialize content windows after language was set
            InitContentWindows();

            //register event to restart app if update was downloaded and button 'Install' pressed
            Helper.Update.AutoUpdate.RestartingApplication += AutoUpdate_RestartingApplication;
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
                    dockPanel1.LoadFromXml(layoutFile, GetContentFromPersistString);
                    layoutLoaded = true;
                }
                catch { }
            }

            if (!layoutLoaded)
            {
                //this.solutionExplorerForm.Show(dockPanel1);
                propertiesForm.Show(dockPanel1);
            }

            //open files
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; i++)
            {
                if (System.IO.File.Exists(arguments[i]))
                    OpenFile(arguments[i]);
            }

            if (dockPanel1.DocumentsCount == 0)
                SetDocumentMenus(false);
        }

        void InitContentWindows()
        {
            propertiesForm = new frmProperties();
            //solutionExplorerForm = new frmSolutionExplorer();

            propertiesForm.OptionsChanged += Properties_OptionsChanged;
            propertiesForm.ContentChanged += Content_DisplayChanged;

            InitSystemEditor();
        }

        void InitSystemEditor()
        {
            systemEditor = new frmSystemEditor();
            systemEditor.SelectionChanged += systemEditor_SelectionChanged;
            systemEditor.FileOpen += systemEditor_FileOpen;
        }

        IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(frmProperties).ToString())
                return propertiesForm;
            //else if (persistString == typeof(frmSolutionExplorer).ToString())
            //    return solutionExplorerForm;
            else if (persistString == typeof(frmSystemEditor).ToString())
                return systemEditor;
            else
            {
                string[] parsedStrings = persistString.Split(new[] { ',' });
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
            foreach (Form child in MdiChildren)
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
            while (index < dockPanel1.ActiveDocumentPane.Contents.Count)
            {
                IDockContent dockContent = dockPanel1.ActiveDocumentPane.Contents[index];

                if (dockContent != dockPanel1.ActiveDocumentPane.ActiveContent)
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
                else if (data.Type == ChangedType.Edit)
                    systemEditor.SetValues(data.NewBlocks);
            }
        }

        void DefaultEditor_SelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (data != null)
            {
                propertiesForm.ShowData(data, templateIndex);

                if (systemEditor != null)
                    systemEditor.Select(data[0]);
            }
            else
            {
                propertiesForm.ClearData();

                if (systemEditor != null)
                    systemEditor.Deselect();
            }
        }

        void DefaultEditor_DataVisibilityChanged(TableBlock block)
        {
            if (systemEditor != null)
                systemEditor.SetVisibility(block);
        }

        void Properties_OptionsChanged(PropertyBlock[] blocks)
        {
            var tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
                tableEditor.SetBlocks(blocks);
        }

        void mnuFullScreen_Click(object sender, EventArgs e)
        {
            FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
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
                Helper.Settings.Data.Data.Forms.Main.Maximized = (WindowState == FormWindowState.Maximized);

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                    WindowState = FormWindowState.Normal;

                Helper.Settings.Data.Data.Forms.Main.Location = Location;
                Helper.Settings.Data.Data.Forms.Main.Size = Size;
            }
        }

        void GetSettings()
        {
            if (Helper.Settings.Data.Data.Forms.Main.Size != new Size(0, 0))
            {
                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(0, 0), true) &&
                    Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                    Location = Helper.Settings.Data.Data.Forms.Main.Location;

                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(0, 0), true) &&
                    Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                    Size = Helper.Settings.Data.Data.Forms.Main.Size;
            }

            if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                WindowState = FormWindowState.Maximized;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
                FullScreen(true);

            uiCultureChanger1.ApplyCulture(new System.Globalization.CultureInfo(Helper.Settings.GetShortLanguage()));

            DisplayRecentFiles();
        }

        void FullScreen(bool value)
        {
            Helper.Settings.Data.Data.Forms.Main.FullScreen = value;
            mnuFullScreen.Checked = value;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                //show fullscreen
                ToolStripMenuItem fullScreenMenuItem = new ToolStripMenuItem(mnuFullScreen.Text, mnuFullScreen.Image, mnuFullScreen_Click);
                fullScreenMenuItem.Checked = true;
                MainMenuStrip.Items.Add(fullScreenMenuItem);

                Helper.Settings.Data.Data.Forms.Main.Location = Location;
                Helper.Settings.Data.Data.Forms.Main.Size = Size;

                Helper.Settings.Data.Data.Forms.Main.Maximized = (WindowState == FormWindowState.Maximized);

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                //exit fullscreen
                MainMenuStrip.Items.RemoveAt(MainMenuStrip.Items.Count - 1);

                FormBorderStyle = FormBorderStyle.Sizable;

                WindowState = Helper.Settings.Data.Data.Forms.Main.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
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
            DisplayRecentFiles();
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

            DisplayRecentFiles();
        }

        void DisplayRecentFiles()
        {
            int firstItemIndex = mnuOpen.DropDownItems.IndexOf(mnuRecentFilesSeperator) + 1;

            //remove all recent menuitems
            for (int i = firstItemIndex; i < mnuOpen.DropDownItems.Count; i++)
            {
                mnuOpen.DropDownItems.RemoveAt(mnuOpen.DropDownItems.Count - 1);
                i--;
            }

            if (Helper.Settings.Data.Data.General.RecentFilesCount > 0 && Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > 0)
            {
                //add recent menuitems
                for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; i++)
                {
                    if (i < Helper.Settings.Data.Data.General.RecentFilesCount)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem(System.IO.Path.GetFileName(Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File), null, mnuLoadRecentFile_Click);

                        menuItem.Tag = Helper.Settings.Data.Data.Forms.Main.RecentFiles[i];

                        if (i == 0)
                            menuItem.ShortcutKeys = Keys.Control & Keys.Shift & Keys.O;

                        mnuOpen.DropDownItems.Add(menuItem);
                    }
                }
                mnuRecentFilesSeperator.Visible = true;
            }
            else
                mnuRecentFilesSeperator.Visible = false;
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
                Helper.Exceptions.Show(String.Format(Properties.Strings.FileErrorOpen, file), ex);
            }
        }

        void OpenFile(string file, int templateIndex)
        {
            int documentIndex = FileOpened(file);
            if (documentIndex != -1)
                dockPanel1.DocumentsToArray()[documentIndex].DockHandler.Show();
            else
            {
                DisplayFile(file, templateIndex);
                AddToRecentFiles(file, templateIndex);
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
            defaultEditor.Show(dockPanel1, DockState.Document);

            return defaultEditor;
        }

        void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
                return;

            if (CancelModClose())
                e.Cancel = true;
            else
                SetSettings();
        }

        bool CancelModClose()
        {
            if (mod == null || mod.Data.About == null || mod.Data.About.Name == null || !modChanged)
                return false;

            DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.FileCloseSave, mod.Data.About.Name), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Cancel)
                return true;
            if (dialogResult == DialogResult.Yes)
            {
                //TODO: save current mod
            }

            return false;
        }

        void LoadMod(string file)
        {
            modChanged = false;
            //TODO:Load
        }

        void CreateMod(Mod.About about, string saveLocation)
        {
            mod = new Mod(about);

            string path = System.IO.Path.Combine(saveLocation, about.Name);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            mod.Save(System.IO.Path.Combine(path, about.Name + Properties.Resources.ModExtension));
            //this.solutionExplorerForm.ShowProject(this.mod);

            modChanged = false;
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
                if (!CancelModClose())
                {
                    //create mod
                    Mod.About about = new Mod.About
                                          {
                        Name = frmNewMod.txtName.Text,
                        Author = frmNewMod.txtAuthor.Text,
                        Version = Properties.Resources.DefaultModVersion,
                        HomePage = frmNewMod.txtHomepage.Text,
                        Description = frmNewMod.txtDescription.Text
                    };
                    CreateMod(about, frmNewMod.txtSaveLocation.Text.Trim());

                    //set mod save location
                    Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation = frmNewMod.txtSaveLocation.Text.Trim();
                }
            }

            //set size
            Helper.Settings.Data.Data.Forms.NewMod.Size = frmNewMod.Size;
        }

        void mnuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        void mnuCheckUpdate_Click(object sender, EventArgs e)
        {
            Helper.Update.Check(false, false);
        }

        void AutoUpdate_RestartingApplication(object sender, CancelEventArgs e)
        {
            //close all MdiChildren and check if user canceled one
            if (CloseAllDocuments())
                BeginInvoke((MethodInvoker)Close);
            else
                e.Cancel = true;
        }

        void mnuOpenMod_Click(object sender, EventArgs e)
        {

        }

        void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            //show properties of document if active document changed
            var tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                if (!tableEditor.CanDisplay3DViewer() && systemEditor != null)
                    CloseSystemEditor();
                else
                    ShowSystemEditor(tableEditor);

                DefaultEditor_SelectionChanged(tableEditor.GetSelectedBlocks(), tableEditor.Data.TemplateIndex);
                Document_DisplayChanged(tableEditor);
            }
            else
            {
                DefaultEditor_SelectionChanged(null, 0);

                if (systemEditor != null)
                    CloseSystemEditor();
            }
        }

        private void ShowSystemEditor(frmTableEditor editor)
        {
            if (systemEditor != null)
            {
                if (editor.ViewerType == ViewerType.Universe)
                    systemEditor.ShowData(editor.Data, editor.File, editor.Archetype);
                else
                    systemEditor.ShowData(editor.Data, editor.File, null);

                //select initially
                List<TableBlock> blocks = editor.GetSelectedBlocks();
                if (blocks != null)
                    systemEditor.Select(blocks[0]);
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
            var fileOpener = new OpenFileDialog { Filter = Properties.Strings.FileDialogFilter };
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
                var tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                    tableEditor.Save();
            }
        }

        void SettingsChanged()
        {
            List<frmTableEditor> editors = new List<frmTableEditor>();
            foreach (IDockContent document in dockPanel1.Contents)
            {
                var tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                {
                    editors.Add(tableEditor);
                    uiCultureChanger1.AddForm((Form)document);
                }
            }

            uiCultureChanger1.ApplyCulture(new System.Globalization.CultureInfo(Helper.Settings.GetShortLanguage()));

            //refresh settings after language change
            foreach (frmTableEditor editor in editors)
                editor.RefreshSettings();

            if (propertiesForm != null)
                propertiesForm.RefreshSettings();

            //if (solutionExplorerForm != null)
            //    solutionExplorerForm.RefreshSettings();

            IDockContent activeDocument = dockPanel1.ActiveDocument;
            if (activeDocument != null)
                Document_DisplayChanged((IDocumentForm)activeDocument);
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

            if (!value)
            {
                mnuChangeVisibility.Visible = false;
                mnuFocusSelected.Visible = false;
                mnu3dEditor.Enabled = false;
            }
        }

        int FileOpened(string file)
        {
            int i = 0;
            foreach (IDockContent document in dockPanel1.Documents)
            {
                var tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                {
                    if (tableEditor.File.ToLower() == file.ToLower())
                        return i;
                }
                i++;
            }
            return -1;
        }

        void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                OpenFile(file);
        }

        void mnuSolutionExplorer_Click(object sender, EventArgs e)
        {
            //solutionExplorerForm.Show(dockPanel1);
        }

        void mnuProperties_Click(object sender, EventArgs e)
        {
            propertiesForm.Show(dockPanel1, DockState.DockRight);
        }

        void mnuNewFile_Click(object sender, EventArgs e)
        {
            int templateIndex;

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
            ((IDocumentForm)dockPanel1.ActiveDocument).Save();
        }

        void mnuSaveAs_Click(object sender, EventArgs e)
        {
            frmTableEditor tableEditor = ((frmTableEditor)dockPanel1.ActiveDocument);
            tableEditor.SaveAs();

            AddToRecentFiles(tableEditor.File, tableEditor.Data.TemplateIndex);
        }

        IContentForm GetContent()
        {
            IContentForm content = dockPanel1.ActiveContent as IContentForm;
            if (content != null && content.UseDocument())
            {
                if (dockPanel1.ActiveDocument != null)
                    return (IContentForm)dockPanel1.ActiveDocument;

                return null;
            }

            return content;
        }

        IDocumentForm GetDocument()
        {
            return dockPanel1.ActiveDocument as IDocumentForm;
        }

        void mnuCut_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
                content.Cut();
        }

        void mnuCopy_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
                content.Copy();
        }

        void mnuPaste_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
                content.Paste();
        }

        void mnuUndo_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
                content.Undo();
        }

        void mnuRedo_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
                content.Redo();
        }

        void mnuClose_Click(object sender, EventArgs e)
        {
            dockPanel1.ActiveContent.DockHandler.Close();
        }

        void mnuAdd_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
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
            IContentForm content = GetContent();
            if (content != null)
                content.Delete();
        }

        void mnuSelectAll_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
                content.SelectAll();
        }

        private void mnuFocusSelected_Click(object sender, EventArgs e)
        {
            if (systemEditor != null)
                systemEditor.FocusSelected();
        }

        private void mnuChangeVisibility_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
                content.ChangeVisibility();
        }

        void mnuGoTo_Click(object sender, EventArgs e)
        {

        }

        void CloseSystemEditor()
        {
            //dispose system editor
            systemEditor.Dispose();
            systemEditor = null;
        }

        void dockPanel1_ContentAdded(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmTableEditor)
                SetDocumentMenus(true);
        }

        void dockPanel1_ContentRemoved(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmSystemEditor)
                CloseSystemEditor();
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

        void Document_DisplayChanged(IDocumentForm document)
        {
            if (document == null)
            {
                SetDocumentMenus(false);
                return;
            }

            if (document.CanSave())
            {
                mnuSave.Text = String.Format(Properties.Strings.FileEditorSave, document.GetTitle());
                mnuSaveAs.Text = String.Format(Properties.Strings.FileEditorSaveAs, document.GetTitle());
            }

            mnuUndo.Enabled = document.CanUndo();
            mnuRedo.Enabled = document.CanRedo();

            mnuChangeVisibility.Visible = document.CanChangeVisibility(false);
            mnuFocusSelected.Visible = document.CanFocusSelected(false);

            toolStripSeparator4.Visible = document.CanChangeVisibility(false) || document.CanFocusSelected(false);

            mnu3dEditor.Enabled = document.CanDisplay3DViewer();
        }

        void Content_DisplayChanged(IContentForm content)
        {
            if (content == null)
            {
                mnuCopy.Enabled = false;
                mnuCut.Enabled = false;
                mnuPaste.Enabled = false;
                mnuAdd.Enabled = false;
                mnuDelete.Enabled = false;
                mnuSelectAll.Enabled = false;

                mnuChangeVisibility.Enabled = false;
                mnuFocusSelected.Enabled = false;

                mnuAdd.DropDown = null;
            }
            else
            {
                mnuCopy.Enabled = content.CanCopy();
                mnuCut.Enabled = content.CanCut();
                mnuPaste.Enabled = content.CanPaste();
                mnuAdd.Enabled = content.CanAdd();
                mnuDelete.Enabled = content.CanDelete();
                mnuSelectAll.Enabled = content.CanSelectAll();

                IDocumentForm document = content as IDocumentForm;
                if (document != null)
                {
                    mnuChangeVisibility.Enabled = document.CanChangeVisibility(true);
                    mnuFocusSelected.Enabled = document.CanFocusSelected(true);
                }
                else
                {
                    mnuChangeVisibility.Enabled = false;
                    mnuFocusSelected.Enabled = false;
                }

                mnuAdd.DropDown = content.MultipleAddDropDown();
            }
        }

        void mnu3dEditor_Click(object sender, EventArgs e)
        {
            if (systemEditor == null)
                InitSystemEditor();

            systemEditor.Show(dockPanel1);

            var tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
                ShowSystemEditor(tableEditor);
        }

        void systemEditor_FileOpen(string path)
        {
            var tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                string file = string.Empty;

                try
                {
                    file = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(tableEditor.File), path);
                    if (System.IO.File.Exists(file))
                        OpenFile(file, Helper.Template.Data.SystemFile);
                    else
                        throw new FileNotFoundException();
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(String.Format(Properties.Strings.FileErrorOpen, file), ex);
                }
            }
        }

        void systemEditor_SelectionChanged(TableBlock block)
        {
            var tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
                tableEditor.Select(block);
        }
    }
}