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

        public bool IsUniverse { get; set; }
        public ArchtypeManager Archtype { get; set; }

        public delegate void DataChangedType(ChangedData data);
        public DataChangedType DataChanged;

        public delegate void SelectionChangedType(List<TableBlock> data, int templateIndex);
        public SelectionChangedType SelectionChanged;

        public delegate void DataVisibilityChangedType(TableBlock block, bool visibility);
        public DataVisibilityChangedType DataVisibilityChanged;

        public delegate void ContentChangedType(ContentInterface content);
        public ContentChangedType ContentChanged;

        public delegate void DocumentChangedType(DocumentInterface document);
        public DocumentChangedType DocumentChanged;

        void OnDataChanged(ChangedData data)
        {
            if (this.DataChanged != null)
                this.DataChanged(data);
        }

        void OnSelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(data, templateIndex);
        }

        void OnDataVisibilityChanged(TableBlock block, bool visibility)
        {
            if (this.DataVisibilityChanged != null)
                this.DataVisibilityChanged(block, visibility);
        }

        void OnContentChanged(ContentInterface content)
        {
            if (this.ContentChanged != null)
                this.ContentChanged(content);
        }

        void OnDocumentChanged(DocumentInterface document)
        {
            if (this.DocumentChanged != null)
                this.DocumentChanged(document);
        }

        public frmTableEditor(int templateIndex, string file)
        {
            InitializeComponent();

            LoadIcons();
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

            RefreshSettings();
        }

        void LoadIcons()
        {
            Icon = Properties.Resources.FileINIIcon;

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

        string ShowSolarArchtypeSelector()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Title = string.Format(Properties.Strings.FileEditorOpenSolarArch, PathGetFileName(File));
            openFile.Filter = "Solar Archtype INI|*.ini";
            if (openFile.ShowDialog() == DialogResult.OK)
                return openFile.FileName;

            return null;
        }

        string PathGetFileName(string path)
        {
            if (path.Trim() == string.Empty)
                return path;

            return System.IO.Path.GetFileName(path);
        }

        public void LoadArchtypes()
        {
            int archtypeTemplate = Helper.Template.Data.Files.IndexOf("Solar Archtype");
            string archtypeFile = ArchtypeManager.GetRelativeArchtype(File, Data.TemplateIndex, archtypeTemplate);

            //user interaction required to get the path of the archtype file
            if (archtypeFile == null)
                archtypeFile = ShowSolarArchtypeSelector();

            if (Archtype == null)
                Archtype = new ArchtypeManager(archtypeFile, archtypeTemplate);

            foreach (TableBlock block in Data.Blocks)
                SetArchtype(block, Archtype);
        }

        void SetArchtype(TableBlock block, ArchtypeManager archtypeManager)
        {
            switch (block.Block.Name.ToLower())
            {
                case "lightsource":
                    block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.LightSource;
                    break;
                case "zone":
                    block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.Zone;
                    break;
                case "system":
                    block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.System;
                    break;
                case "object":
                    if (archtypeManager != null)
                    {
                        bool hasArchtype = false;

                        //get type of object based on archtype
                        foreach (EditorINIOption option in block.Block.Options)
                        {
                            if (option.Name.ToLower() == "archetype")
                            {
                                if (option.Values.Count > 0)
                                {
                                    block.Archtype = archtypeManager.TypeOf(option.Values[0].Value.ToString());
                                    if (block.Archtype != null)
                                    {
                                        block.ObjectType = block.Archtype.Type;
                                        hasArchtype = true;
                                    }

                                    break;
                                }
                            }
                        }

                        if (!hasArchtype)
                            block.ObjectType = FreelancerModStudio.SystemPresenter.ContentType.None;
                    }
                    break;
            }

            if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                block.Visibility = true;
        }

        public void ShowData()
        {
            bool isSystem = Data.TemplateIndex == Helper.Template.Data.Files.IndexOf("System");
            IsUniverse = Data.TemplateIndex == Helper.Template.Data.Files.IndexOf("Universe");
            if (isSystem || IsUniverse)
                LoadArchtypes();

#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            AddColumns(isSystem);

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

        void AddColumns(bool isSystem)
        {
            //clear all items and columns
            objectListView1.Clear();

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

        void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(GetSelectedBlocks(), Data.TemplateIndex);
            OnContentChanged((ContentInterface)this);
        }

        void Save(string file)
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

        void SetFile(string file)
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

        bool CancelClose()
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

        void AddBlocks(List<TableBlock> blocks, int undoBlock, bool undo)
        {
            List<TableBlock> selectedData = new List<TableBlock>();
            for (int i = 0; i < blocks.Count; i++)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks[blocks[i].Block.TemplateIndex];
                TableBlock tableBlock = blocks[i];

                //set block to be modified except in is undo mode
                if (!undo)
                    tableBlock.Modified = TableModified.Changed;

                //set archtype of block
                if (tableBlock.Archtype == null)
                    SetArchtype(tableBlock, Archtype);

                bool existSingle = false;

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < Data.Blocks.Count; j++)
                    {
                        //block already exists
                        if (Data.Blocks[j].Block.TemplateIndex == blocks[i].Block.TemplateIndex)
                        {
                            //overwrite data if we add blocks which are single then they are overwritten which means they have to be changed to edit in the undo history
                            undoManager.CurrentData[undoBlock] = new ChangedData()
                            {
                                Type = ChangedType.Edit,
                                OldBlocks = new List<TableBlock>() { Data.Blocks[j] },
                                NewBlocks = new List<TableBlock>() { tableBlock },
                            };

                            //overwrite block
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

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectedObjects = selectedData;
            EnsureSelectionVisible();

            Modified = true;
        }

        void AddBlock(string blockName, int templateIndex)
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

            Data.MaxID++;

            //add actual block
            undoManager.Execute(new ChangedData() { NewBlocks = new List<TableBlock> { new TableBlock(Data.MaxID, editorBlock, Data.TemplateIndex) }, Type = ChangedType.Add });
        }

        public List<TableBlock> GetSelectedBlocks()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return null;

            List<TableBlock> blocks = new List<TableBlock>();
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
                blocks.Add(tableData);

            return blocks;
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

            foreach (TableBlock block in newBlocks)
                SetArchtype(block, Archtype);

            undoManager.Execute(new ChangedData() { NewBlocks = newBlocks, OldBlocks = oldBlocks, Type = ChangedType.Edit });
        }

        void ChangeBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            for (int i = 0; i < oldBlocks.Count; i++)
            {
                int index = Data.Blocks.IndexOf(oldBlocks[i]);
                Data.Blocks[index] = newBlocks[i];
            }

            Data.Blocks.Sort();

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.RefreshObjects(Data.Blocks);
            objectListView1.SelectObjects(newBlocks);
            EnsureSelectionVisible();

            Modified = true;
        }

        void DeleteBlocks(List<TableBlock> blocks)
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

        void DeleteSelectedBlocks()
        {
            List<TableBlock> blocks = new List<TableBlock>();

            foreach (TableBlock block in objectListView1.SelectedObjects)
                blocks.Add(block);

            undoManager.Execute(new ChangedData() { NewBlocks = blocks, Type = ChangedType.Delete });
        }

        void EnsureSelectionVisible()
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                objectListView1.EnsureVisible(objectListView1.IndexOf(objectListView1.SelectedObjects[objectListView1.SelectedObjects.Count - 1]));
                objectListView1.EnsureVisible(objectListView1.IndexOf(objectListView1.SelectedObjects[0]));
            }
        }

        void frmDefaultEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = CancelClose();
        }

        public void SelectAll()
        {
            objectListView1.SelectAll();
        }

        void mnuClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void mnuDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedBlocks();
        }

        void mnuSelectAll_Click(object sender, EventArgs e)
        {
            objectListView1.SelectAll();
        }

        void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SetContextMenuEnabled();
        }

        void SetContextMenuEnabled()
        {
            bool selection = objectListView1.SelectedObjects.Count > 0;

            mnuDeleteSeperator.Visible = selection;
            mnuDelete.Visible = selection;
            mnuDelete.Enabled = selection;
        }

        void mnuAddItem_Click(object sender, EventArgs e)
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
                    Data.MaxID++;
                    blocks.Add(new TableBlock(Data.MaxID, editorData.Blocks[i], Data.TemplateIndex));
                }

                undoManager.Execute(new ChangedData() { NewBlocks = blocks, Type = ChangedType.Add });
            }
        }

        public bool UseDocument()
        {
            return false;
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

        void ExecuteDataChanged(ChangedData data, int undoBlock, bool undo)
        {
            if (data.Type == ChangedType.Add)
                AddBlocks(data.NewBlocks, undoBlock, undo);
            else if (data.Type == ChangedType.Delete)
                DeleteBlocks(data.NewBlocks);
            else if (data.Type == ChangedType.Edit)
                ChangeBlocks(data.NewBlocks, data.OldBlocks);

            OnDataChanged(data);
        }

        void UndoManager_DataChanged(List<ChangedData> data, bool undo)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (undo)
                    ExecuteDataChanged(data[i].GetUndoData(), i, undo);
                else
                    ExecuteDataChanged(data[i], i, undo);
            }

            OnDocumentChanged((DocumentInterface)this);
        }

        public void Select(TableBlock block)
        {
            objectListView1.SelectedObjects = new List<TableBlock> { block };
            EnsureSelectionVisible();
        }

        //overwrite to add extra information to layout.xml
        protected override string GetPersistString()
        {
            return GetType().ToString() + "," + File + "," + Data.TemplateIndex;
        }

        public void HideShowSelected()
        {
            if (objectListView1.SelectedObjects.Count > 0)
            {
                bool visibility = !((TableBlock)objectListView1.SelectedObjects[0]).Visibility;

                foreach (TableBlock block in objectListView1.SelectedObjects)
                {
                    if (block.ObjectType != FreelancerModStudio.SystemPresenter.ContentType.None)
                    {
                        block.Visibility = visibility;
                        OnDataVisibilityChanged(block, visibility);
                    }
                }

                objectListView1.RefreshObjects(objectListView1.SelectedObjects);
            }
        }

        public bool CanChangeVisibility()
        {
            return Archtype != null;
        }

        public void ChangeVisibility()
        {
            HideShowSelected();
        }
    }
}