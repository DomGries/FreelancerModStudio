using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreelancerModStudio.Settings;

namespace FreelancerModStudio
{
    public partial class frmDefaultEditor : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public TemplateINIData Data;
        public string File;
        private bool modified = false;

        public delegate void SelectedDataChangedType(TemplateINIBlock[] data, int templateIndex);
        public SelectedDataChangedType SelectedDataChanged;

        private void OnSelectedDataChanged(TemplateINIBlock[] data, int templateIndex)
        {
            if (this.SelectedDataChanged != null)
                this.SelectedDataChanged(data, templateIndex);
        }

        public frmDefaultEditor(int templateIndex, string file)
        {
            InitializeComponent();

            TemplateINIData iniContent = FileManager.Read(FileEncoding.Automatic, templateIndex, file);
            Data = iniContent;

            SetFile(file);
            RefreshSettings();
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Properties.Strings.FileEditorEmpty;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(BrightIdeasSoftware.OLVListItem lvi)
            {
                EditorData editorData = (EditorData)lvi.RowObject;
                if (editorData.Modified)
                    lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedColor;
            };

            objectListView1.Refresh();
        }

        public void ShowData()
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            objectListView1.Clear();
            objectListView1.ShowGroups = true;

            //add columns
            BrightIdeasSoftware.OLVColumn col = new BrightIdeasSoftware.OLVColumn("Name", "Name");
            col.FillsFreeSpace = true;
            col.GroupKeyGetter = delegate(object row)
            {
                return ((EditorData)row).Group;
            };
            objectListView1.Columns.Add(col);

            //add data
            List<EditorData> editorData = new List<EditorData>();
            for (int i = 0; i < Data.Blocks.Count; i++)
            {
                if (Data.Blocks[i].Options.Count > 0)
                {
                    //name of block
                    string blockName = null;
                    if (Data.Blocks[i].MainOptionIndex > -1 && Data.Blocks[i].Options.Count >= Data.Blocks[i].MainOptionIndex + 1)
                        blockName = Data.Blocks[i].Options[Data.Blocks[i].MainOptionIndex].Value.ToString();
                    else
                    {
                        if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[Data.Blocks[i].TemplateIndex].Multiple)
                            blockName = blockName + i.ToString();
                        else
                            blockName = Data.Blocks[i].Name;
                    }

                    //name of group
                    string groupName = null;
                    if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[Data.Blocks[i].TemplateIndex].Multiple)
                        groupName = Data.Blocks[i].Name;
                    else
                        groupName = Properties.Strings.FileDefaultDategory;

                    editorData.Add(new EditorData(blockName, groupName, i));
                }
            }
            objectListView1.SetObjects(editorData);

