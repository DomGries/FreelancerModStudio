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
        public TableData Data;
        public string File { get; set; }
        public bool IsBINI { get; set; }

        private UndoRedo<System.Collections.IList> selectedObjects = new UndoRedo<System.Collections.IList>();
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

            UndoRedoManager.Start("");

            if (file != null)
            {
                FileManager fileManager = new FileManager(file);
                EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

                Data = new TableData(iniContent);
                IsBINI = fileManager.IsBini;

                SetFile(file);
            }
            else
            {
                Data = new TableData() { TemplateIndex = templateIndex };
                IsBINI = false;

                SetFile("");
            }

            RefreshSettings();
            ShowData();

            UndoRedoManager.ClearHistory();
            UndoRedoManager.CommandDone += delegate
            {
                this.ReloadDisplay();
            };
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Properties.Strings.FileEditorEmpty;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(BrightIdeasSoftware.OLVListItem lvi)
            {
                TableBlock tableData = (TableBlock)lvi.RowObject;
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

            //sort by type and name
            Data.Blocks.Sort();
            ReloadDisplay();

            //add block types to add menu
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
            fileManager.Write(Data.GetEditorData());

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
                        foreach (TableBlock tableData in objectListView1.Objects)
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

        private void AddBlocks(TableBlock[] blocks)
        {
            UndoRedoManager.Start("");

            List<TableBlock> selectedData = new List<TableBlock>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[blocks[i].Block.TemplateIndex];
                TableBlock tableBlock = blocks[i];
                tableBlock.Modified = TableModified.Changed;

                bool existSingle = false;

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < Data.Blocks.Count; j++)
                    {
                        if (Data.Blocks[j].Block.TemplateIndex == blocks[i].Block.TemplateIndex)
                        {
                            //block already exists, select it
                            Data.Blocks[j] = tableBlock;

                            existSingle = true;
                            break;
                        }
                    }
                }

                if (!existSingle)
                    Data.Blocks.Add(blocks[i]);

                selectedData.Add(tableBlock);
            }

            Data.Blocks.Sort();
            UndoRedoManager.Commit();

            objectListView1.SelectedObjects = selectedData;
            objectListView1.EnsureVisible(objectListView1.IndexOf(selectedData[0]));

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
            AddBlocks(new TableBlock[] { new TableBlock(editorBlock, Data.TemplateIndex) });
        }

        public EditorINIBlock[] GetSelectedBlocks()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<EditorINIBlock> blocks = new List<EditorINIBlock>();
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
                blocks.Add(tableData.Block);

            return blocks.ToArray();
        }

        public void SetBlocks(PropertyBlock[] blocks)
        {
            UndoRedoManager.Start("");
            bool sortRequired = false;

            for (int i = 0; i < blocks.Length; i++)
            {
                TableBlock tableData = (TableBlock)objectListView1.SelectedObjects[i];

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
                Data.Blocks.Sort();
                UndoRedoManager.Commit();
            }
            else
                UndoRedoManager.Commit();

            Modified = true;

            OnSelectedDataChanged(GetSelectedBlocks(), Data.TemplateIndex);
        }

        private void ReloadDisplay()
        {
            System.Collections.ArrayList selection = (System.Collections.ArrayList)objectListView1.SelectedObjects;

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectObjects(selection);

            if (objectListView1.SelectedObjects.Count > 0)
                objectListView1.EnsureVisible(objectListView1.IndexOf(objectListView1.SelectedObjects[0]));
        }

        private void DeleteSelectedBlocks()
        {
            UndoRedoManager.Start("");

            foreach (TableBlock tableBlock in objectListView1.SelectedObjects)
                Data.Blocks.Remove(tableBlock);

            UndoRedoManager.Commit();
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
                return ((TableBlock)x).CompareTo((TableBlock)y);
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
            return Helper.Clipboard.CanPaste(typeof(EditorINIData));
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
            EditorINIData data = new EditorINIData(Data.TemplateIndex);
            foreach (TableBlock tableData in this.objectListView1.SelectedObjects)
                data.Blocks.Add(tableData.Block);

            Helper.Clipboard.Copy(data);

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
            EditorINIData editorData = (EditorINIData)Helper.Clipboard.Paste(typeof(EditorINIData));

            if (editorData.TemplateIndex == Data.TemplateIndex)
            {
                TableBlock[] blocks = new TableBlock[editorData.Blocks.Count];
                for (int i = 0; i < editorData.Blocks.Count; i++ )
                    blocks[i] = new TableBlock(editorData.Blocks[i], Data.TemplateIndex);

                AddBlocks(blocks);
            }
        }
    }

    public class TableData
    {
        public UndoRedoList<TableBlock> Blocks { get; set; }
        public int TemplateIndex { get; set; }

        public TableData()
        {
            Blocks = new UndoRedoList<TableBlock>();
        }

        public TableData(EditorINIData data)
        {
            Blocks = new UndoRedoList<TableBlock>();
            TemplateIndex = data.TemplateIndex;

            foreach (EditorINIBlock block in data.Blocks)
                this.Blocks.Add(new TableBlock(block, TemplateIndex));
        }

        public EditorINIData GetEditorData()
        {
            EditorINIData data = new EditorINIData(TemplateIndex);

            foreach (TableBlock block in Blocks)
                data.Blocks.Add(block.Block);

            return data;
        }
    }

    public class TableBlock : IComparable<TableBlock>
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public EditorINIBlock Block;
        public TableModified Modified = TableModified.Normal;

        public TableBlock(EditorINIBlock block, int templateIndex)
        {
            //name of block
            if (block.MainOptionIndex > -1 && block.Options.Count >= block.MainOptionIndex + 1)
            {
                if (block.Options[block.MainOptionIndex].Values.Count > 0)
                    Name = block.Options[block.MainOptionIndex].Values[0].Value.ToString();
                else
                    Name = block.Name;
            }
            else
            {
                //if (Helper.Template.Data.Files[Data.TemplateIndex].Blocks[block.TemplateIndex].Multiple)
                //    blockName = blockName + i.ToString();
                //else
                Name = block.Name;
                //todo: different block name if they are all the same
            }

            //name of group
            if (Helper.Template.Data.Files[templateIndex].Blocks[block.TemplateIndex].Multiple)
                Group = block.Name;
            else
                Group = Properties.Strings.FileDefaultCategory;

            Block = block;
        }

        public int CompareTo(TableBlock other)
        {
            //sort by group, name, modified
            int groupComparison = this.Group.CompareTo(other.Group);
            if (groupComparison == 0)
            {
                int nameComparison = this.Name.CompareTo(other.Name);
                if (nameComparison == 0)
                    return this.Modified.CompareTo(other.Modified);

                return nameComparison;
            }

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