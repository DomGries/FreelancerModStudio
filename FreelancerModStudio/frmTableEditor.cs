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
    public partial class frmTableEditor : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public EditorINIData Data;
        public string File { get; set; }
        public bool IsBINI { get; set; }
        private bool modified = false;

        public delegate void SelectedDataChangedType(EditorINIBlock[] data, int templateIndex);
        public SelectedDataChangedType SelectedDataChanged;

        private void OnSelectedDataChanged(EditorINIBlock[] data, int templateIndex)
        {
            if (this.SelectedDataChanged != null)
                this.SelectedDataChanged(data, templateIndex);
        }

        public frmTableEditor(int templateIndex, string file)
        {
            InitializeComponent();
            this.Icon = Properties.Resources.FileINIIcon;

            FileManager fileManager = new FileManager(file);
            EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);
            Data = iniContent;
            IsBINI = fileManager.IsBini;

            SetFile(file);
            RefreshSettings();
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Properties.Strings.FileEditorEmpty;
            objectListView1.AlternateRowBackColor = Helper.Settings.Data.Data.General.EditorAlternateRowColor;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(BrightIdeasSoftware.OLVListItem lvi)
            {
                TableData tableData = (TableData)lvi.RowObject;
                if (tableData.Modified)
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

            //add columns
            BrightIdeasSoftware.OLVColumn[] cols = {
                new BrightIdeasSoftware.OLVColumn("Name", "Name"),
                new BrightIdeasSoftware.OLVColumn("Type", "Group")};

            cols[1].Width = 150;
            cols[0].FillsFreeSpace = true;
            objectListView1.Columns.AddRange(cols);

            //add data
            List<TableData> tableData = new List<TableData>();
            for (int i = 0; i < Data.Blocks.Count; i++)
            {
                if (Data.Blocks[i].Options.Count > 0)
                {
                    //name of block
                    string blockName = null;
                    if (Data.Blocks[i].MainOptionIndex > -1 && Data.Blocks[i].Options.Count >= Data.Blocks[i].MainOptionIndex + 1)
                        blockName = Data.Blocks[i].Options[Data.Blocks[i].MainOptionIndex].Values[0].Value.ToString();
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

                    tableData.Add(new TableData(blockName, groupName, i));
                }
            }

            //sort by type and name
            tableData.Sort();

            objectListView1.SetObjects(tableData);
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        public EditorINIBlock[] GetSelectedData()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<EditorINIBlock> blocks = new List<EditorINIBlock>();
            foreach (TableData tableData in objectListView1.SelectedObjects)
                blocks.Add(Data.Blocks[tableData.BlockIndex]);

            return blocks.ToArray();
        }

        public void SetSelectedData(OptionChangedValue[] options)
        {
            //bool itemTextChanged = false;

            //foreach (OptionChangedValue option in options)
            //{
            //    //change data
            //    EditorData editorData = (EditorData)objectListView1.SelectedObjects[option.PropertyIndex];
            //    List<object> values = Data.Blocks[editorData.BlockIndex].Options[option.OptionIndex].Values;

            //    if (option.NewValue.ToString().Trim() != "" && option.OptionEntryIndex > values.Count - 1)
            //        values.Add(option.NewValue);
            //    else if (option.NewValue.ToString().Trim() == "" && option.OptionEntryIndex < values.Count)
            //        values.RemoveAt(option.OptionEntryIndex);
            //    else if (option.OptionEntryIndex < values.Count)
            //        values[option.OptionEntryIndex] = option.NewValue;
            //    else
            //        return;

            //    editorData.Modified = true;

            //    //change data in listview
            //    if (Data.Blocks[editorData.BlockIndex].MainOptionIndex == option.OptionIndex)
            //    {
            //        editorData.Name = option.NewValue.ToString();
            //        itemTextChanged = true;
            //    }
            //}

            //if (itemTextChanged)
            //    objectListView1.BeginUpdate();

            ////refresh because of changed modified property (different background color)
            //objectListView1.RefreshSelectedObjects();

            //if (itemTextChanged)
            //{
            //    objectListView1.Sort();
            //    objectListView1.EndUpdate();

            //    objectListView1.BeginUpdate();
            //    objectListView1.EnsureVisible(objectListView1.IndexOf(((EditorData)objectListView1.SelectedObjects[options[options.Length - 1].PropertyIndex])));
            //    objectListView1.EnsureVisible(objectListView1.IndexOf(((EditorData)objectListView1.SelectedObjects[options[0].PropertyIndex])));
            //    objectListView1.EndUpdate();
            //}

            //Modified = true;
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedDataChanged(GetSelectedData(), Data.TemplateIndex);
            SetMenuEnabled();
        }

        public void Save()
        {
            this.Save(File);
        }

        private void Save(string file)
        {
            try
            {
                FileManager fileManager = new FileManager(file, IsBINI);
                fileManager.Write(Data);

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
                        foreach (TableData tableData in objectListView1.Objects)
                            tableData.Modified = false;

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
            foreach (TableData tableData in objectListView1.SelectedObjects)
                Data.Blocks.RemoveAt(tableData.BlockIndex);

            objectListView1.RemoveObjects(objectListView1.SelectedObjects);
            Modified = true;
        }

        private void mnuSelectAll_Click(object sender, EventArgs e)
        {
            objectListView1.SelectAll();
        }

        private void SetMenuEnabled()
        {
            bool selection = objectListView1.SelectedObjects.Count > 0;

            mnuDelete.Enabled = selection;
        }

        private void SetContextMenuEnabled()
        {
            bool selection = objectListView1.SelectedObjects.Count > 0;

            mnuAdd2.Visible = !selection;
            mnuDelete2.Enabled = selection;
            mnuDelete2.Visible = selection;
        }

        private void mnuAdd_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SetContextMenuEnabled();
        }
    }

    public class TableData : IComparable<TableData>
    {
        private string mName;
        private string mGroup;
        public int BlockIndex;
        public bool Modified;

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        public string Group
        {
            get { return mGroup; }
            set { mGroup = value; }
        }

        public TableData(string name, string group, int blockIndex)
        {
            mName = name;
            mGroup = group;
            BlockIndex = blockIndex;
        }

        public int CompareTo(TableData other)
        {
            int groupComparison = this.mGroup.CompareTo(other.Group);
            if (groupComparison == 0)
                return this.mName.CompareTo(other.Name);

            return groupComparison;
        }
    }
}