#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        public void ShowData(string filter)
        {
            //GENERAL  todo
            objectListView1.Clear();
            objectListView1.ShowGroups = false;

            List<string> uniqueColumns = new List<string>();

            for (int i = 0; i < Data.Blocks.Count; i++)
            {
                if (Data.Blocks[i].Name == filter && Data.Blocks[i].Options.Count > 0)
                {
                    ListViewItem item = new ListViewItem();
                    for (int j = 0; j < Data.Blocks[i].Options.Count; j++)
                    {
                        if (!uniqueColumns.Contains(Data.Blocks[i].Options[j].Name))
                        {
                            uniqueColumns.Add(Data.Blocks[i].Options[j].Name);

                            BrightIdeasSoftware.OLVColumn col = new BrightIdeasSoftware.OLVColumn(Data.Blocks[i].Options[j].Name, Data.Blocks[i].Options[j].Name);
                            col.FillsFreeSpace = true;
                            objectListView1.Columns.Add(col);
                        }

                        if (j == 0)
                            item.Text = Data.Blocks[i].Options[j].Value.ToString();
                        else
                            item.SubItems.Add(Data.Blocks[i].Options[j].Value.ToString());

                    }

                    if (item != null)
                    {
                        item.Tag = i;
                        objectListView1.Items.Add((ListViewItem)item);
                    }
                }
            }
        }

        public TemplateINIBlock[] GetSelectedData()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<TemplateINIBlock> blocks = new List<TemplateINIBlock>();
            foreach (EditorData editorData in objectListView1.SelectedObjects)
                blocks.Add(Data.Blocks[editorData.BlockIndex]);

            return blocks.ToArray();
        }

        public void SetSelectedData(OptionChangedValue[] options)
        {
            bool itemTextChanged = false;

            foreach (OptionChangedValue option in options)
            {
                //change data
                EditorData editorData = (EditorData)objectListView1.SelectedObjects[option.PropertyIndex];
                Data.Blocks[editorData.BlockIndex].Options[option.OptionIndex].Value = option.NewValue;

                editorData.Modified = true;

                //change data in listview
                if (Data.Blocks[editorData.BlockIndex].MainOptionIndex == option.OptionIndex)
                {
                    editorData.Name = option.NewValue.ToString();
                    itemTextChanged = true;
                }
            }

            if (itemTextChanged)
                objectListView1.BeginUpdate();

            //refresh because of changed modified property (different background color)
            objectListView1.RefreshSelectedObjects();

            if (itemTextChanged)
            {
                objectListView1.Sort();
                objectListView1.EndUpdate();

                objectListView1.BeginUpdate();
                objectListView1.EnsureVisible(objectListView1.IndexOf(((EditorData)objectListView1.SelectedObjects[options[options.Length - 1].PropertyIndex])));
                objectListView1.EnsureVisible(objectListView1.IndexOf(((EditorData)objectListView1.SelectedObjects[options[0].PropertyIndex])));
                objectListView1.EndUpdate();
            }

            Modified = true;
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedDataChanged(GetSelectedData(), Data.TemplateIndex);
        }

        public void Save()
        {
            this.Save(File);
        }

        private void Save(string file)
        {
            try
            {
                FileManager.Write(Data, file);

                modified = false;
                SetFile(file);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(ex);
            }
        }

        private void SetFile(string file)
        {
            string fileName = Path.GetFileName(file);
            this.File = file;

            string tabText = fileName;
            if (modified)
                tabText += "*";

            this.TabText = tabText;
            this.Text = tabText;
            this.ToolTipText = File;

            int saveTextIndex = this.mnuSave.Text.IndexOf(' ');
            if (saveTextIndex == -1)
                this.mnuSave.Text += " " + fileName;
            else
                this.mnuSave.Text = this.mnuSave.Text.Substring(0, saveTextIndex) + " " + fileName;
        }

        public bool Modified
        {
            get
            {
                return modified;
            }
            set
            {
                if (modified != value)
                {
                    modified = value;
                    SetFile(File);

                    //set objects in listview as unmodified
                    if (!modified)
                    {
                        foreach (EditorData editorData in objectListView1.Objects)
                            editorData.Modified = false;

                        objectListView1.Refresh();
                    }
                }
            }
        }

        private bool CancelClose()
        {
            if (this.modified)
            {
                DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.FileCloseSave, Path.GetFileName(File)), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                    return true;
                else if (dialogResult == DialogResult.Yes)
                    Save();
            }

            return false;
        }

        private void frmDefaultEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CancelClose();
        }


        private void mnuSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saverDialog = new SaveFileDialog();
            saverDialog.Filter = Properties.Strings.FileDialogFilter;
            if (saverDialog.ShowDialog() == DialogResult.OK)
                this.Save(saverDialog.FileName);
        }

        private void mnuClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {

        }

        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            objectListView1.SelectAll();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            bool selection = objectListView1.SelectedObjects.Count > 0;
        }
    }

    public class EditorData
    {
        private string mName;
        public string Group;
        public int BlockIndex;
        public bool Modified;

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public EditorData(string name, string group, int blockIndex)
        {
            mName = name;
            Group = group;
            BlockIndex = blockIndex;
        }
    }
}