using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using BrightIdeasSoftware;

namespace FreelancerModStudio
{
    public partial class frmTableEditor : WeifenLuo.WinFormsUI.Docking.DockContent, DocumentInterface, ContentInterface
    {
        public TableData Data;
        public string File { get; set; }
        public bool IsBINI { get; set; }

        bool modified = false;
        UndoManager<ChangedData> undoManager = new UndoManager<ChangedData>();

        public delegate void DataChangedType(ChangedData data);
        public DataChangedType DataChanged;

        public delegate void SelectionChangedType(TableBlock[] data, int templateIndex);
        public SelectionChangedType SelectionChanged;

        //public delegate void SelectedDataChangedType(TableBlock[] data, int templateIndex);
        //public SelectedDataChangedType SelectedDataChanged;

        public delegate void DataVisibilityChangedType(TableBlock block, bool visibility);
        public DataVisibilityChangedType DataVisibilityChanged;

        public delegate void ContentChangedType(ContentInterface content);
        public ContentChangedType ContentChanged;

        public delegate void DocumentChangedType(DocumentInterface document);
        public DocumentChangedType DocumentChanged;

        private void OnDataChanged(ChangedData data)
        {
            if (this.DataChanged != null)
                this.DataChanged(data);
        }

        private void OnSelectionChanged(TableBlock[] data, int templateIndex)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(data, templateIndex);
        }

        //private void OnSelectedDataChanged(TableBlock[] data, int templateIndex)
        //{
        //    if (this.SelectedDataChanged != null)
        //        this.SelectedDataChanged(data, templateIndex);
        //}

        private void OnDataVisibilityChanged(TableBlock block, bool visibility)
        {
            if (this.DataVisibilityChanged != null)
                this.DataVisibilityChanged(block, visibility);
        }

        private void OnContentChanged(ContentInterface content)
        {
            if (this.ContentChanged != null)
                this.ContentChanged(content);
        }

        private void OnDocumentChanged(DocumentInterface document)
        {
            if (this.DocumentChanged != null)
                this.DocumentChanged(document);
        }

        public frmTableEditor(int templateIndex, string file)
        {
            InitializeComponent();

            Icon = Properties.Resources.FileINIIcon;
            undoManager.DataChanged += UndoManager_DataChanged;

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

            LoadIcons();
            RefreshSettings();
        }

        private void LoadIcons()
        {
            ImageList imageList = new ImageList() { ColorDepth = ColorDepth.Depth32Bit };
            imageList.Images.AddRange(new Image[]
            {
                Properties.Resources.LightSource,
                Properties.Resources.Sun,
                Properties.Resources.Planet,
                Properties.Resources.Station,
                Properties.Resources.Satellite,
                Properties.Resources.Construct,
                Properties.Resources.Depot,
                Properties.Resources.Ship,
                Properties.Resources.WeaponsPlatform,
                Properties.Resources.DockingRing,
                Properties.Resources.JumpHole,
                Properties.Resources.JumpGate,
                Properties.Resources.TradeLane,
                Properties.Resources.Zone,
            });
            objectListView1.SmallImageList = imageList;
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Properties.Strings.FileEditorEmpty;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(OLVListItem lvi)
            {
                TableBlock block = (TableBlock)lvi.RowObject;
                if (block.Modified == TableModified.Changed)
                    lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedColor;
                else if (block.Modified == TableModified.ChangedSaved)
                    lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedSavedColor;

                if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None && !block.Visibility)
                    lvi.ForeColor = Helper.Settings.Data.Data.General.EditorHiddenColor;
            };

            //refresh column text
            if (objectListView1.Columns.Count > 0)
            {
                objectListView1.Columns[0].Text = Properties.Strings.FileEditorColumnName;
                objectListView1.Columns[1].Text = Properties.Strings.FileEditorColumnType;
            }

            //update 'New file' to new language
            if (File == "")
                SetFile("");

