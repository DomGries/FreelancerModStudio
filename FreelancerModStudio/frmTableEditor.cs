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
    public partial class frmTableEditor : WeifenLuo.WinFormsUI.Docking.DockContent, ContentInterface
    {
        public EditorINIData Data;
        public string File { get; set; }
        public bool IsBINI { get; set; }

        private bool modified = false;

        public delegate void SelectedDataChangedType(EditorINIBlock[] data, int templateIndex);
        public SelectedDataChangedType SelectedDataChanged;

        public delegate void DisplayChangedType(ContentInterface content);
        public DisplayChangedType DisplayChanged;

        private void OnSelectedDataChanged(EditorINIBlock[] data, int templateIndex)
        {
            if (this.SelectedDataChanged != null)
                this.SelectedDataChanged(data, templateIndex);
        }

        private void OnDisplayChanged(ContentInterface content)
        {
            if (this.DisplayChanged != null)
                this.DisplayChanged(content);
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
                Data = new EditorINIData(templateIndex);
                IsBINI = false;

                SetFile("");
            }

            RefreshSettings();

            //todo:
            //UndoRedoManager.CommandDone += delegate
            //{
            //    mnuUnDo.Enabled = UndoRedoManager.CanUndo;
            //    mnuReDo.Enabled = UndoRedoManager.CanRedo;
            //};
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

            //refresh column texts
            if (objectListView1.Columns.Count == 2)
            {
                objectListView1.Columns[0].Text = Properties.Strings.FileEditorColumnName;
                objectListView1.Columns[1].Text = Properties.Strings.FileEditorColumnType;
            }

            //update 'New file' to new language
            if (File == "")
                SetFile("");

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
                //todo: different block name if they are all the same
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
                new BrightIdeasSoftware.OLVColumn(Properties.Strings.FileEditorColumnName, "Name"),
                new BrightIdeasSoftware.OLVColumn(Properties.Strings.FileEditorColumnType, "Group")};

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

            //todo: add block types to add menu
            for (int i = 0; i < Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Count; i++)
            {
                ToolStripMenuItem addItem = new ToolStripMenuItem();
                addItem.Text = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[i].Name;
                addItem.Tag = i;
                addItem.Click += mnuAddItem_Click;
                this.mnuAdd.DropDownItems.Add(addItem);
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectedDataChanged(GetSelectedBlocks(), Data.TemplateIndex);

            if (this.DockHandler.IsActivated)
                OnDisplayChanged((ContentInterface)this);
        }

        private void Save(string file)
        {
            FileManager fileManager = new FileManager(file, IsBINI);
            fileManager.Write(Data);

            Modified = false;
            try
            {
                SetFile(file);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(ex);
            }
        }

        private void SetFile(string file)
        {
            string fileName;
            if (file == "")
                fileName = Properties.Strings.FileEditorNewFile;
            else
                fileName = Path.GetFileName(file);

            this.File = file;

            string tabText = fileName;
            if (Modified)
                tabText += "*";

            this.TabText = tabText;
            this.Text = tabText;
            this.ToolTipText = File;

            OnDisplayChanged((ContentInterface)this);
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
                string fileName;
                if (File == "")
                    fileName = Properties.Strings.FileEditorNewFile;
                else
                    fileName = Path.GetFileName(File);

                DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.FileCloseSave, fileName), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                    return true;
                else if (dialogResult == DialogResult.Yes)
                    Save();
            }

            return false;
        }

        private void AddBlocks(EditorINIBlock[] blocks)
        {
            List<TableData> tableData = new List<TableData>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[blocks[i].TemplateIndex];

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    int j = 0;
                    foreach (TableData tableBlock in objectListView1.Objects)
                    {
                        if (tableBlock.Block.TemplateIndex == blocks[i].TemplateIndex)
                        {
                            //block already exists, select it
                            if (i == blocks.Length - 1)
                            {
                                objectListView1.SelectObject(tableBlock);
                                objectListView1.EnsureVisible(j);
                                return;
                            }
                            break;
                        }
                        j++;
                    }
                }

                Data.Blocks.Add(blocks[i]);

                TableData newTableData = GetTableData(blocks[i]);
                newTableData.Modified = TableModified.Changed;
                tableData.Add(newTableData);
            }

            System.Collections.ArrayList objects = ((System.Collections.ArrayList)objectListView1.Objects);
            objects.AddRange(tableData);
            objects.Sort((System.Collections.IComparer)new TableDataComparer());

            objectListView1.SetObjects(objects);
            objectListView1.SelectedObjects = tableData;
            objectListView1.EnsureVisible(objectListView1.IndexOf(tableData[0]));

            Modified = true;
        }

        private void AddBlock(string blockName, int templateIndex)
        {
            Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[templateIndex];

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
            AddBlocks(new EditorINIBlock[] { editorBlock });
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
            bool sortRequired = false;

            for (int i = 0; i < blocks.Length; i++)
            {
                TableData tableData = (TableData)objectListView1.SelectedObjects[i];

                for (int j = 0; j < blocks[i].Count; j++)
                {
                    List<EditorINIEntry> options = tableData.Block.Options[j].Values;

                    if (blocks[i][j].Value is PropertySubOptions)
                    {
                        options.Clear();

                        //loop all sub values in the sub value collection
                        foreach (PropertyOption value in (PropertySubOptions)blocks[i][j].Value)
                        {
                            string text = ((string)value.Value).Trim();
                            if (text != string.Empty)
                            {
                                if (text.Contains(Environment.NewLine))
                                {
                                    string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    List<object> subOptions = new List<object>();
                                    for (int k = 1; k < lines.Length; k++)
                                        subOptions.Add(lines[k].Trim());

                                    options.Add(new EditorINIEntry(lines[0], subOptions));
                                }
                                else
                                    options.Add(new EditorINIEntry(text));
                            }
                        }
                    }
                    else
                    {
                        bool changedValue = true;

                        string text = ((string)blocks[i][j].Value).Trim();
                        if (text != string.Empty)
                        {
                            if (options.Count > 0)
                            {
                                //check if value is different
                                if (options[0].Value.ToString() != text)
                                    options[0].Value = text;
                                else
                                    changedValue = false;
                            }
                            else
                                options.Add(new EditorINIEntry(text));
                        }
                        else
                            options.Clear();

                        //change data in listview
                        if (tableData.Block.MainOptionIndex == j && changedValue)
                        {
                            tableData.Name = text;
                            sortRequired = true;
                        }
                    }
                }

                tableData.Modified = TableModified.Changed;
            }

            //refresh because of changed modified property (different background color)
            if (sortRequired)
            {
                System.Collections.ArrayList selectedObjects = (System.Collections.ArrayList)objectListView1.GetSelectedObjects();

                System.Collections.ArrayList objects = (System.Collections.ArrayList)objectListView1.Objects;
                objects.Sort((System.Collections.IComparer)new TableDataComparer());

                objectListView1.SetObjects(objects);
                objectListView1.SelectObjects(selectedObjects);
                objectListView1.EnsureVisible(objectListView1.IndexOf(selectedObjects[0]));
            }
            else
                //objectListView1.RefreshSelectedObjects doesnt work if it is just on entry left in the list
                objectListView1.RefreshObjects(objectListView1.SelectedObjects);

            Modified = true;

            OnSelectedDataChanged(GetSelectedBlocks(), Data.TemplateIndex);
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

        public void SelectAll()
        {
            objectListView1.SelectAll();
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

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SetContextMenuEnabled();
        }

        private void SetContextMenuEnabled()
        {
            bool selection = objectListView1.SelectedObjects.Count > 0;

            mnuDeleteSeperator.Visible = selection;
            mnuDelete.Visible = selection;
            mnuDelete.Enabled = selection;
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

        public class TableDataComparer : System.Collections.IComparer
        {
            int System.Collections.IComparer.Compare(object x, object y)
            {
                return ((TableData)x).CompareTo((TableData)y);
            }
        }

        private void mnuReDo_Click(object sender, EventArgs e)
        {

        }

        private void mnuUnDo_Click(object sender, EventArgs e)
        {

        }

        public bool CanSave()
        {
            return true;
        }

        public bool CanCopy()
        {
            return this.objectListView1.SelectedObjects.Count > 0;
        }

        public bool CanCut()
        {
            return CanCopy();
        }

        public bool CanPaste()
        {
            return Helper.Clipboard.CanPaste(typeof(EditorINIBlock[]));
        }

        public bool CanAdd()
        {
            return true;
        }

        public bool CanDelete()
        {
            return this.objectListView1.SelectedObjects.Count > 0;
        }

        public bool CanSelectAll()
        {
            return true;
        }

        public bool CanAddMultiple()
        {
            return true;
        }

        public void Save()
        {
            if (File == "")
                this.SaveAs();
            else
                this.Save(File);
        }

        public void SaveAs()
        {
            SaveFileDialog saverDialog = new SaveFileDialog();
            saverDialog.Filter = Properties.Strings.FileDialogFilter;
            if (saverDialog.ShowDialog() == DialogResult.OK)
                this.Save(saverDialog.FileName);
        }

        public void Add(int index)
        {
            this.AddBlock(this.mnuAdd.DropDownItems[index].Text, (int)this.mnuAdd.DropDownItems[index].Tag);
        }

        public void Delete()
        {
            DeleteSelectedBlocks();
        }

        public ToolStripDropDown MultipleAddDropDown()
        {
            return this.mnuAdd.DropDown;
        }

        public string GetTitle()
        {
            if (this.File == "")
                return Properties.Strings.FileEditorNewFile;
            else
                return Path.GetFileName(this.File);
        }

        public void Copy()
        {
            List<EditorINIBlock> blocks = new List<EditorINIBlock>();
            foreach (TableData tableData in this.objectListView1.SelectedObjects)
                blocks.Add(tableData.Block);

            Helper.Clipboard.Copy(blocks.ToArray());

            if (this.DockHandler.IsActivated)
                OnDisplayChanged((ContentInterface)this);
        }

        public void Cut()
        {
            Copy();
            DeleteSelectedBlocks();
        }

        public void Paste()
        {
            EditorINIBlock[] blocks = (EditorINIBlock[])Helper.Clipboard.Paste(typeof(EditorINIBlock[]));
            AddBlocks(blocks);
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