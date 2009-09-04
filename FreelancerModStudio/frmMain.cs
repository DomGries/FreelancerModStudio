using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmMain : Form
    {
        Settings.Mod mod;

        bool modChanged = false;

        frmProperties propertiesForm = new frmProperties();
        frmSolutionExplorer solutionExplorerForm = new frmSolutionExplorer();

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;

            this.GetSettings();

            this.propertiesForm.OptionsChanged += Properties_OptionsChanged;

            //display sub forms
            //this.solutionExplorerForm.Show(dockPanel1);
            this.propertiesForm.Show(dockPanel1);

            //open files
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; i++)
            {
                if (System.IO.File.Exists(arguments[i]))
                    OpenFile(arguments[i]);
            }
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            frmAbout frmAbout = new frmAbout();
            frmAbout.ShowDialog();
        }

        private void mnuVisitForum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://groups.google.com/group/freelancer-mod-studio/topics");
        }

        private void mnuReportIssue_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/freelancermodstudio/issues");
        }

        private void mnuCloseAllDocuments_Click(object sender, EventArgs e)
        {
            foreach (Form child in this.MdiChildren)
                child.Close();
        }

        private void CloseOtherDocuments()
        {
            int index = 0;
            while (index < this.dockPanel1.ActiveDocumentPane.Contents.Count)
            {
                IDockContent dockContent = this.dockPanel1.ActiveDocumentPane.Contents[index];

                if (dockContent != this.dockPanel1.ActiveDocumentPane.ActiveContent)
                    dockContent.DockHandler.Close();
                else
                    index++;
            }
        }

        private void mnuNewWindow_Click(object sender, EventArgs e)
        {
        }

        private void DefaultEditor_SelectedDataChanged(Settings.EditorINIBlock[] data, int templateIndex)
        {
            if (data != null)
                propertiesForm.ShowData(data, templateIndex);
            else
                propertiesForm.ClearData();
        }

        private void Properties_OptionsChanged(PropertyBlock[] blocks)
        {
            IDockContent activeDocument = this.dockPanel1.ActiveDocument;
            if (activeDocument != null && activeDocument is frmTableEditor)
            {
                frmTableEditor defaultEditor = (frmTableEditor)activeDocument;
                defaultEditor.SetBlocks(blocks);
            }
        }

        private void mnuFullScreen_Click(object sender, EventArgs e)
        {
            this.FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
        }

        private void SetSettings()
        {
            if (!Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                Helper.Settings.Data.Data.Forms.Main.Maximized = (this.WindowState == FormWindowState.Maximized);

                if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                    this.WindowState = FormWindowState.Normal;

                Helper.Settings.Data.Data.Forms.Main.Location = this.Location;
                Helper.Settings.Data.Data.Forms.Main.Size = this.Size;
            }

            //todo: save language to settings when changing language setting (not at SetSettings)
            Helper.Settings.SetShortLanguage(Application.CurrentCulture.TwoLetterISOLanguageName);
        }

        private void GetSettings()
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

        private void FullScreen(bool value)
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

        private void RemoveFromRecentFiles(string file)
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

        private void AddToRecentFiles(string file, int templateIndex)
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
            Helper.Settings.Data.Data.Forms.Main.RecentFiles.Insert(0, new Settings.Settings.RecentFile(file, templateIndex));

            //remove last recentfile to keep ajusted amount of recentfiles
            if (Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > Helper.Settings.Data.Data.General.RecentFilesCount)
                Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1);

            this.DisplayRecentFiles();
        }

        private void DisplayRecentFiles()
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

        private void mnuLoadRecentFile_Click(object sender, EventArgs e)
        {
            Settings.Settings.RecentFile recentFile = (Settings.Settings.RecentFile)((ToolStripMenuItem)sender).Tag;

            OpenFile(recentFile.File, recentFile.TemplateIndex); try
            {
            }
            catch
            {
                if (MessageBox.Show(String.Format(Properties.Strings.FileErrorOpenRecent, recentFile.File), Helper.Assembly.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    RemoveFromRecentFiles(recentFile.File);
            }
        }

        private void OpenFile(string file)
        {
            int templateIndex = -1;
            int documentIndex = FileOpened(file);
            if (documentIndex == -1)
            {
                templateIndex = Settings.FileManager.GetTemplateIndex(file);
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
            OpenFile(file, templateIndex);
        }

        private void OpenFile(string file, int templateIndex)
        {
            int documentIndex = FileOpened(file);
            if (documentIndex != -1)
                dockPanel1.ActivePane.ActiveContent = dockPanel1.DocumentsToArray()[0];
            else
            {
                frmTableEditor defaultEditor = new frmTableEditor(templateIndex, file);
                defaultEditor.SelectedDataChanged += DefaultEditor_SelectedDataChanged;
                defaultEditor.ShowData();
                defaultEditor.Show(this.dockPanel1, DockState.Document);

                AddToRecentFiles(file, templateIndex);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.CancelDocumentClose())
                e.Cancel = true;
            else
                this.SetSettings();
        }

        private bool CancelDocumentClose()
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

        private bool ModChanged
        {
            get { return this.modChanged; }
            set
            {
                this.modChanged = value;

                if (value)
                    this.Text = this.mod.Data.About.Name + "* - " + Helper.Assembly.Title;
                else
                    this.Text = this.mod.Data.About.Name + " - " + Helper.Assembly.Title;
            }
        }

        private void LoadMod(string file)
        {
            this.ModChanged = false;
            //TODO:Load
        }

        private void CreateMod(Settings.Mod.About about, string saveLocation)
        {
            this.mod = new Settings.Mod(about);

            string path = System.IO.Path.Combine(saveLocation, about.Name);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            this.mod.Save(System.IO.Path.Combine(path, about.Name + Properties.Resources.ModExtension));
            this.solutionExplorerForm.ShowProject(this.mod);

            this.ModChanged = false;
        }

        private void mnuNewMod_Click(object sender, EventArgs e)
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
                if (!this.CancelDocumentClose())
                {
                    //create mod
                    this.CreateMod(new Settings.Mod.About(frmNewMod.txtName.Text, frmNewMod.txtAuthor.Text, Properties.Resources.DefaultModVersion, frmNewMod.txtHomepage.Text, frmNewMod.txtDescription.Text), frmNewMod.txtSaveLocation.Text.Trim());

                    //set mod save location
                    Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation = frmNewMod.txtSaveLocation.Text.Trim();
                }
            }

            //set size
            Helper.Settings.Data.Data.Forms.NewMod.Size = frmNewMod.Size;
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuCheckUpdate_Click(object sender, EventArgs e)
        {
            string proxy = "";
            string username = "";
            string password = "";

            if (Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Enabled)
            {
                proxy = Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Uri;
                username = Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Username;
                password = Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Password;
            }

            AutoUpdate.AutoUpdate autoUpdate = new AutoUpdate.AutoUpdate(proxy, username, password, new Uri(Helper.Settings.Data.Data.General.AutoUpdate.NewestVersionFile), false, false);
            autoUpdate.RestartingApplication += new EventHandler<CancelEventArgs>(this.AutoUpdate_RestartingApplication);

            autoUpdate.Check();
        }

        private void AutoUpdate_RestartingApplication(object sender, CancelEventArgs e)
        {
            if (this.CancelDocumentClose())
                e.Cancel = true;
            else
            {
                this.modChanged = false;
                this.Close();
            }
        }

        private void mnuOpenMod_Click(object sender, EventArgs e)
        {

        }

        private void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            //show properties of document if active document changed
            IDockContent activeDocument = ((DockPanel)sender).ActiveDocument;
            if (activeDocument != null && activeDocument is frmTableEditor)
            {
                frmTableEditor defaultEditor = (frmTableEditor)activeDocument;
                DefaultEditor_SelectedDataChanged(defaultEditor.GetSelectedBlocks(), defaultEditor.Data.TemplateIndex);
            }
            else
                DefaultEditor_SelectedDataChanged(null, 0);
        }

        private void mnuClose_Click(object sender, EventArgs e)
        {
            if (dockPanel1.ActiveDocumentPane != null)
                dockPanel1.ActiveDocumentPane.ActiveContent.DockHandler.Close();
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            frmOptions optionsForm = new frmOptions();
            optionsForm.ShowDialog();

            SettingsChanged();
        }

        private void mnuOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpener = new OpenFileDialog();
            fileOpener.Filter = Properties.Strings.FileDialogFilter;
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in fileOpener.FileNames)
                    OpenFile(file);
            }
        }

        private void mnuSaveAll_Click(object sender, EventArgs e)
        {
            foreach (IDockContent document in dockPanel1.Documents)
            {
                if (document is frmTableEditor)
                    ((frmTableEditor)document).Save();
            }
        }

        private void SettingsChanged()
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
        }

        private void SetDocumentMenus(bool value)
        {
            mnuSaveAll.Visible = value;
            mnuSaveSeperator.Visible = value;
            mnuWindowsSeperator.Visible = value;
            mnuCloseAllDocuments.Visible = value;
        }

        private void dockPanel1_ContentAdded(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmTableEditor)
                SetDocumentMenus(true);
        }

        private void dockPanel1_ContentRemoved(object sender, DockContentEventArgs e)
        {
            if (e.Content is frmTableEditor)
            {
                foreach (IDockContent document in dockPanel1.Documents)
                {
                    //there is at least one editor left in documents pane
                    if (document is frmTableEditor)
                        return;
                }

                //no editors found
                SetDocumentMenus(false);
            }
        }

        private int FileOpened(string file)
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

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                OpenFile(file);
        }

        private void mnuSolutionExplorer_Click(object sender, EventArgs e)
        {
            solutionExplorerForm.Show(dockPanel1);
        }

        private void mnuProperties_Click(object sender, EventArgs e)
        {
            propertiesForm.Show(dockPanel1);
        }

        private void mnuNewFile_Click(object sender, EventArgs e)
        {
            int templateIndex = -1;

            //let the user choose the ini file type
            frmFileType fileTypeForm = new frmFileType(null);
            if (fileTypeForm.ShowDialog() == DialogResult.OK && fileTypeForm.FileTypeIndex >= 0)
                templateIndex = fileTypeForm.FileTypeIndex;
            else
                return;

            frmTableEditor defaultEditor = new frmTableEditor(templateIndex, null);
            defaultEditor.SelectedDataChanged += DefaultEditor_SelectedDataChanged;
            defaultEditor.ShowData();
            defaultEditor.Show(this.dockPanel1, DockState.Document);
        }
    }
}