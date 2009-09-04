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

            if (file != null)
            {
                FileManager fileManager = new FileManager(file);
                EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);
                Data = iniContent;
                IsBINI = fileManager.IsBini;

                SetFile(file);
            }
            else
            {
                //todo: newfile function - File is emtpy and causes error
                Data = new EditorINIData(templateIndex);
                IsBINI = false;
            }

            RefreshSettings();
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Properties.Strings.FileEditorEmpty;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(BrightIdeasSoftware.OLVListItem lvi)
            {
                TableData tableData = (TableData)lvi.RowObject;
                if (tableData.Modified == TableModified.Changed)
                    lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedColor;
                else if (tableData.Modified == TableModified.ChangedSaved)
                    lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedSavedColor;
            };

            objectListView1.Refresh();
        }

        private TableData GetTableData(Settings.EditorINIBlock block)
        {
            //name of block
            string blockName = null;
            if (block.MainOptionIndex > -1 && block.Options.Count >= block.MainOptionIndex + 1)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                    blockName = block.Options[block.MainOptionIndex].Values[0].Value.ToString();
                else
                    blockName = block.Name;
            }
            else
            {
                //if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[block.TemplateIndex].Multiple)
                //    blockName = blockName + i.ToString();
                //else
                blockName = block.Name;
            }

            //name of group
            string groupName = null;
            if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[block.TemplateIndex].Multiple)
                groupName = block.Name;
            else
                groupName = Properties.Strings.FileDefaultCategory;

            return new TableData(blockName, groupName, block);
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
                    tableData.Add(GetTableData(Data.Blocks[i]));
            }

            //sort by type and name
            tableData.Sort();
            objectListView1.SetObjects(tableData);

            //add block types to add menu
            for (int i = 0; i < Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Count; i++)
            {
                ToolStripMenuItem addItem = new ToolStripMenuItem();
                addItem.Text = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[i].Name;
                addItem.Tag = i;
                addItem.Click += mnuAddItem_Click;
                this.mnuAdd.DropDownItems.Add(addItem);
            }
            this.mnuAdd2.DropDown = this.mnuAdd.DropDown;
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedDataChanged(GetSelectedBlocks(), Data.TemplateIndex);
            SetMenuEnabled();
        }

        public void Save()
        {
            this.Save(File);
        }

        private void Save(string file)
        {
            FileManager fileManager = new FileManager(file, IsBINI);
            fileManager.Write(Data);

            Modified = false;
            SetFile(file);
            try
            {
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
            if (Modified)
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
                        {
                            if (tableData.Modified == TableModified.Changed)
                            {
                                tableData.Modified = TableModified.ChangedSaved;
                                objectListView1.RefreshObject(tableData);
                            }
                        }
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

        private void AddBlock(string blockName, int templateIndex)
        {
            Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[templateIndex];

            //check if block already exists if it is a single block
            if (!templateBlock.Multiple)
            {
                int i = 0;
                foreach (TableData tableBlock in objectListView1.Objects)
                {
                    if (tableBlock.Block.TemplateIndex == templateIndex)
                    {
                        //block already exists, select it
                        objectListView1.SelectObject(tableBlock);
                        objectListView1.EnsureVisible(i);
                        return;
                    }
                    i++;
                }
            }

            //add options to new block
            EditorINIBlock editorBlock = new EditorINIBlock(blockName, templateIndex);
            for (int i = 0; i < templateBlock.Options.Count; i++)
            {
                Template.Option option = templateBlock.Options[i];
                editorBlock.Options.Add(new EditorINIOption(option.Name, i));

                if (templateBlock.Identifier != null && templateBlock.Identifier.ToLower() == editorBlock.Options[editorBlock.Options.Count - 1].Name.ToLower())
                {
                    editorBlock.MainOptionIndex = editorBlock.Options.Count - 1;
                    editorBlock.Options[editorBlock.Options.Count - 1].Values.Add(new EditorINIEntry(blockName));
                }
            }

            //add actual block
            Data.Blocks.Add(editorBlock);

            TableData newTableData = GetTableData(editorBlock);
            objectListView1.AddObject(newTableData);
            objectListView1.SelectedObject = newTableData;

            //sort (should use TableData.Sort()) + ensure visible (get index somehow) must be changed
            //objectListView1.Sort((BrightIdeasSoftware.OLVColumn)objectListView1.Columns[1], SortOrder.Ascending);
            //objectListView1.EnsureVisible(
            Modified = true;
        }

        public EditorINIBlock[] GetSelectedBlocks()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<EditorINIBlock> blocks = new List<EditorINIBlock>();
            foreach (TableData tableData in objectListView1.SelectedObjects)
                blocks.Add(tableData.Block);

            return blocks.ToArray();
        }

        public void SetBlocks(PropertyBlock[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                TableData tableData = (TableData)objectListView1.SelectedObjects[i];

                for (int j = 0; j < blocks[i].Count; j++)
                {
                    List<EditorINIEntry> options = tableData.Block.Options[j].Values;

                    string text = ((string)blocks[i][j].Value).Trim();
                    if (text != string.Empty)
                    {
                        if (text.Contains(Environment.NewLine))
                        {
                            options.Clear();
                            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                            foreach (string line in lines)
                            {
                                if (line.Trim()[0] != '+')
                                    options.Add(new EditorINIEntry(line.Trim()));
                                else
                                {
                                    if (options.Count > 0)
                                    {
                                        if (options[options.Count - 1].SubOptions == null)
                                            options[options.Count - 1].SubOptions = new List<object>();

                                        options[options.Count - 1].SubOptions.Add(line.Substring(1).Trim());
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (options.Count > 0)
                                options[0].Value = text;
                            else
                                options.Add(new EditorINIEntry(text));
                        }
                    }
                    else
                        options.Clear();

                    //change data in listview
                    if (tableData.Block.MainOptionIndex == j)
                        tableData.Name = text;
                }

                tableData.Modified = TableModified.Changed;
            }

            //objectListView1.RefreshObjects(objectListView1.SelectedObjects);

            //if (itemTextChanged)
            //    objectListView1.BeginUpdate();

            //refresh because of changed modified property (different background color)
            //objectListView1.RefreshSelectedObjects();

            //if (itemTextChanged)
            //{
            //objectListView1.Sort();
            //    objectListView1.EndUpdate();

            //    objectListView1.BeginUpdate();
            //objectListView1.SelectedItem.EnsureVisible();
            //    objectListView1.EnsureVisible(objectListView1.IndexOf(((TableData)objectListView1.SelectedObjects[options[options.Length - 1].PropertyIndex])));
            //    objectListView1.EnsureVisible(objectListView1.IndexOf(((TableData)objectListView1.SelectedObjects[options[0].PropertyIndex])));
            //    objectListView1.EndUpdate();
            //}

            Modified = true;
            //OnSelectedDataChanged(GetSelectedData(), Data.TemplateIndex);
        }

        private void DeleteSelectedBlocks()
        {
            foreach (TableData tableData in objectListView1.SelectedObjects)
                Data.Blocks.Remove(tableData.Block);

            objectListView1.RemoveObjects(objectListView1.SelectedObjects);
            Modified = true;
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
            DeleteSelectedBlocks();
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
            //ContextMenuStrip addMenu = new ContextMenuStrip();

            //foreach (Template.Block block in Helper.Template.Data.Files[Data.TemplateIndex].Blocks)
            //{
            //    ToolStripMenuItem addItem = new ToolStripMenuItem(block.Name);
            //    addItem.Click += mnuAddItem_Click;
            //    addMenu.Items.Add(addItem);
            //}

            //addMenu.Show(this.objectListView1, new Point(0,0));
        }

        private void mnuAddItem_Click(object sender, EventArgs e)
        {
            string blockName = ((ToolStripMenuItem)sender).Text;
            int templateIndex = (int)((ToolStripMenuItem)sender).Tag;

            AddBlock(blockName, templateIndex);
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
        public EditorINIBlock Block;
        public TableModified Modified = TableModified.Normal;

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

        public TableData(string name, string group, EditorINIBlock block)
        {
            mName = name;
            mGroup = group;
            Block = block;
        }

        public int CompareTo(TableData other)
        {
            int groupComparison = this.mGroup.CompareTo(other.Group);
            if (groupComparison == 0)
                return this.mName.CompareTo(other.Name);

            return groupComparison;
        }
    }

    public enum TableModified
    {
        Normal,
        Changed,
        ChangedSaved
    }
}