            objectListView1.Refresh();
        }

        public void LoadArchtypes(string file, int templateIndex)
        {
            if (Helper.Archtype.ArchtypeManager == null)
                Helper.Archtype.LoadArchtypes(file, templateIndex);

            foreach (TableBlock block in Data.Blocks)
                SetArchtype(block, Helper.Archtype.ArchtypeManager);
        }

        private void SetArchtype(TableBlock block, ArchtypeManager archtypeManager)
        {
            BlockType blockType = (BlockType)block.Block.TemplateIndex;
            if (blockType == BlockType.LightSource)
                block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.LightSource;
            else if (blockType == BlockType.Zone)
                block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.Zone;
            else
            {
                //get type of object based on archtype
                foreach (EditorINIOption option in block.Block.Options)
                {
                    if ((ObjectOptionType)option.TemplateIndex == ObjectOptionType.Archtype)
                    {
                        if (option.Values.Count > 0)
                        {
                            block.Archtype = archtypeManager.TypeOf(option.Values[0].Value.ToString());
                            if (block.Archtype != null)
                                block.ObjectType = block.Archtype.Type;

                            break;
                        }
                    }
                }
            }

            if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                block.Visibility = true;
        }

        public void ShowData()
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            AddColumns();

            //sort by type and name
            Data.Blocks.Sort();
            objectListView1.SetObjects(Data.Blocks);

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

        private void AddColumns()
        {
            //clear all items and columns
            objectListView1.Clear();

            bool isSystem = Data.TemplateIndex == Helper.Template.Data.Files.IndexOf("System");
            objectListView1.CheckBoxes = isSystem;

            OLVColumn[] cols =
            {
                    new OLVColumn(Properties.Strings.FileEditorColumnName, "Name"),
                    new OLVColumn(Properties.Strings.FileEditorColumnType, "Group")
            };

            cols[0].Width = 150;
            cols[1].MinimumWidth = 120;
            cols[1].FillsFreeSpace = true;

            if (isSystem)
            {
                //legend icons in system editor
                cols[0].AspectGetter = delegate(object x)
                {
                    return ((TableBlock)x).Name;
                };
                cols[0].ImageGetter = delegate(object rowObject)
                {
                    TableBlock block = (TableBlock)rowObject;
                    if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                        return (int)block.ObjectType;

                    return -1;
                };

                //checkboxes for hidden shown objects
                objectListView1.BooleanCheckStateGetter = delegate(object x)
                {
                    return ((TableBlock)x).Visibility;
                };
                objectListView1.BooleanCheckStatePutter = delegate(object x, bool newValue)
                {
                    TableBlock block = (TableBlock)x;
                    if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                    {
                        block.Visibility = newValue;

                        OnDataVisibilityChanged(block, newValue);
                        return newValue;
                    }

                    return block.Visibility;
                };

                //show content type if possible otherwise group
                cols[1].AspectGetter = delegate(object x)
                {
                    TableBlock block = (TableBlock)x;
                    if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                        return block.ObjectType.ToString();

                    return block.Group;
                };
            }
            else
            {
                objectListView1.BooleanCheckStateGetter = null;
                objectListView1.BooleanCheckStatePutter = null;
            }

            objectListView1.Columns.AddRange(cols);
        }

        private void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(GetSelectedBlocks(), Data.TemplateIndex);

            if (this.DockHandler.IsActivated)
                OnContentChanged((ContentInterface)this);
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

            OnDocumentChanged((DocumentInterface)this);
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

        private void AddBlocks(List<TableBlock> blocks)
        {
            List<ChangedData> overwrittenData = new List<ChangedData>();

            List<TableBlock> selectedData = new List<TableBlock>();
            for (int i = 0; i < blocks.Count; i++)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[blocks[i].Block.TemplateIndex];
                TableBlock tableBlock = blocks[i];
                tableBlock.Modified = TableModified.Changed;

                //set archtype of block
                if (tableBlock.Archtype == null)
                    SetArchtype(tableBlock, Helper.Archtype.ArchtypeManager);

                bool existSingle = false;

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < Data.Blocks.Count; j++)
                    {
                        if (Data.Blocks[j].Block.TemplateIndex == blocks[i].Block.TemplateIndex)
                        {
                            //block already exists
                            //undoManager.CurrentData[0].NewBlocks.RemoveAt(i - overwrittenData.Count);

                            overwrittenData.Add(new ChangedData()
                            {
                                Type = ChangedType.Edit,
                                OldBlocks = new List<TableBlock>() { Data.Blocks[j] },
                                NewBlocks = new List<TableBlock>() { tableBlock },
                            });

                            //overwrite block
                            Data.Blocks[j] = tableBlock;
                            existSingle = true;

                            break;
                        }
                    }
                }

                if (!existSingle)
                {
                    Data.Blocks.Add(blocks[i]);

                    overwrittenData.Add(new ChangedData()
                    {
                        Type = ChangedType.Add,
                        NewBlocks = new List<TableBlock>() { tableBlock }
                    });
                }

                selectedData.Add(tableBlock);
            }

            //overwrite data if we add blocks which are single then they are overwritten which means they have to be changed to edit in the undo history
            //undoManager.CurrentData.AddRange(overwrittenData);
            undoManager.CurrentData = overwrittenData;

            Data.Blocks.Sort();

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectedObjects = selectedData;
            EnsureSelectionVisible();

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

            int id = 0;
            if (Data.Blocks.Count > 0)
                id = Data.Blocks[Data.Blocks.Count - 1].ID + 1;

            //add actual block
            undoManager.Execute(new ChangedData() { NewBlocks = new List<TableBlock> { new TableBlock(id, editorBlock, Data.TemplateIndex) }, Type = ChangedType.Add });
        }

        public TableBlock[] GetSelectedBlocks()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<TableBlock> blocks = new List<TableBlock>();
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
                blocks.Add(tableData);

            return blocks.ToArray();
        }

        public void SetBlocks(PropertyBlock[] blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();
            List<TableBlock> oldBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Length; i++)
            {
                oldBlocks.Add((TableBlock)objectListView1.SelectedObjects[i]);
                newBlocks.Add(ObjectClone.Clone<TableBlock>(oldBlocks[i]));

                for (int j = 0; j < blocks[i].Count; j++)
                {
                    List<EditorINIEntry> options = newBlocks[i].Block.Options[j].Values;

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
                        string text = ((string)blocks[i][j].Value).Trim();
                        if (text != string.Empty)
                        {
                            if (options.Count > 0)
                            {
                                //check if value is different
                                if (options[0].Value.ToString() != text)
                                    options[0].Value = text;
                            }
                            else
                                options.Add(new EditorINIEntry(text));
                        }
                        else
                            options.Clear();

                        //change data in listview
                        if (newBlocks[i].Block.MainOptionIndex == j)
                            newBlocks[i].Name = text;
                    }
                }

                newBlocks[i].Modified = TableModified.Changed;
            }

            undoManager.Execute(new ChangedData() { NewBlocks = newBlocks, OldBlocks = oldBlocks, Type = ChangedType.Edit });
            //OnDataChanged(GetSelectedBlocks(), Data.TemplateIndex);
        }

        private void ChangeBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            for (int i = 0; i < oldBlocks.Count; i++)
            {
                int index = Data.Blocks.IndexOf(oldBlocks[i]);
                Data.Blocks[index] = newBlocks[i];
            }

            Data.Blocks.Sort();

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectObjects(newBlocks);
            EnsureSelectionVisible();

            Modified = true;
        }

        private void DeleteBlocks(List<TableBlock> blocks)
        {
            System.Collections.IList selection = objectListView1.SelectedObjects;

            foreach (TableBlock tableBlock in blocks)
                Data.Blocks.Remove(tableBlock);

            objectListView1.RemoveObjects(blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(selection);
            EnsureSelectionVisible();

            Modified = true;
        }

        private void DeleteSelectedBlocks()
        {
            List<TableBlock> blocks = new List<TableBlock>();

            foreach (TableBlock block in objectListView1.SelectedObjects)
                blocks.Add(block);

            undoManager.Execute(new ChangedData() { NewBlocks = blocks, Type = ChangedType.Delete });
        }

        private void EnsureSelectionVisible()
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                objectListView1.EnsureVisible(objectListView1.IndexOf(objectListView1.SelectedObjects[objectListView1.SelectedObjects.Count - 1]));
                objectListView1.EnsureVisible(objectListView1.IndexOf(objectListView1.SelectedObjects[0]));
            }
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

        private void mnuAddItem_Click(object sender, EventArgs e)
        {
            string blockName = ((ToolStripMenuItem)sender).Text;
            int templateIndex = (int)((ToolStripMenuItem)sender).Tag;

            AddBlock(blockName, templateIndex);
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
            return Clipboard.CanPaste(typeof(EditorINIData));
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

            Clipboard.Copy(data);

            if (this.DockHandler.IsActivated)
                OnContentChanged((ContentInterface)this);
        }

        public void Cut()
        {
            Copy();
            DeleteSelectedBlocks();
        }

        public void Paste()
        {
            EditorINIData editorData = (EditorINIData)Clipboard.Paste(typeof(EditorINIData));

            if (editorData.TemplateIndex == Data.TemplateIndex)
            {
                List<TableBlock> blocks = new List<TableBlock>();
                for (int i = 0; i < editorData.Blocks.Count; i++)
                {
                    int id = 0;
                    if (Data.Blocks.Count > 0)
                        id = Data.Blocks[Data.Blocks.Count - 1].ID + 1;

                    blocks.Add(new TableBlock(id, editorData.Blocks[i], Data.TemplateIndex));
                }

                undoManager.Execute(new ChangedData() { NewBlocks = blocks, Type = ChangedType.Add });
            }
        }

        public bool CanUndo()
        {
            return undoManager.CanUndo();
        }

        public bool CanRedo()
        {
            return undoManager.CanRedo();
        }

        public void Undo()
        {
            undoManager.Undo(1);
        }

        public void Redo()
        {
            undoManager.Redo(1);
        }

        private void ExecuteDataChanged(ChangedData data)
        {
            if (data.Type == ChangedType.Add)
                AddBlocks(data.NewBlocks);
            else if (data.Type == ChangedType.Delete)
                DeleteBlocks(data.NewBlocks);
            else if (data.Type == ChangedType.Edit)
                ChangeBlocks(data.NewBlocks, data.OldBlocks);

            OnDocumentChanged((DocumentInterface)this);
            OnDataChanged(data);
        }

        private void UndoManager_DataChanged(List<ChangedData> data, bool undo)
        {
            foreach (ChangedData change in data)
            {
                if (undo)
                    ExecuteDataChanged(change.GetUndoData());
                else
                    ExecuteDataChanged(change);
            }
        }

        public void Select(TableBlock block)
        {
            objectListView1.SelectedObject = block;
            EnsureSelectionVisible();
        }

        //overwrite to add extra information to layout.xml
        protected override string GetPersistString()
        {
            return GetType().ToString() + "," + File + "," + Data.TemplateIndex;
        }

        private void HideShowSelected()
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                bool visibility = !((TableBlock)objectListView1.SelectedObjects[0]).Visibility;

                foreach (TableBlock block in objectListView1.SelectedObjects)
                    OnDataVisibilityChanged(block, visibility);

                objectListView1.RefreshObjects(objectListView1.SelectedObjects);
            }
        }
    }
}