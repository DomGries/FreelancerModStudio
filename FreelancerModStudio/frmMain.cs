using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmMain : Form
    {
        private Settings.Mod mMod;

        private bool mModChanged = false;

        private frmProperties mFrmProperties = new frmProperties();
        private frmSolutionExplorer mFrmSolutionExplorer = new frmSolutionExplorer();

        public frmMain()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.LogoIcon;

            this.GetSettings();

            this.mFrmProperties.Show(dockPanel1);
            this.mFrmSolutionExplorer.Show(dockPanel1);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            frmAbout frmAbout = new frmAbout();
            frmAbout.ShowDialog();
        }

        private void mnuVisitForum_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://groups.google.com/group/freelancer-mod-studio/topics?gvc=2");
        }

        private void mnuReportIssue_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/freelancermodstudio/issues");
        }

        private void mnuCloseAllDocuments_Click(object sender, EventArgs e)
        {
        }

        private void mnuNewWindow_Click(object sender, EventArgs e)
        {
            Settings.INIData iniData = new FreelancerModStudio.Settings.INIData(@"E:\DAT\Visual Studio 2005\Kopie von FreelancerModStudio\templates\DATA\igraph.ini");
            iniData.Read();
            List<Settings.INIGroup> data = iniData.Data;
            iniData.File = iniData.File + "2.ini";
            iniData.Write();
        }

        private void mnuFullScreen_Click(object sender, EventArgs e)
        {
            this.FullScreen(!Helper.Settings.Data.Data.Forms.Main.FullScreen);
        }

        /*private void GetChangedLines(string[] sourceLines, string[] destinationLines)
        {
            List<string> NewLines;
            List<string> DeletedLines;

            foreach (string sourceLine in sourceLines)
            {
                string line = GetLineMatch(sourceLine, destinationLines);

                if (line != null)
                    NewLines.Add(line);
                else
                    DeletedLines.Add(line);
            }
        }

        private string GetLineMatch(string sourceLine, string[] destinationLines)
        {
            foreach (string DestinationLine in destinationLines)
            {
                if (sourceLine.Trim().ToLower() == DestinationLine.Trim().ToLower())
                    return DestinationLine;
            }
            return null;
        }*/

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
            Helper.Settings.Data.Data.General.Language = Application.CurrentCulture.TwoLetterISOLanguageName;
        }

        private void GetSettings()
        {
            if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(0, 0), true) &&
                Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Location, new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                this.Location = Helper.Settings.Data.Data.Forms.Main.Location;

            if (Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(0, 0), true) &&
                Helper.Compare.Size(Helper.Settings.Data.Data.Forms.Main.Size, new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height), false))
                this.Size = Helper.Settings.Data.Data.Forms.Main.Size;

            if (Helper.Settings.Data.Data.Forms.Main.Maximized)
                this.WindowState = FormWindowState.Maximized;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
                this.FullScreen(true);

            if (Helper.Settings.Data.Data.General.Language != null && (Helper.Settings.Data.Data.General.Language.ToLower() == "de" || Helper.Settings.Data.Data.General.Language.ToLower() == "en"))
                this.uiCultureChanger1.ApplyCulture(new System.Globalization.CultureInfo(Helper.Settings.Data.Data.General.Language.ToLower()));

            /*
            if ((frmMain.mManagerSettings.Data.RecentFiles != null) && frmMain.mManagerSettings.Data.RecentFiles.Count > 0)
                this.LoadLibrary(frmMain.mManagerSettings.Data.RecentFiles(0));
            else
                this.DisplayRecentFiles();
            */
        }

        private void FullScreen(bool value)
        {
            Helper.Settings.Data.Data.Forms.Main.FullScreen = value;
            this.mnuFullScreen.Checked = value;

            if (Helper.Settings.Data.Data.Forms.Main.FullScreen)
            {
                //show fullscreen
                ToolStripMenuItem fullScreenMenuItem = new ToolStripMenuItem(this.mnuFullScreen.Text, this.mnuFullScreen.Image, new EventHandler(this.mnuFullScreen_Click));
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

        private void AddToRecentFiles(string file)
        {
            //remove double files
            for (int index = 0; index < Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count; index++)
            {
                if (Helper.Settings.Data.Data.Forms.Main.RecentFiles[index].ToLower() == file.ToLower())
                    Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(index);
                else
                    index += 1;
            }

            //insert new recentfile at first place
            Helper.Settings.Data.Data.Forms.Main.RecentFiles.Insert(0, file);

            //remove last recentfile to keep ajusted amount of recentfiles
            if (Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > Helper.Settings.Data.Data.Forms.Main.RecentFilesCount)
                Helper.Settings.Data.Data.Forms.Main.RecentFiles.RemoveAt(Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1);

            this.DisplayRecentFiles();
        }

        private void DisplayRecentFiles()
        {
            int lastMenuItemIndex = this.mnuOpen.DropDownItems.IndexOf(this.mnuRecentFilesSeperator);

            //remove all recent menuitems
            for (int index = lastMenuItemIndex + 1; index <= this.mnuOpen.DropDownItems.Count - 1; index++)
                this.mnuOpen.DropDownItems.RemoveAt(this.mnuOpen.DropDownItems.Count - 1);

            if (Helper.Settings.Data.Data.Forms.Main.RecentFilesCount > 0 && Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count > 0)
            {
                //add recent menuitems
                for (int index = 0; index <= Helper.Settings.Data.Data.Forms.Main.RecentFiles.Count - 1; index++)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(System.IO.Path.GetFileName(Helper.Settings.Data.Data.Forms.Main.RecentFiles[index]), null, new EventHandler(mnuLoadRecentFile_Click));

                    menuItem.Tag = Helper.Settings.Data.Data.Forms.Main.RecentFiles[index];

                    if (index == 0)
                        menuItem.ShortcutKeys = Keys.Control & Keys.Shift & Keys.O;

                    this.mnuOpen.DropDownItems.Add(menuItem);
                }
                this.mnuRecentFilesSeperator.Visible = true;
            }
            else
                this.mnuRecentFilesSeperator.Visible = false;
        }

        private void mnuLoadRecentFile_Click(object sender, EventArgs e)
        {/*
            string path = (string)((ToolStripMenuItem)sender).Tag;

            if (System.IO.File.Exists(path))
                //loadfile
        */
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
            if (mMod == null || mMod.Data.About == null || mMod.Data.About.Name == null || !this.mModChanged)
                return false;

            DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.CloseSave, mMod.Data.About.Name), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
            get { return this.mModChanged; }
            set
            {
                this.mModChanged = value;

                if (value)
                    this.Text = this.mMod.Data.About.Name + "* - " + Helper.Assembly.Title;
                else
                    this.Text = this.mMod.Data.About.Name + " - " + Helper.Assembly.Title;
            }
        }

        private void LoadMod(string file)
        {
            //TODO:Load
            this.ModChanged = false;
            //TODO:Load
        }

        private void CreateSolutionExplorerTree()
        {

        }

        private void CreateMod(Settings.Mod.About about, string saveLocation)
        {
            this.mMod = new Settings.Mod(about);

            string path = System.IO.Path.Combine(saveLocation, about.Name);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            this.mMod.Save(System.IO.Path.Combine(path, about.Name + Properties.Resources.ModExtension));

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
                    string modName = String.Format(Properties.Strings.NewModName, index);
                    if (!System.IO.Directory.Exists(System.IO.Path.Combine(Helper.Settings.Data.Data.Forms.NewMod.ModSaveLocation, modName)))
                    {
                        frmNewMod.txtName.Text = modName;
                        break;
                    }
                }
            }
            else
                frmNewMod.txtName.Text = String.Format(Properties.Strings.NewModName, 1);

            //show window
            if (frmNewMod.ShowDialog() == DialogResult.OK)
            {
                if (!this.CancelDocumentClose())
                {
                    //create mod
                    this.CreateMod(new Settings.Mod.About(frmNewMod.txtName.Text, frmNewMod.txtAuthor.Text, Properties.Resources.DefaultVersion, frmNewMod.txtHomepage.Text, frmNewMod.txtDescription.Text), frmNewMod.txtSaveLocation.Text.Trim());

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

            if (Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Enabled)
                proxy = Helper.Settings.Data.Data.General.AutoUpdate.Proxy.Uri;

            AutoUpdate.AutoUpdate autoUpdate = new AutoUpdate.AutoUpdate(proxy, "", "", new Uri(Helper.Settings.Data.Data.General.AutoUpdate.NewestVersionFile), false, false);
            autoUpdate.RestartingApplication += new EventHandler<CancelEventArgs>(this.AutoUpdate_RestartingApplication);

            autoUpdate.Check();
        }
        
        private void AutoUpdate_RestartingApplication(object sender, CancelEventArgs e)
        {
            if (this.CancelDocumentClose())
                e.Cancel = true;
            else
            {
                this.mModChanged = false;
                this.Close();
            }
        }
    }
}