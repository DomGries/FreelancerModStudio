using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using FreelancerModStudio.Controls;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmMain : Form
    {
        //Mod mod;
        //bool modChanged;

        frmProperties _propertiesForm;
        //frmSolutionExplorer solutionExplorerForm = null;
        frmSystemEditor _systemEditor;
        readonly UICultureChanger _uiCultureChanger = new UICultureChanger();

        public frmMain()
        {
            InitializeComponent();
            Icon = Resources.LogoIcon;

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
            string layoutFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.LayoutPath);
            if (File.Exists(layoutFile))
            {
                // don't throw an error if the layout file can't be loaded
                try
                {
                    dockPanel1.LoadFromXml(layoutFile, GetContentFromPersistString);
                    layoutLoaded = true;
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            if (!layoutLoaded)
            {
                //this.solutionExplorerForm.Show(dockPanel1);
                _propertiesForm.Show(dockPanel1);
            }

            //open files
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; ++i)
            {
                if (File.Exists(arguments[i]))
                {
                    OpenFile(arguments[i]);
                }
            }

            if (dockPanel1.DocumentsCount == 0)
            {
                SetDocumentMenus(false);
            }

            SettingsChanged();
        }

        void InitContentWindows()
        {
            _propertiesForm = new frmProperties();
            //solutionExplorerForm = new frmSolutionExplorer();

            _propertiesForm.OptionsChanged += Properties_OptionsChanged;
            _propertiesForm.ContentChanged += Content_DisplayChanged;

            InitSystemEditor();
        }

        void InitSystemEditor()
        {
            _systemEditor = new frmSystemEditor();
            _systemEditor.SelectionChanged += systemEditor_SelectionChanged;
            _systemEditor.FileOpen += systemEditor_FileOpen;
            _systemEditor.DataManipulated += systemEditor_DataManipulated;
        }

        IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(frmProperties).ToString())
            {
                return _propertiesForm;
            }
            //else if (persistString == typeof(frmSolutionExplorer).ToString())
            //    return solutionExplorerForm;
            else if (persistString == typeof(frmSystemEditor).ToString())
            {
                return _systemEditor;
            }
            else
            {
                string[] parsedStrings = persistString.Split(new[] { ',' });
                if (parsedStrings.Length != 3)
                {
                    return null;
                }

                if (parsedStrings[0] != typeof(frmTableEditor).ToString() || parsedStrings[1].Length == 0 || parsedStrings[2].Length == 0)
                {
                    return null;
                }

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
            Process.Start("http://the-starport.net/freelancer/forum/viewtopic.php?topic_id=2174");
        }

        void mnuReportIssue_Click(object sender, EventArgs e)
        {
            Process.Start("http://code.google.com/p/freelancermodstudio/issues");
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
                {
                    return false;
                }
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
                    {
                        return false;
                    }
                }
                else
                {
                    ++index;
                }
            }

            return true;
        }

        void mnuNewWindow_Click(object sender, EventArgs e)
        {
        }

        void DefaultEditor_DataChanged(ChangedData data)
        {
            if (_systemEditor != null)
            {
                if (data.Type == ChangedType.Add)
                {
                    _systemEditor.Add(data.NewBlocks);
                }
                else if (data.Type == ChangedType.Delete)
                {
                    _systemEditor.Delete(data.NewBlocks);
                }
                else if (data.Type == ChangedType.Edit)
                {
                    _systemEditor.SetValues(data.NewBlocks);
                }
            }
        }

        void DefaultEditor_SelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (data != null)
            {
                _propertiesForm.ShowData(data, templateIndex);

                if (_systemEditor != null)
                {
                    _systemEditor.Select(data[0]);
                }
            }
            else
            {
                _propertiesForm.ClearData();

                if (_systemEditor != null)
                {
                    _systemEditor.Deselect();
                }
            }
        }

        void DefaultEditor_DataVisibilityChanged(TableBlock block)
        {
            if (_systemEditor != null)
            {
                _systemEditor.SetVisibility(block);
            }
        }

        void Properties_OptionsChanged(PropertyBlock[] blocks)
        {
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                tableEditor.SetBlocks(blocks);
            }
        }

        void mnuFullScreen_Click(object sender, EventArgs e)
        {
            FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
        }

        void SetSettings()
        {
            // save layout (don't throw an error if it fails)
            string layoutFile = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.ProductName), Resources.LayoutPath);
            try
            {
                string directory = Path.GetDirectoryName(layoutFile);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                dockPanel1.SaveAsXml(layoutFile);
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }

            if (!Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                Helper.Settings.Data.Data.Forms.Main.Maximized = (WindowState == FormWindowState.Maximized);

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                {
                    WindowState = FormWindowState.Normal;
                }

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
                {
                    Location = Helper.Settings.Data.Data.Forms.Main.Location;
                }

                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(0, 0), true) &&
                    Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                {
                    Size = Helper.Settings.Data.Data.Forms.Main.Size;
                }
            }

            if (Helper.Settings.Data.Data.Forms.Main.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                FullScreen(true);
            }

            DisplayRecentFiles();
        }

        void FullScreen(bool value)
        {
            Helper.Settings.Data.Data.Forms.Main.FullScreen = value;
            mnuFullScreen.Checked = value;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                //show fullscreen
                ToolStripMenuItem fullScreenMenuItem = new ToolStripMenuItem(mnuFullScreen.Text, mnuFullScreen.Image, mnuFullScreen_Click)
                    {
                        Checked = true
                    };
                MainMenuStrip.Items.Add(fullScreenMenuItem);

                Helper.Settings.Data.Data.Forms.Main.Location = Location;
                Helper.Settings.Data.Data.Forms.Main.Size = Size;

                Helper.Settings.Data.Data.Forms.Main.Maximized = (WindowState == FormWindowState.Maximized);

                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                //exit fullscreen
                MainMenuStrip.Items.RemoveAt(MainMenuStrip.Items.Count - 1);

                WindowState = Helper.Settings.Data.Data.Forms.Main.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;

                Location = Helper.Settings.Data.Data.Forms.Main.Location;
                Size = Helper.Settings.Data.Data.Forms.Main.Size;
            }
        }

        void RemoveFromRecentFiles(string file)
        {
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
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
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File.Equals(file, StringComparison.OrdinalIgnoreCase))
                {
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(i);
                    i--;
                }
            }

            //insert new recentfile at first place
            Helper.Settings.Data.Data.Forms.Main.RecentFiles.Insert(0, new Settings.RecentFile
                {
                    File = file,
                    TemplateIndex = templateIndex
                });

            //remove last recentfile to keep ajusted amount of recentfiles
            if (Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > Helper.Settings.Data.Data.General.RecentFilesCount)
            {
                Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1);
            }

            DisplayRecentFiles();
        }

        void DisplayRecentFiles()
        {
            int firstItemIndex = mnuOpen.DropDownItems.IndexOf(mnuRecentFilesSeperator) + 1;

            //remove all recent menuitems
            for (int i = firstItemIndex; i < mnuOpen.DropDownItems.Count; ++i)
            {
                mnuOpen.DropDownItems.RemoveAt(mnuOpen.DropDownItems.Count - 1);
                i--;
            }

            if (Helper.Settings.Data.Data.General.RecentFilesCount > 0 && Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > 0)
            {
                //add recent menuitems
                for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
                {
                    if (i < Helper.Settings.Data.Data.General.RecentFilesCount)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem(Path.GetFileName(Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File), null, mnuLoadRecentFile_Click);

                        menuItem.Tag = Helper.Settings.Data.Data.Forms.Main.RecentFiles[i];

                        if (i == 0)
                        {
                            menuItem.ShortcutKeys = Keys.Control & Keys.Shift & Keys.O;
                        }

                        mnuOpen.DropDownItems.Add(menuItem);
                    }
                }
                mnuRecentFilesSeperator.Visible = true;
            }
            else
            {
                mnuRecentFilesSeperator.Visible = false;
            }
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
                if (MessageBox.Show(String.Format(Strings.FileErrorOpenRecent, file), Helper.Assembly.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    RemoveFromRecentFiles(file);
                }
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
                    {
                        templateIndex = fileTypeForm.FileTypeIndex;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            try
            {
                OpenFile(file, templateIndex);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(String.Format(Strings.FileErrorOpen, file), ex);
            }
        }

        void OpenFile(string file, int templateIndex)
        {
            int documentIndex = FileOpened(file);
            if (documentIndex != -1)
            {
                dockPanel1.DocumentsToArray()[documentIndex].DockHandler.Show();
            }
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
            {
                return;
            }

            /*if (CancelModClose())
            {
                e.Cancel = true;
                return;
            }*/

            SetSettings();
        }

        /*bool CancelModClose()
        {
            if (mod == null || mod.Data.About == null || mod.Data.About.Name == null || !modChanged)
            {
                return false;
            }

            DialogResult dialogResult = MessageBox.Show(String.Format(Strings.FileCloseSave, mod.Data.About.Name), Helper.Assembly.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Cancel)
            {
                return true;
            }
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

            string path = Path.Combine(saveLocation, about.Name);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            mod.Save(Path.Combine(path, about.Name + Resources.ModExtension));
            //this.solutionExplorerForm.ShowProject(this.mod);

            modChanged = false;
        }

        void mnuNewMod_Click(object sender, EventArgs e)
        {
            frmNewMod frmNewMod = new frmNewMod();

            //get saved size
            if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.NewMod.Size, frmNewMod.MinimumSize, true) &&
                Helper.Compare.Size(Helper.Settings.Data.Data.Forms.NewMod.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
            {
                frmNewMod.Size = Helper.Settings.Data.Data.Forms.NewMod.Size;
            }

            //get saved mod save location
            frmNewMod.txtSaveLocation.Text = Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation;

            //set default mod name
            if (Directory.Exists(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation))
            {
                for (int index = 1;; ++index)
                {
                    string modName = String.Format(Strings.ModNewName, index);
                    if (!Directory.Exists(Path.Combine(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation, modName)))
                    {
                        frmNewMod.txtName.Text = modName;
                        break;
                    }
                }
            }
            else
            {
                frmNewMod.txtName.Text = String.Format(Strings.ModNewName, 1);
            }

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
                            Version = Resources.DefaultModVersion,
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
        }*/

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
            {
                BeginInvoke((MethodInvoker)Close);
            }
            else
            {
                e.Cancel = true;
            }
        }

        void mnuOpenMod_Click(object sender, EventArgs e)
        {
        }

        void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            //show properties of document if active document changed
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                if (_systemEditor != null && (!tableEditor.CanDisplay3DViewer() || _systemEditor.IsHidden))
                {
                    CloseSystemEditor();
                }
                else
                {
                    ShowSystemEditor(tableEditor);
                }

                // set selection changed after showing/closing the system editor as this needs to be forwarded to system editor
                DefaultEditor_SelectionChanged(tableEditor.GetSelectedBlocks(), tableEditor.Data.TemplateIndex);
                Document_DisplayChanged(tableEditor);
            }
            else
            {
                DefaultEditor_SelectionChanged(null, 0);

                if (_systemEditor != null)
                {
                    CloseSystemEditor();
                }
            }
        }

        void ShowSystemEditor(frmTableEditor editor)
        {
            if (_systemEditor != null)
            {
                _systemEditor.ShowViewer(editor.ViewerType);

                // set data path before showing the models
                _systemEditor.DataPath = editor.DataPath;

                switch (editor.ViewerType)
                {
                    case ViewerType.System:
                        // set model mode as it was reset if the editor was closed
                        _systemEditor.IsModelMode = mnuShowModels.Checked;
                        _systemEditor.ShowData(editor.Data);
                        break;
                    case ViewerType.Universe:
                        _systemEditor.IsModelMode = false;
                        _systemEditor.ShowData(editor.Data);
                        _systemEditor.ShowUniverseConnections(editor.File, editor.Data.Blocks, editor.Archetype);
                        break;
                    case ViewerType.SolarArchetype:
                    case ViewerType.ModelPreview:
                        _systemEditor.IsModelMode = true;
                        break;
                }

                //select initially
                List<TableBlock> blocks = editor.GetSelectedBlocks();
                if (blocks != null)
                {
                    _systemEditor.Select(blocks[0]);
                }
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
            OpenFileDialog fileOpener = new OpenFileDialog
                {
                    Filter = Strings.FileDialogFilter
                };
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileOpener.FileNames)
                {
                    OpenFile(file);
                }
            }
        }

        void mnuSaveAll_Click(object sender, EventArgs e)
        {
            foreach (IDockContent document in dockPanel1.Documents)
            {
                frmTableEditor tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                {
                    tableEditor.Save();
                }
            }
        }

        void SettingsChanged()
        {
            _uiCultureChanger.ApplyCulture(new CultureInfo(Helper.Settings.ShortLanguage));
            _uiCultureChanger.ApplyCultureToForm(this);

            foreach (IDockContent document in dockPanel1.Contents)
            {
                frmTableEditor tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                {
                    _uiCultureChanger.ApplyCultureToForm(tableEditor);

                    //refresh settings after language change
                    tableEditor.RefreshSettings();
                }
            }

            if (_propertiesForm != null)
            {
                _propertiesForm.RefreshSettings();
            }

            if (_systemEditor != null)
            {
                _systemEditor.RefreshSettings();
            }

            //if (solutionExplorerForm != null)
            //    solutionExplorerForm.RefreshSettings();

            IDockContent activeDocument = dockPanel1.ActiveDocument;
            if (activeDocument != null)
            {
                Document_DisplayChanged((IDocumentForm)activeDocument);
            }
        }

        int FileOpened(string file)
        {
            int i = 0;
            foreach (IDockContent document in dockPanel1.Documents)
            {
                frmTableEditor tableEditor = document as frmTableEditor;
                if (tableEditor != null)
                {
                    if (tableEditor.File.Equals(file, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
                ++i;
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
            {
                OpenFile(file);
            }
        }

        void mnuSolutionExplorer_Click(object sender, EventArgs e)
        {
            //solutionExplorerForm.Show(dockPanel1);
        }

        void mnuProperties_Click(object sender, EventArgs e)
        {
            _propertiesForm.Show(dockPanel1, DockState.DockRight);
        }

        void mnuNewFile_Click(object sender, EventArgs e)
        {
            int templateIndex;

            //let the user choose the ini file type
            frmFileType fileTypeForm = new frmFileType(null);
            if (fileTypeForm.ShowDialog() == DialogResult.OK && fileTypeForm.FileTypeIndex >= 0)
            {
                templateIndex = fileTypeForm.FileTypeIndex;
            }
            else
            {
                return;
            }

            DisplayFile(null, templateIndex);
        }

        void mnuSave_Click(object sender, EventArgs e)
        {
            IDocumentForm document = dockPanel1.ActiveDocument as IDocumentForm;
            if (document != null)
            {
                try
                {
                    document.Save();
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(String.Format(Strings.FileErrorSave, document.Title), ex);
                }
            }
        }

        void mnuSaveAs_Click(object sender, EventArgs e)
        {
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                try
                {
                    tableEditor.SaveAs();

                    AddToRecentFiles(tableEditor.File, tableEditor.Data.TemplateIndex);
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(String.Format(Strings.FileErrorSave, tableEditor.Title), ex);
                }
            }
        }

        IContentForm GetContent()
        {
            IContentForm content = dockPanel1.ActiveContent as IContentForm;
            if (content != null && content.UseDocument())
            {
                content = dockPanel1.ActiveDocument as IContentForm;
                if (content != null)
                {
                    return content;
                }

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
            {
                content.Cut();
            }
        }

        void mnuCopy_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
            {
                content.Copy();
            }
        }

        void mnuPaste_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
            {
                content.Paste();
            }
        }

        void mnuUndo_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
            {
                content.Undo();
            }
        }

        void mnuRedo_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
            {
                content.Redo();
            }
        }

        void mnuClose_Click(object sender, EventArgs e)
        {
            dockPanel1.ActiveContent.DockHandler.Close();
        }

        void mnuDelete_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
            {
                content.Delete();
            }
        }

        void mnuSelectAll_Click(object sender, EventArgs e)
        {
            IContentForm content = GetContent();
            if (content != null)
            {
                content.SelectAll();
            }
        }

        void mnuShowModels_Click(object sender, EventArgs e)
        {
            bool isModelMode = !mnuShowModels.Checked;
            mnuShowModels.Checked = isModelMode;

            if (_systemEditor != null)
            {
                _systemEditor.IsModelMode = isModelMode;
            }
        }

        void mnuManipulationNone_Click(object sender, EventArgs e)
        {
            mnuManipulationNone.Checked = true;
            mnuManipulationTranslate.Checked = false;
            mnuManipulationRotate.Checked = false;
            mnuManipulationScale.Checked = false;

            if (_systemEditor != null)
            {
                _systemEditor.ManipulationMode = SystemPresenter.ManipulationMode.None;
            }
        }

        void mnuManipulationTranslate_Click(object sender, EventArgs e)
        {
            mnuManipulationNone.Checked = false;
            mnuManipulationTranslate.Checked = true;
            mnuManipulationRotate.Checked = false;
            mnuManipulationScale.Checked = false;

            if (_systemEditor != null)
            {
                _systemEditor.ManipulationMode = SystemPresenter.ManipulationMode.Translate;
            }
        }

        void mnuManipulationRotate_Click(object sender, EventArgs e)
        {
            mnuManipulationNone.Checked = false;
            mnuManipulationTranslate.Checked = false;
            mnuManipulationRotate.Checked = true;
            mnuManipulationScale.Checked = false;

            if (_systemEditor != null)
            {
                _systemEditor.ManipulationMode = SystemPresenter.ManipulationMode.Rotate;
            }
        }

        void mnuManipulationScale_Click(object sender, EventArgs e)
        {
            mnuManipulationNone.Checked = false;
            mnuManipulationTranslate.Checked = false;
            mnuManipulationRotate.Checked = false;
            mnuManipulationScale.Checked = true;

            if (_systemEditor != null)
            {
                _systemEditor.ManipulationMode = SystemPresenter.ManipulationMode.Scale;
            }
        }

        void mnuFocusSelected_Click(object sender, EventArgs e)
        {
            if (_systemEditor != null)
            {
                _systemEditor.FocusSelected();
            }
        }

        void mnuTrackSelected_Click(object sender, EventArgs e)
        {
            if (_systemEditor != null)
            {
                _systemEditor.TrackSelected();
            }
        }

        void mnuChangeVisibility_Click(object sender, EventArgs e)
        {
            IDocumentForm content = GetDocument();
            if (content != null)
            {
                content.ChangeVisibility();
            }
        }

        void mnuGoTo_Click(object sender, EventArgs e)
        {
        }

        void CloseSystemEditor()
        {
            //dispose system editor
            _systemEditor.Dispose();
            _systemEditor = null;
        }

        void dockPanel1_ContentAdded(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmTableEditor)
            {
                SetDocumentMenus(true);
            }
        }

        void dockPanel1_ContentRemoved(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmSystemEditor)
            {
                CloseSystemEditor();
            }
            else if (e.Content is frmTableEditor)
            {
                foreach (IDockContent document in dockPanel1.Documents)
                {
                    //there is at least one editor left in documents pane
                    if (document is frmTableEditor)
                    {
                        return;
                    }
                }

                //no editors found
                SetDocumentMenus(false);

                if (_systemEditor != null)
                {
                    _systemEditor.Clear(false, false);
                }
            }
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
                mnu3dEditor.Enabled = false;

                mnuFocusSelected.Visible = false;
                mnuTrackSelected.Visible = false;
                mnuFocusSelectedSeperator.Visible = false;

                mnuShowModels.Visible = false;
                mnuShowModels.Enabled = false;
                mnuChangeVisibility.Visible = false;
                mnuShowModelsSeperator.Visible = false;
            }
        }

        void Document_DisplayChanged(IDocumentForm document)
        {
            if (document == null)
            {
                SetDocumentMenus(false);
                return;
            }

            if (_systemEditor != null)
            {
                _systemEditor.DataPath = document.DataPath;
            }

            if (document.CanSave())
            {
                string title = document.Title;
                mnuSave.Text = String.Format(Strings.FileEditorSave, title);
                mnuSaveAs.Text = String.Format(Strings.FileEditorSaveAs, title);
            }

            mnuUndo.Enabled = document.CanUndo();
            mnuRedo.Enabled = document.CanRedo();

            mnu3dEditor.Enabled = document.CanDisplay3DViewer();

            bool active = document.CanFocusSelected(false);
            mnuFocusSelected.Visible = active;
            mnuFocusSelectedSeperator.Visible = active;
            mnuTrackSelected.Visible = active;

            active = document.CanManipulatePosition();
            mnuManipulationSeperator.Visible = active;
            mnuManipulationNone.Visible = active;
            mnuManipulationNone.Enabled = active;
            mnuManipulationTranslate.Visible = active;
            mnuManipulationTranslate.Enabled = active;
            mnuManipulationRotate.Visible = active;
            mnuManipulationScale.Visible = active;

            active = document.CanManipulateRotationScale();
            mnuManipulationRotate.Enabled = active;
            mnuManipulationScale.Enabled = active;

            active = document.CanChangeVisibility(false);
            mnuShowModels.Visible = active;
            mnuShowModels.Enabled = active;
            mnuChangeVisibility.Visible = active;
            mnuShowModelsSeperator.Visible = active;
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
                mnuTrackSelected.Enabled = false;

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
                    mnuTrackSelected.Enabled = document.CanTrackSelected(true);
                }
                else
                {
                    mnuChangeVisibility.Enabled = false;
                    mnuFocusSelected.Enabled = false;
                    mnuTrackSelected.Enabled = false;
                }

                mnuAdd.DropDown = content.MultipleAddDropDown();
            }
        }

        void dockPanel1_ActiveContentChanged(object sender, EventArgs e)
        {
            Content_DisplayChanged(GetContent());
        }

        void mnu3dEditor_Click(object sender, EventArgs e)
        {
            if (_systemEditor == null)
            {
                InitSystemEditor();
            }

            // system editor is never null as it was initialized above if that was the case
            // ReSharper disable PossibleNullReferenceException
            _systemEditor.Show(dockPanel1);
            // ReSharper restore PossibleNullReferenceException

            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                ShowSystemEditor(tableEditor);
            }
        }

        void systemEditor_FileOpen(string path)
        {
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                string file = string.Empty;

                try
                {
                    string directory = Path.GetDirectoryName(tableEditor.File);
                    if (directory != null)
                    {
                        file = Path.Combine(directory, path);
                        if (File.Exists(file))
                        {
                            OpenFile(file, Helper.Template.Data.SystemFile);
                        }
                        else
                        {
                            throw new FileNotFoundException();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(String.Format(Strings.FileErrorOpen, file), ex);
                }
            }
        }

        void systemEditor_SelectionChanged(TableBlock block, bool toggle)
        {
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                tableEditor.Select(block, toggle);
            }
        }

        void systemEditor_DataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            frmTableEditor tableEditor = dockPanel1.ActiveDocument as frmTableEditor;
            if (tableEditor != null)
            {
                tableEditor.SetBlocks(new List<TableBlock> { newBlock }, new List<TableBlock> { oldBlock });
            }
        }
    }
}
