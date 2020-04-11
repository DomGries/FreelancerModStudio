namespace FreelancerModStudio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    using FLUtils;

    using FreelancerModStudio.Controls;
    using FreelancerModStudio.Data;
    using FreelancerModStudio.Properties;
    using FreelancerModStudio.SystemDesigner;

    using WeifenLuo.WinFormsUI.Docking;

    public partial class MainForm : Form
    {
        // Mod mod;
        // bool modChanged;
        private PropertiesForm propertiesFormForm;

        // frmSolutionExplorer solutionExplorerForm = null;
        private SystemEditorForm systemEditorForm;

        private readonly UiCultureChanger uiCultureChanger = new UiCultureChanger();

        public MainForm()
        {
            this.InitializeComponent();
            this.Icon = Resources.LogoIcon;

            this.GetSettings();

            // initialize content windows after language was set
            this.InitContentWindows();
        }

        private void FrmMainLoad(object sender, EventArgs e)
        {
            // open files
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; ++i)
            {
                if (File.Exists(arguments[i]))
                {
                    this.OpenFile(arguments[i]);
                }
            }

            // load layout
            bool layoutLoaded = false;
            string layoutFile = Path.Combine(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Application.ProductName),
                Resources.LayoutPath);
            if (File.Exists(layoutFile))
            {
                // don't throw an error if the layout file can't be loaded
                try
                {
                    this.dockPanel1.LoadFromXml(layoutFile, this.GetContentFromPersistString);
                    layoutLoaded = true;
                }
                catch
                {
                }
            }

            if (!layoutLoaded)
            {
                // this.solutionExplorerForm.Show(dockPanel1);
                this.propertiesFormForm.Show(this.dockPanel1);
            }

            this.SettingsChanged();
        }

        private void InitContentWindows()
        {
            this.propertiesFormForm = new PropertiesForm();

            // solutionExplorerForm = new frmSolutionExplorer();
            this.propertiesFormForm.OptionsChanged += this.PropertiesOptionsChanged;

            this.InitSystemEditor();
        }

        private void InitSystemEditor()
        {
            this.systemEditorForm = new SystemEditorForm();
            this.systemEditorForm.SelectionChanged += this.SystemEditorSelectionChanged;
            this.systemEditorForm.FileOpen += this.SystemEditorFileOpen;
            this.systemEditorForm.DataManipulated += this.SystemEditorDataManipulated;
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(PropertiesForm).ToString())
                return this.propertiesFormForm;

            // else if (persistString == typeof(frmSolutionExplorer).ToString())
            // return solutionExplorerForm;
            else if (persistString == typeof(SystemEditorForm).ToString())
            {
                // do not open system editor if not a single document could be loaded
                // usually we handle this over active document changed events, but this will not be fired if no document was loaded
                if (this.MdiChildren.Length == 0)
                {
                    return null;
                }

                return this.systemEditorForm;
            }
            else
            {
                string[] parsedStrings = persistString.Split(new[] { ',' });
                if (parsedStrings.Length != 3)
                {
                    return null;
                }

                if (parsedStrings[0] != typeof(FrmTableEditor).ToString() || parsedStrings[1].Length == 0
                                                                          || parsedStrings[2].Length == 0)
                {
                    return null;
                }

                try
                {
                    return this.DisplayFile(parsedStrings[1], Convert.ToInt32(parsedStrings[2]));
                }
                catch
                {
                    return null;
                }
            }
        }

        private void MnuAboutClick(object sender, EventArgs e)
        {
            FrmAbout frmAbout = new FrmAbout();
            frmAbout.ShowDialog();
        }

        private void MnuVisitForumClick(object sender, EventArgs e) =>
            Process.Start("https://github.com/AftermathFreelancer/FLModStudio");

        private void MnuReportIssueClick(object sender, EventArgs e) =>
            Process.Start("https://github.com/AftermathFreelancer/FLModStudio/issues");

        private void MnuCloseAllDocumentsClick(object sender, EventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                child.Close();
                if (!child.IsDisposed)
                    return;
            }
        }

        private void MnuNewWindowClick(object sender, EventArgs e)
        {
        }

        private void DefaultEditorDataChanged(ChangedData data)
        {
            if (this.systemEditorForm != null)
            {
                switch (data.Type)
                {
                    case ChangedType.Add:
                        this.systemEditorForm.Add(data.NewBlocks);
                        break;
                    case ChangedType.Delete:
                        this.systemEditorForm.Delete(data.NewBlocks);
                        break;
                    case ChangedType.Edit:
                        this.systemEditorForm.SetValues(data.NewBlocks);
                        break;
                    case ChangedType.AddAndEdit:
                        this.systemEditorForm.Add(data.NewBlocks);
                        this.systemEditorForm.SetValues(data.NewAdditionalBlocks);
                        break;
                    case ChangedType.DeleteAndEdit:
                        this.systemEditorForm.Delete(data.NewBlocks);
                        this.systemEditorForm.SetValues(data.NewAdditionalBlocks);
                        break;
                }
            }
        }

        private void DefaultEditorSelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (data != null)
            {

                this.propertiesFormForm.ShowData(data, templateIndex);
                this.systemEditorForm?.Deselect();
                foreach (TableBlock block in data)
                    this.systemEditorForm?.Select(block);
            }
            else
            {
                this.propertiesFormForm.ClearData();

                this.systemEditorForm?.Deselect();
            }
        }

        private void DefaultEditorDataVisibilityChanged(TableBlock block)
        {
            this.systemEditorForm?.SetVisibility(block);
        }

        private void DefaultEditorFormClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }

            FrmTableEditor tableEditor = sender as FrmTableEditor;
            if (tableEditor != null)
            {
                this.AddToRecentFiles(tableEditor.File, tableEditor.Data.TemplateIndex);
            }
        }

        private void DefaultEditorDocumentChanged(IDocumentForm document)
        {
            this.SetDocumentMenus(document);

            if (this.dockPanel1.ActiveContent == document)
            {
                this.SetContentMenus(document);
            }
        }

        private void PropertiesOptionsChanged(PropertyBlock[] blocks)
        {
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
            tableEditor?.ChangeBlocks(blocks);
        }

        private void MnuFullScreenClick(object sender, EventArgs e)
        {
            this.FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
        }

        private void SetSettings()
        {
            // save layout (don't throw an error if it fails)
            string layoutFile = Path.Combine(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Application.ProductName),
                Resources.LayoutPath);
            try
            {
                string directory = Path.GetDirectoryName(layoutFile);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                this.dockPanel1.SaveAsXml(layoutFile);
            }

            // ReSharper disable EmptyGeneralCatchClause
            catch
            {
                // ReSharper restore EmptyGeneralCatchClause
            }

            if (!Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                Helper.Settings.Data.Data.Forms.Main.Maximized = (this.WindowState == FormWindowState.Maximized);

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }

                Helper.Settings.Data.Data.Forms.Main.Location = this.Location;
                Helper.Settings.Data.Data.Forms.Main.Size = this.Size;
            }
        }

        private void GetSettings()
        {
            if (Helper.Settings.Data.Data.Forms.Main.Size != new Size(0, 0))
            {
                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(0, 0), true)
                    && Helper.Compare.Size(
                        Helper.Settings.Data.Data.Forms.Main.Location,
                        new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                        false))
                {
                    this.Location = Helper.Settings.Data.Data.Forms.Main.Location;
                }

                if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(0, 0), true)
                    && Helper.Compare.Size(
                        Helper.Settings.Data.Data.Forms.Main.Size,
                        new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                        false))
                {
                    this.Size = Helper.Settings.Data.Data.Forms.Main.Size;
                }
            }

            if (Helper.Settings.Data.Data.Forms.Main.Maximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                this.FullScreen(true);
            }

            this.DisplayRecentFiles();
        }

        private void FullScreen(bool value)
        {
            Helper.Settings.Data.Data.Forms.Main.FullScreen = value;
            this.mnuFullScreen.Checked = value;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                // show fullscreen
                ToolStripMenuItem fullScreenMenuItem = new ToolStripMenuItem(
                                                           this.mnuFullScreen.Text,
                                                           this.mnuFullScreen.Image,
                                                           this.MnuFullScreenClick) { Checked = true };
                this.MainMenuStrip.Items.Add(fullScreenMenuItem);

                Helper.Settings.Data.Data.Forms.Main.Location = this.Location;
                Helper.Settings.Data.Data.Forms.Main.Size = this.Size;

                Helper.Settings.Data.Data.Forms.Main.Maximized = (this.WindowState == FormWindowState.Maximized);

                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                // exit fullscreen
                this.MainMenuStrip.Items.RemoveAt(this.MainMenuStrip.Items.Count - 1);

                this.WindowState = Helper.Settings.Data.Data.Forms.Main.Maximized
                                       ? FormWindowState.Maximized
                                       : FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.Sizable;

                this.Location = Helper.Settings.Data.Data.Forms.Main.Location;
                this.Size = Helper.Settings.Data.Data.Forms.Main.Size;
            }
        }

        private void RemoveFromRecentFiles(string file)
        {
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File == file)
                {
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(i);
                    break;
                }
            }

            this.DisplayRecentFiles();
        }

        private void AddToRecentFiles(string file, int templateIndex)
        {
            // remove double files
            for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File
                    .Equals(file, StringComparison.OrdinalIgnoreCase))
                {
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(i);
                    i--;
                }
            }

            // insert new recentfile at first place
            Helper.Settings.Data.Data.Forms.Main.RecentFiles.Insert(
                0,
                new Settings.RecentFile { File = file, TemplateIndex = templateIndex });

            // remove last recentfile to keep ajusted amount of recentfiles
            if (Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count
                > Helper.Settings.Data.Data.General.RecentFilesCount)
            {
                Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1);
            }

            this.DisplayRecentFiles();
        }

        private void DisplayRecentFiles()
        {
            int firstItemIndex = this.mnuOpen.DropDownItems.IndexOf(this.mnuRecentFilesSeperator) + 1;

            // remove all recent menuitems
            for (int i = firstItemIndex; i < this.mnuOpen.DropDownItems.Count; ++i)
            {
                this.mnuOpen.DropDownItems.RemoveAt(this.mnuOpen.DropDownItems.Count - 1);
                i--;
            }

            if (Helper.Settings.Data.Data.General.RecentFilesCount > 0
                && Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > 0)
            {
                // add recent menuitems
                for (int i = 0; i < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; ++i)
                {
                    if (i < Helper.Settings.Data.Data.General.RecentFilesCount)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem(
                            Path.GetFileName(Helper.Settings.Data.Data.Forms.Main.RecentFiles[i].File),
                            null,
                            this.MnuLoadRecentFileClick);

                        menuItem.Tag = Helper.Settings.Data.Data.Forms.Main.RecentFiles[i];

                        if (i == 0)
                        {
                            menuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.O;
                        }

                        this.mnuOpen.DropDownItems.Add(menuItem);
                    }
                }

                this.mnuRecentFilesSeperator.Visible = true;
            }
            else
            {
                this.mnuRecentFilesSeperator.Visible = false;
            }
        }

        private void MnuLoadRecentFileClick(object sender, EventArgs e)
        {
            Settings.RecentFile recentFile = (Settings.RecentFile)((ToolStripMenuItem)sender).Tag;
            this.OpenRecentFile(recentFile.File, recentFile.TemplateIndex);
        }

        private void OpenRecentFile(string file, int templateIndex)
        {
            try
            {
                this.OpenFile(file, templateIndex);
            }
            catch
            {
                if (MessageBox.Show(
                        string.Format(Strings.FileErrorOpenRecent, file),
                         AssemblyUtils.Name,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.RemoveFromRecentFiles(file);
                }
            }
        }

        private void OpenFile(string file)
        {
            int templateIndex = -1;
            int documentIndex = this.FileOpened(file);
            if (documentIndex == -1)
            {
                templateIndex = Helper.Template.Data.GetIndex(file);
                if (templateIndex == -1)
                {
                    // Let the user choose the ini file type
                    templateIndex = GetTemplateIndexDialog(file);

                    // The user cancelled out of the menu.
                    if (templateIndex == -1)
                        return;
                }
            }

            try
            {
                this.OpenFile(file, templateIndex);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(string.Format(Strings.FileErrorOpen, file), ex);
            }
        }

        private void OpenFile(string file, int templateIndex)
        {
            int documentIndex = this.FileOpened(file);
            if (documentIndex != -1)
            {
                this.dockPanel1.DocumentsToArray()[documentIndex].DockHandler.Show();
            }
            else
            {
                this.DisplayFile(file, templateIndex);
            }
        }

        private FrmTableEditor DisplayFile(string file, int templateIndex)
        {
            this.Text = $"Freelancer Mod Studio - {file}";
            FrmTableEditor tableEditor = new FrmTableEditor(templateIndex, file);
            tableEditor.ShowData();
            tableEditor.TabPageContextMenuStrip = this.contextMenuStrip1;

            tableEditor.DataChanged += this.DefaultEditorDataChanged;
            tableEditor.SelectionChanged += this.DefaultEditorSelectionChanged;
            tableEditor.DataVisibilityChanged += this.DefaultEditorDataVisibilityChanged;
            tableEditor.DocumentChanged += this.DefaultEditorDocumentChanged;
            tableEditor.FormClosed += this.DefaultEditorFormClosed;
            tableEditor.Show(this.dockPanel1, DockState.Document);

            return tableEditor;
        }

        private void FrmMainFormClosing(object sender, FormClosingEventArgs e)
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
            this.SetSettings();
        }

        /*bool CancelModClose()
        {
            if (mod == null || mod.Data.About == null || mod.Data.About.Name == null || !modChanged)
            {
                return false;
            }

            DialogResult dialogResult = MessageBox.Show(string.Format(Strings.FileCloseSave, mod.Data.About.Name), Helper.Assembly.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
                    string modName = string.Format(Strings.ModNewName, index);
                    if (!Directory.Exists(Path.Combine(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation, modName)))
                    {
                        frmNewMod.txtName.Text = modName;
                        break;
                    }
                }
            }
            else
            {
                frmNewMod.txtName.Text = string.Format(Strings.ModNewName, 1);
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
        private void MnuExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MnuCheckUpdateClick(object sender, EventArgs e)
        {
            Helper.Update.Check(false, false);
        }

        private void MnuOpenModClick(object sender, EventArgs e)
        {
        }

        private void DockPanel1ActiveDocumentChanged(object sender, EventArgs e)
        {
            // show properties of document if active document changed
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
            if (tableEditor != null)
            {
                this.Text = $"Freelancer Mod Studio - {tableEditor.File}";
                if (this.systemEditorForm != null
                    && (!tableEditor.CanDisplay3DViewer() || this.systemEditorForm.IsHidden))
                {
                    this.CloseSystemEditor();
                }
                else
                {
                    this.ShowSystemEditor(tableEditor);
                }

                // update property window after changing active document
                this.propertiesFormForm.ShowData(tableEditor.GetSelectedBlocks(), tableEditor.Data.TemplateIndex);
            }
            else
            {
                this.Text = $"Freelancer Mod Studio";
                this.propertiesFormForm.ClearData();

                if (this.systemEditorForm != null)
                {
                    this.CloseSystemEditor();
                }
            }

            this.SetDocumentMenus(tableEditor);
            this.SetContentMenus(tableEditor);
        }

        private void ShowSystemEditor(FrmTableEditor tableEditor)
        {
            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.ShowViewer(tableEditor.ViewerType);

                // set data path before showing the models
                this.systemEditorForm.DataPath = tableEditor.DataPath;

                switch (tableEditor.ViewerType)
                {
                    case ViewerType.System:
                        // set model mode as it was reset if the editor was closed
                        this.systemEditorForm.IsModelMode = this.mnuShowModels.Checked;
                        this.systemEditorForm.ShowData(tableEditor.Data);
                        break;
                    case ViewerType.Universe:
                        this.systemEditorForm.IsModelMode = false;
                        this.systemEditorForm.ShowData(tableEditor.Data);
                        this.systemEditorForm.ShowUniverseConnections(
                            tableEditor.File,
                            tableEditor.Data.Blocks,
                            tableEditor.Archetype);
                        break;
                    case ViewerType.SolarArchetype:
                    case ViewerType.ModelPreview:
                        this.systemEditorForm.IsModelMode = true;
                        break;
                }

                // set manipulation mode as it was reset if the editor was closed
                if (this.mnuManipulationTranslate.Checked)
                {
                    this.systemEditorForm.ManipulationMode = ManipulationMode.Translate;
                }
                else if (this.mnuManipulationRotate.Checked)
                {
                    this.systemEditorForm.ManipulationMode = ManipulationMode.Rotate;
                }
                else if (this.mnuManipulationScale.Checked)
                {
                    this.systemEditorForm.ManipulationMode = ManipulationMode.Scale;
                }
                else
                {
                    this.systemEditorForm.ManipulationMode = ManipulationMode.None;
                }

                // select initially
                List<TableBlock> blocks = tableEditor.GetSelectedBlocks();
                if (blocks != null)
                {
                    foreach (TableBlock block in blocks)
                        this.systemEditorForm.Select(block);
                }
            }
        }

        private void MnuOptionsClick(object sender, EventArgs e)
        {
            OptionsForm optionsFormForm = new OptionsForm();
            optionsFormForm.ShowDialog();

            // check for valid data
            Helper.Settings.Data.Data.General.CheckValidData();

            this.SettingsChanged();
        }

        private void MnuOpenFileClick(object sender, EventArgs e)
        {
            OpenFileDialog fileOpener = new OpenFileDialog { Filter = Strings.FileDialogFilter };
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileOpener.FileNames)
                {
                    this.OpenFile(file);
                }
            }

            fileOpener.Dispose();
        }

        private void MnuSaveAllClick(object sender, EventArgs e)
        {
            foreach (IDockContent document in this.dockPanel1.Documents)
            {
                FrmTableEditor tableEditor = document as FrmTableEditor;
                tableEditor?.Save();
            }
        }

        private void SettingsChanged()
        {
            this.uiCultureChanger.ApplyCulture(new CultureInfo(Helper.Settings.ShortLanguage));
            this.uiCultureChanger.ApplyCultureToForm(this);

            foreach (IDockContent dockContent in this.dockPanel1.Contents)
            {
                FrmTableEditor tableEditor = dockContent as FrmTableEditor;
                if (tableEditor != null)
                {
                    this.uiCultureChanger.ApplyCultureToForm(tableEditor);

                    // refresh settings after language change
                    tableEditor.RefreshSettings();
                }
            }

            this.propertiesFormForm?.RefreshSettings();

            this.systemEditorForm?.RefreshSettings();

            // if (solutionExplorerForm != null)
            // solutionExplorerForm.RefreshSettings();
            this.SetDocumentMenus(this.GetDocument());
        }

        private int FileOpened(string file)
        {
            int i = 0;
            foreach (IDockContent document in this.dockPanel1.Documents)
            {
                FrmTableEditor tableEditor = document as FrmTableEditor;
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

        private void FrmMainDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void FrmMainDragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                this.OpenFile(file);
            }
        }

        private void MnuSolutionExplorerClick(object sender, EventArgs e)
        {
            // solutionExplorerForm.Show(dockPanel1);
        }

        private void MnuPropertiesClick(object sender, EventArgs e)
        {
            this.propertiesFormForm.Show(this.dockPanel1, DockState.DockRight);
        }

        private void MnuNewFileClick(object sender, EventArgs e)
        {
            // let the user choose the ini file type
            int templateIndex = GetTemplateIndexDialog(null);
            if (templateIndex != -1)
            {
                this.DisplayFile(null, templateIndex);
            }
        }

        private void MnuSaveClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            if (document != null)
            {
                try
                {
                    document.Save();
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(string.Format(Strings.FileErrorSave, document.GetTitle()), ex);
                }
            }
        }

        private void MnuSaveAsClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            if (document != null)
            {
                try
                {
                    document.SaveAs();
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(string.Format(Strings.FileErrorSave, document.GetTitle()), ex);
                }
            }
        }

        private IDocumentForm GetDocument()
        {
            return this.dockPanel1.ActiveDocument as IDocumentForm;
        }

        private void MnuCutClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Cut();
        }

        private void MnuCopyClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Copy();
        }

        private void MnuPasteClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Paste();
        }

        private void MnuUndoClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Undo();
        }

        private void MnuRedoClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Redo();
        }

        private void MnuCloseClick(object sender, EventArgs e) => this.dockPanel1.ActiveDocument?.DockHandler.Close();

        private void MnuDeleteClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.Delete();
        }

        private void MnuSelectAllClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            document?.SelectAll();
        }

        private void MnuShowModelsClick(object sender, EventArgs e)
        {
            bool isModelMode = !this.mnuShowModels.Checked;
            this.mnuShowModels.Checked = isModelMode;

            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.IsModelMode = isModelMode;
            }
        }

        private void MnuManipulationNoneClick(object sender, EventArgs e)
        {
            this.mnuManipulationNone.Checked = true;
            this.mnuManipulationTranslate.Checked = false;
            this.mnuManipulationRotate.Checked = false;
            this.mnuManipulationScale.Checked = false;

            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.ManipulationMode = ManipulationMode.None;
            }
        }

        private void MnuManipulationTranslateClick(object sender, EventArgs e)
        {
            this.mnuManipulationNone.Checked = false;
            this.mnuManipulationTranslate.Checked = true;
            this.mnuManipulationRotate.Checked = false;
            this.mnuManipulationScale.Checked = false;

            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.ManipulationMode = ManipulationMode.Translate;
            }
        }

        private void MnuManipulationRotateClick(object sender, EventArgs e)
        {
            this.mnuManipulationNone.Checked = false;
            this.mnuManipulationTranslate.Checked = false;
            this.mnuManipulationRotate.Checked = true;
            this.mnuManipulationScale.Checked = false;

            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.ManipulationMode = ManipulationMode.Rotate;
            }
        }

        private void MnuManipulationScaleClick(object sender, EventArgs e)
        {
            this.mnuManipulationNone.Checked = false;
            this.mnuManipulationTranslate.Checked = false;
            this.mnuManipulationRotate.Checked = false;
            this.mnuManipulationScale.Checked = true;

            if (this.systemEditorForm != null)
            {
                this.systemEditorForm.ManipulationMode = ManipulationMode.Scale;
            }
        }

        private void MnuFocusSelectedClick(object sender, EventArgs e)
        {
            this.systemEditorForm?.FocusSelected();
        }

        private void MnuLookAtSelectedClick(object sender, EventArgs e)
        {
            this.systemEditorForm?.LookAtSelected();
        }

        private void MnuTrackSelectedClick(object sender, EventArgs e)
        {
            this.systemEditorForm?.TrackSelected();
        }

        private void MnuChangeVisibilityClick(object sender, EventArgs e)
        {
            IDocumentForm content = this.GetDocument();
            content?.ChangeVisibility();
        }

        private void MnuGoToClick(object sender, EventArgs e)
        {
        }

        private void MnuCloseOtherClick(object sender, EventArgs e)
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child != this.dockPanel1.ActiveDocument)
                {
                    child.Close();
                    if (!child.IsDisposed)
                    {
                        return;
                    }
                }
            }
        }

        private void MnuCopyFullPathClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            if (document != null)
            {
                string file = document.File;
                if (!string.IsNullOrEmpty(file))
                {
                    System.Windows.Forms.Clipboard.SetText(file, TextDataFormat.Text);
                }
            }
        }

        private void MnuOpenContainingFolderClick(object sender, EventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            if (document != null)
            {
                string file = document.File;
                if (!string.IsNullOrEmpty(file))
                {
                    string directory = Path.GetDirectoryName(file);
                    if (directory != null && Directory.Exists(directory))
                    {
                        // open the folder in explorer
                        Process.Start(directory);
                    }
                }
            }
        }

        private void CloseSystemEditor()
        {
            // dispose system editor
            this.systemEditorForm.Dispose();
            this.systemEditorForm = null;
        }

        private void DockPanel1ContentRemoved(object sender, DockContentEventArgs e)
        {
            if (e.Content is SystemEditorForm)
            {
                this.CloseSystemEditor();
            }
        }

        private void SetMenuVisible(ToolStripMenuItem menuItem, bool value)
        {
            menuItem.Enabled = value;
            menuItem.Visible = value;
        }

        private void SetMenuVisible(ToolStripMenuItem menuItem, bool value, bool enabled)
        {
            menuItem.Enabled = enabled;
            menuItem.Visible = value;
        }

        private void SetMenuVisible(ToolStripSeparator menuItem, bool value)
        {
            menuItem.Visible = value;
        }

        private void SetDocumentMenus(IDocumentForm document)
        {
            bool isDocument = document != null;
            bool isVisible;
            bool isEnabled;

            if (isDocument && this.systemEditorForm != null)
            {
                this.systemEditorForm.DataPath = document.DataPath;
            }

            this.SetMenuVisible(this.mnuSave, isDocument);
            this.SetMenuVisible(this.mnuSaveAs, isDocument);
            this.SetMenuVisible(this.mnuSaveAll, isDocument);
            this.SetMenuVisible(this.mnuSaveSeperator, isDocument);

            this.SetMenuVisible(this.mnuWindowsSeperator, isDocument);

            this.SetMenuVisible(this.mnuClose, isDocument);
            this.mnuCloseAllDocuments.Enabled = isDocument;

            isVisible = isDocument && document.CanSave();
            this.SetMenuVisible(this.mnuSave, isVisible);
            this.SetMenuVisible(this.mnuSaveAs, isVisible);

            if (isVisible)
            {
                string title = document.GetTitle();
                this.mnuSave.Text = string.Format(Strings.FileEditorSave, title);
                this.mnuSaveAs.Text = string.Format(Strings.FileEditorSaveAs, title);
            }

            this.mnuUndo.Enabled = isDocument && document.CanUndo();
            this.mnuRedo.Enabled = isDocument && document.CanRedo();
            this.mnuCopy.Enabled = isDocument && document.ObjectSelected();
            this.mnuCut.Enabled = isDocument && document.ObjectSelected();
            this.mnuPaste.Enabled = isDocument && document.CanPaste();
            this.mnuAdd.Enabled = isDocument && document.CanAdd();
            this.mnuAdd.DropDown = isDocument ? document.MultipleAddDropDown() : null;
            this.mnuSelectAll.Enabled = isDocument && document.CanSelectAll();

            this.mnu3dEditor.Enabled = isDocument && document.CanDisplay3DViewer();

            isVisible = isDocument && document.CanFocusSelected(false);
            isEnabled = isVisible && document.CanFocusSelected(true);
            this.SetMenuVisible(this.mnuFocusSelected, isVisible, isEnabled);
            this.SetMenuVisible(this.mnuLookAtSelected, isVisible, isEnabled);
            this.SetMenuVisible(this.mnuFocusSelectedSeperator, isVisible);

            isVisible = isDocument && document.CanTrackSelected(false);
            isEnabled = isVisible && document.CanTrackSelected(true);
            this.SetMenuVisible(this.mnuTrackSelected, isVisible, isEnabled);

            isVisible = isDocument && document.CanManipulatePosition();
            isEnabled = isVisible && document.CanManipulateRotationScale();
            this.SetMenuVisible(this.mnuManipulationNone, isVisible);
            this.SetMenuVisible(this.mnuManipulationTranslate, isVisible);
            this.SetMenuVisible(this.mnuManipulationRotate, isVisible, isEnabled);
            this.SetMenuVisible(this.mnuManipulationScale, isVisible, isEnabled);
            this.SetMenuVisible(this.mnuManipulationSeperator, isVisible);

            isVisible = isDocument && document.CanChangeVisibility(false);
            isEnabled = isVisible && document.CanChangeVisibility(true);
            this.SetMenuVisible(this.mnuChangeVisibility, isVisible, isEnabled);
            this.SetMenuVisible(this.mnuShowModels, isVisible);
            this.SetMenuVisible(this.mnuShowModelsSeperator, isVisible);
        }

        private void SetContentMenus(IContentForm content)
        {
            this.mnuDelete.Enabled = content != null && content.CanDelete();
        }

        private void DockPanel1ActiveContentChanged(object sender, EventArgs e)
        {
            IContentForm content = this.dockPanel1.ActiveContent as IContentForm;
            if (content == null)
            {
                content = this.dockPanel1.ActiveDocument as IContentForm;
            }

            this.SetContentMenus(content);
        }

        private void Mnu3dEditorClick(object sender, EventArgs e)
        {
            if (this.systemEditorForm == null)
            {
                this.InitSystemEditor();
            }

            // system editor is never null as it was initialized above if that was the case
            // ReSharper disable PossibleNullReferenceException
            this.systemEditorForm.Show(this.dockPanel1);

            // ReSharper restore PossibleNullReferenceException
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
            if (tableEditor != null)
            {
                this.ShowSystemEditor(tableEditor);
            }
        }

        private void SystemEditorFileOpen(string path)
        {
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
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
                            this.OpenFile(file, Helper.Template.Data.SystemFile);
                        }
                        else
                        {
                            throw new FileNotFoundException();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.Exceptions.Show(string.Format(Strings.FileErrorOpen, file), ex);
                }
            }
        }

        private void SystemEditorSelectionChanged(TableBlock block, bool toggle)
        {
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
            tableEditor?.Select(block, toggle);
        }

        private void SystemEditorDataManipulated(List<TableBlock> newBlock, List<TableBlock> oldBlock)
        {
            FrmTableEditor tableEditor = this.dockPanel1.ActiveDocument as FrmTableEditor;
            tableEditor?.ChangeBlocks(newBlock, oldBlock);
        }

        private static int GetTemplateIndexDialog(string file)
        {
            FrmFileType fileTypeForm = new FrmFileType(file);
            if (fileTypeForm.ShowDialog() == DialogResult.OK && fileTypeForm.FileTypeIndex >= 0)
            {
                return fileTypeForm.FileTypeIndex;
            }

            return -1;
        }

        private void ContextMenuStrip1Opening(object sender, CancelEventArgs e)
        {
            IDocumentForm document = this.GetDocument();
            if (document != null)
            {
                bool enabled = !string.IsNullOrEmpty(document.File);
                this.mnuOpenContainingFolder.Enabled = enabled;
                this.mnuCopyFullPath.Enabled = enabled;
            }
        }

        private void MnuCloseAllToLeft_Click(object sender, EventArgs e)
        {
            int index = Array.IndexOf(this.MdiChildren, this.dockPanel1.ActiveDocument);
            if (index == 0 || index == -1)
                return;

            for (int i = --index; i != -1; i--)
            {
                Form f = this.MdiChildren[i];
                f.Close();
                if (!f.IsDisposed)
                    return;
            }
        }

        private void MnuCloseAllToRight_Click(object sender, EventArgs e)
        {
            int index = Array.IndexOf(this.MdiChildren, this.dockPanel1.ActiveDocument);
            if (index == this.MdiChildren.Length - 1 || index == -1)
                return;

            for (int i = ++index; i != this.MdiChildren.Length; i++)
            {
                Form f = this.MdiChildren[i--];
                f.Close();
                if (!f.IsDisposed)
                    return;
            }
        }

        private void MnuCloseAllUnchanged_Click(object sender, EventArgs e)
        {
            List<Form> forms = new List<Form>();
            foreach (var badForm in this.MdiChildren)
            {
                FrmTableEditor form = (FrmTableEditor)badForm;
                if (form.undoManager.IsModified())
                    continue;

                forms.Add(form);
            }

            foreach (var form in forms)
            {
                form.Close();
                if (!form.IsDisposed)
                    return;
            }
        }
    }
}
