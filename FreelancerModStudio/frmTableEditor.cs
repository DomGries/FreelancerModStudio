using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FreelancerModStudio.Controls;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;

namespace FreelancerModStudio
{
    public partial class frmTableEditor : WeifenLuo.WinFormsUI.Docking.DockContent, IDocumentForm, IContentForm
    {
        public TableData Data;
        public string File { get; set; }
        public bool IsBINI { get; set; }

        UndoManager<ChangedData> undoManager = new UndoManager<ChangedData>();

        public ViewerType ViewerType { get; set; }
        public ArchetypeManager Archetype { get; set; }

        public delegate void DataChangedType(ChangedData data);
        public DataChangedType DataChanged;

        public delegate void SelectionChangedType(List<TableBlock> data, int templateIndex);
        public SelectionChangedType SelectionChanged;

        public delegate void DataVisibilityChangedType(TableBlock block);
        public DataVisibilityChangedType DataVisibilityChanged;

        public delegate void ContentChangedType(IContentForm content);
        public ContentChangedType ContentChanged;

        public delegate void DocumentChangedType(IDocumentForm document);
        public DocumentChangedType DocumentChanged;

        void OnDataChanged(ChangedData data)
        {
            if (DataChanged != null)
                DataChanged(data);
        }

        void OnSelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (SelectionChanged != null)
                SelectionChanged(data, templateIndex);
        }

        void OnDataVisibilityChanged(TableBlock block)
        {
            if (DataVisibilityChanged != null)
                DataVisibilityChanged(block);
        }

        void OnContentChanged(IContentForm content)
        {
            if (ContentChanged != null)
                ContentChanged(content);
        }

        void OnDocumentChanged(IDocumentForm document)
        {
            if (DocumentChanged != null)
                DocumentChanged(document);
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
                IsBINI = fileManager.IsBINI;

                SetFile(file);
            }
            else
            {
                Data = new TableData { TemplateIndex = templateIndex };
                IsBINI = false;

                SetFile(string.Empty);
            }

            objectListView1.CellToolTip.InitialDelay = 1000;
            objectListView1.UnfocusedHighlightBackgroundColor = objectListView1.HighlightBackgroundColorOrDefault;
            objectListView1.UnfocusedHighlightForegroundColor = objectListView1.HighlightForegroundColorOrDefault;

            var dropSink = objectListView1.DropSink as SimpleDropSink;
            if (dropSink != null)
            {
                dropSink.CanDropBetween = true;
                dropSink.CanDropOnItem = false;
            }

            RefreshSettings();
        }

        void LoadIcons()
        {
            Icon = Properties.Resources.FileINIIcon;

            ImageList imageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
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
                Properties.Resources.System,
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

                if (block.ObjectType != SystemPresenter.ContentType.None && !block.Visibility)
                    lvi.ForeColor = Helper.Settings.Data.Data.General.EditorHiddenColor;
            };

            //refresh column text
            if (objectListView1.Columns.Count > 2)
            {
                objectListView1.Columns[0].Text = Properties.Strings.FileEditorColumnName;
                objectListView1.Columns[2].Text = Properties.Strings.FileEditorColumnType;
            }

            //update 'New file' to new language
            if (File == string.Empty)
                SetFile(string.Empty);

            objectListView1.Refresh();
        }

        string ShowSolarArchetypeSelector()
        {
            var openFile = new OpenFileDialog
                                          {
                                              Title = string.Format(Properties.Strings.FileEditorOpenSolarArch, PathGetFileName(File)),
                                              Filter = "Solar Archetype INI|*.ini"
                                          };
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

        public void LoadArchetypes()
        {
            int archetypeTemplate = Helper.Template.Data.SolarArchetypeFile;
            string archetypeFile = ArchetypeManager.GetRelativeArchetype(File, Data.TemplateIndex, archetypeTemplate);

            //user interaction required to get the path of the archetype file
            if (archetypeFile == null)
                archetypeFile = ShowSolarArchetypeSelector();

            if (Archetype == null)
                Archetype = new ArchetypeManager(archetypeFile, archetypeTemplate);

            foreach (TableBlock block in Data.Blocks)
                SetObjectType(block, Archetype);
        }

        void SetObjectType(TableBlock block, ArchetypeManager archetypeManager)
        {
            switch (block.Block.Name.ToLower())
            {
                case "lightsource":
                    block.ObjectType = SystemPresenter.ContentType.LightSource;
                    break;
                case "zone":
                    block.ObjectType = SystemPresenter.ContentType.Zone;
                    break;
                case "system":
                    block.ObjectType = SystemPresenter.ContentType.System;
                    break;
                case "object":
                    if (archetypeManager != null)
                    {
                        bool hasArchetype = false;

                        //get type of object based on archetype
                        foreach (EditorINIOption option in block.Block.Options)
                        {
                            if (option.Name.ToLower() == "archetype")
                            {
                                if (option.Values.Count > 0)
                                {
                                    block.Archetype = archetypeManager.TypeOf(option.Values[0].Value.ToString());
                                    if (block.Archetype != null)
                                    {
                                        block.ObjectType = block.Archetype.Type;
                                        hasArchetype = true;
                                    }

                                    break;
                                }
                            }
                        }

                        if (!hasArchetype)
                            block.ObjectType = SystemPresenter.ContentType.None;
                    }
                    break;
            }

            if (block.ObjectType != SystemPresenter.ContentType.None)
                block.Visibility = true;
        }

        public void ShowData()
        {
            if (Data.TemplateIndex == Helper.Template.Data.SystemFile)
                ViewerType = ViewerType.System;
            else if (Data.TemplateIndex == Helper.Template.Data.UniverseFile)
                ViewerType = ViewerType.Universe;
            else
                ViewerType = ViewerType.None;

            if (ViewerType != ViewerType.None)
                LoadArchetypes();

#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            AddColumns();

            objectListView1.SetObjects(Data.Blocks);

            //add block types to add menu
            for (int i = 0; i < Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Count; i++)
            {
                var addItem = new ToolStripMenuItem
                                                {
                                                    Text = Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Values[i].Name,
                                                    Tag = i
                                                };
                addItem.Click += mnuAddItem_Click;
                mnuAdd.DropDownItems.Add(addItem);
            }
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        void AddColumns()
        {
            //clear all items and columns
            objectListView1.Clear();

            objectListView1.CheckBoxes = ViewerType == ViewerType.System;

            OLVColumn[] cols =
            {
                    new OLVColumn(Properties.Strings.FileEditorColumnName, "Name"),
                    new OLVColumn("#", "ID"),
                    new OLVColumn(Properties.Strings.FileEditorColumnType, "Group")
            };

            cols[0].Width = 150;
            cols[1].Width = cols[1].MinimumWidth = cols[1].MaximumWidth = 34;
            cols[2].MinimumWidth = 120;
            cols[2].FillsFreeSpace = true;

            if (ViewerType != ViewerType.None)
            {
                //legend icons in system editor
                cols[0].ImageGetter = delegate(object rowObject)
                {
                    return (int)((TableBlock)rowObject).ObjectType - 1;
                };
            }

            if (ViewerType == ViewerType.System)
            {
                cols[0].AspectGetter = delegate(object x)
                {
                    return ((TableBlock)x).Name;
                };

                //checkboxes for hidden shown objects
                objectListView1.BooleanCheckStateGetter = delegate(object x)
                {
                    return ((TableBlock)x).Visibility;
                };
                objectListView1.BooleanCheckStatePutter = delegate(object x, bool newValue)
                {
                    TableBlock block = (TableBlock)x;
                    if (block.ObjectType != SystemPresenter.ContentType.None)
                    {
                        block.Visibility = newValue;
                        OnDataVisibilityChanged(block);
                        return newValue;
                    }

                    return block.Visibility;
                };

                //show content type if possible otherwise group
                cols[2].AspectGetter = delegate(object x)
                {
                    TableBlock block = (TableBlock)x;
                    if (block.ObjectType != SystemPresenter.ContentType.None)
                        return block.ObjectType.ToString();

                    return block.Group;
                };
            }
            else
            {
                objectListView1.BooleanCheckStateGetter = null;
                objectListView1.BooleanCheckStatePutter = null;
            }

            //show ID + 1
            cols[1].AspectGetter = delegate(object x)
            {
                return ((TableBlock)x).ID + 1;
            };

            //show all options of a block in the tooltip
            objectListView1.CellToolTipGetter = delegate(OLVColumn col, Object x)
            {
                return ((TableBlock)x).ToolTip;
            };

            objectListView1.Columns.AddRange(cols);
        }

        void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(GetSelectedBlocks(), Data.TemplateIndex);
            OnContentChanged(this);
        }

        void Save(string file)
        {
            var fileManager = new FileManager(file, IsBINI)
                                          {
                                              WriteSpaces = Helper.Settings.Data.Data.General.FormattingSpaces,
                                              WriteEmptyLine = Helper.Settings.Data.Data.General.FormattingEmptyLine
                                          };
            fileManager.Write(Data.GetEditorData());

            SetAsSaved();

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
            File = file;

            var title = GetTitle();
            if (undoManager.IsModified())
                title += "*";

            TabText = title;
            Text = title;
            ToolTipText = File;

            OnDocumentChanged(this);
        }

        void SetAsSaved()
        {
            if (undoManager.IsModified())
            {
                undoManager.SetAsSaved();

                //set objects in listview as unmodified
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

        bool CancelClose()
        {
            if (undoManager.IsModified())
            {
                DialogResult dialogResult = MessageBox.Show(String.Format(Properties.Strings.FileCloseSave, GetTitle()), Helper.Assembly.Title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                    return true;
                if (dialogResult == DialogResult.Yes)
                    Save();
            }

            return false;
        }

        void AddBlocks(List<TableBlock> blocks, int undoBlock, bool undo)
        {
            List<TableBlock> selectedData = new List<TableBlock>();
            foreach (TableBlock block in blocks)
            {
                Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Values[block.Block.TemplateIndex];
                TableBlock tableBlock = block;

                //set block to be modified except in undo mode
                if (!undo)
                    tableBlock.Modified = TableModified.Changed;

                //set archetype of block
                if (tableBlock.Archetype == null)
                    SetObjectType(tableBlock, Archetype);

                bool existSingle = false;

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < Data.Blocks.Count; j++)
                    {
                        //block already exists
                        if (Data.Blocks[j].Block.TemplateIndex == block.Block.TemplateIndex)
                        {
                            tableBlock.ID = Data.Blocks[j].ID;
                            tableBlock.UniqueID = Data.Blocks[j].UniqueID;

                            //overwrite data if we add blocks which are single then they are overwritten which means they have to be changed to edit in the undo history
                            undoManager.CurrentData[undoBlock] = new ChangedData
                                                                     {
                                                                         Type = ChangedType.Edit,
                                                                         OldBlocks = new List<TableBlock> { Data.Blocks[j] },
                                                                         NewBlocks = new List<TableBlock> { tableBlock },
                                                                     };

                            //overwrite block
                            Data.Blocks[j] = tableBlock;
                            existSingle = true;

                            break;
                        }
                    }
                }

                if (!existSingle)
                {
                    if (block.ID >= Data.Blocks.Count)
                        Data.Blocks.Add(block);
                    else
                        Data.Blocks.Insert(block.ID, block);
                }

                selectedData.Add(tableBlock);
            }

            Data.RefreshID(blocks[0].ID);

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectedObjects = selectedData;
            EnsureSelectionVisible();
        }

        void AddNewBlock(string blockName, int templateIndex)
        {
            Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Values[templateIndex];

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

            //add new block under selected one if it exists otherwise at the end
            int id;
            if (objectListView1.SelectedIndices.Count > 0)
                id = objectListView1.SelectedIndices[objectListView1.SelectedIndices.Count - 1] + 1;
            else
                id = Data.Blocks.Count;

            //add actual block
            undoManager.Execute(new ChangedData { NewBlocks = new List<TableBlock> { new TableBlock(id, Data.MaxID++, editorBlock, Data.TemplateIndex) }, Type = ChangedType.Add });
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
                newBlocks.Add(ObjectClone.Clone(oldBlocks[i]));

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
                SetObjectType(block, Archetype);

            undoManager.Execute(new ChangedData { NewBlocks = newBlocks, OldBlocks = oldBlocks, Type = ChangedType.Edit });
        }

        void ChangeBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            for (int i = 0; i < oldBlocks.Count; i++)
            {
                int index = Data.Blocks.IndexOf(oldBlocks[i]);
                Data.Blocks[index] = newBlocks[i];
            }

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.RefreshObjects(Data.Blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(newBlocks);
            EnsureSelectionVisible();
        }

        private void MoveBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            //remove all moved blocks first because otherwise inserted index would be wrong
            List<TableBlock> blocks = new List<TableBlock>();
            for (int i = oldBlocks.Count - 1; i >= 0; i--)
            {
                blocks.Add(Data.Blocks[oldBlocks[i].ID]);
                Data.Blocks.RemoveAt(oldBlocks[i].ID);
            }

            //insert blocks at new position
            for (int i = 0; i < oldBlocks.Count; i++)
                Data.Blocks.Insert(newBlocks[i].ID, blocks[oldBlocks.Count - i - 1]);

            Data.RefreshID(Math.Min(oldBlocks[0].ID, newBlocks[0].ID));
            objectListView1.SetObjects(Data.Blocks);
            objectListView1.RefreshObjects(Data.Blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(blocks);
            EnsureSelectionVisible();
        }

        void DeleteBlocks(List<TableBlock> blocks)
        {
            System.Collections.IList selection = objectListView1.SelectedObjects;

            foreach (TableBlock tableBlock in blocks)
                Data.Blocks.Remove(tableBlock);

            Data.RefreshID(blocks[0].ID);
            objectListView1.RemoveObjects(blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(selection);
            EnsureSelectionVisible();
        }

        void DeleteSelectedBlocks()
        {
            List<TableBlock> blocks = new List<TableBlock>();

            foreach (TableBlock block in objectListView1.SelectedObjects)
                blocks.Add(block);

            undoManager.Execute(new ChangedData { NewBlocks = blocks, Type = ChangedType.Delete });
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

        void mnuDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedBlocks();
        }

        void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SetContextMenuEnabled();
        }

        void SetContextMenuEnabled()
        {
            bool isSelected = objectListView1.SelectedObjects.Count > 0;

            mnuDeleteSeperator.Visible = isSelected;
            mnuDelete.Visible = isSelected;
            mnuDelete.Enabled = isSelected;
        }

        void mnuAddItem_Click(object sender, EventArgs e)
        {
            string blockName = ((ToolStripMenuItem)sender).Text;
            int templateIndex = (int)((ToolStripMenuItem)sender).Tag;

            AddNewBlock(blockName, templateIndex);
        }

        public bool CanSave()
        {
            return true;
        }

        public bool CanCopy()
        {
            return objectListView1.SelectedObjects.Count > 0;
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
            return objectListView1.SelectedObjects.Count > 0;
        }

        public bool CanSelectAll()
        {
            return true;
        }

        public void Save()
        {
            if (File == string.Empty)
                SaveAs();
            else
                Save(File);
        }

        public void SaveAs()
        {
            var saveDialog = new SaveFileDialog { Filter = Properties.Strings.FileDialogFilter };
            if (saveDialog.ShowDialog() == DialogResult.OK)
                Save(saveDialog.FileName);
        }

        public void Add(int index)
        {
            AddNewBlock(mnuAdd.DropDownItems[index].Text, (int)mnuAdd.DropDownItems[index].Tag);
        }

        public void Delete()
        {
            DeleteSelectedBlocks();
        }

        public ToolStripDropDown MultipleAddDropDown()
        {
            return mnuAdd.DropDown;
        }

        public string GetTitle()
        {
            return File == string.Empty ? Properties.Strings.FileEditorNewFile : Path.GetFileName(File);
        }

        public void Copy()
        {
            EditorINIData data = new EditorINIData(Data.TemplateIndex);
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
                data.Blocks.Add(tableData.Block);

            Clipboard.Copy(data);

            OnContentChanged(this);
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
                //add new block under selected one if it exists otherwise at the end
                int id;
                if (objectListView1.SelectedIndices.Count > 0)
                    id = objectListView1.SelectedIndices[objectListView1.SelectedIndices.Count - 1] + 1;
                else
                    id = Data.Blocks.Count;

                List<TableBlock> blocks = new List<TableBlock>();
                for (int i = 0; i < editorData.Blocks.Count; i++)
                    blocks.Add(new TableBlock(id + i, Data.MaxID++, editorData.Blocks[i], Data.TemplateIndex));

                undoManager.Execute(new ChangedData { NewBlocks = blocks, Type = ChangedType.Add });
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

        public bool CanDisplay3DViewer()
        {
            return ViewerType != ViewerType.None;
        }

        void ExecuteDataChanged(ChangedData data, int undoBlock, bool undo)
        {
            switch (data.Type)
            {
                case ChangedType.Add:
                    AddBlocks(data.NewBlocks, undoBlock, undo);
                    break;
                case ChangedType.Delete:
                    DeleteBlocks(data.NewBlocks);
                    break;
                case ChangedType.Edit:
                    ChangeBlocks(data.NewBlocks, data.OldBlocks);
                    break;
                case ChangedType.Move:
                    MoveBlocks(data.NewBlocks, data.OldBlocks);
                    break;
            }

            OnDataChanged(data);
        }

        void UndoManager_DataChanged(List<ChangedData> data, bool undo)
        {
            for (int i = 0; i < data.Count; i++)
                ExecuteDataChanged(undo ? data[i].GetUndoData() : data[i], i, undo);

            SetFile(File);
            OnDocumentChanged(this);
        }

        public void Select(int id)
        {
            objectListView1.SelectedIndex = id;
            EnsureSelectionVisible();
        }

        public void SelectByUID(int id)
        {
            foreach (TableBlock block in Data.Blocks)
            {
                if (block.UniqueID == id)
                {
                    Select(block.ID);
                    return;
                }
            }
        }

        //overwrite to add extra information to layout.xml
        protected override string GetPersistString()
        {
            return GetType() + "," + File + "," + Data.TemplateIndex;
        }

        public void HideShowSelected()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return;

            bool visibility = !((TableBlock)objectListView1.SelectedObjects[0]).Visibility;

            foreach (TableBlock block in objectListView1.SelectedObjects)
            {
                if (block.ObjectType != SystemPresenter.ContentType.None)
                {
                    block.Visibility = visibility;
                    OnDataVisibilityChanged(block);
                }
            }

            objectListView1.RefreshObjects(objectListView1.SelectedObjects);
        }

        public bool CanChangeVisibility(bool rightNow)
        {
            bool correctFileType = ViewerType == ViewerType.System;
            if (rightNow)
                return correctFileType && objectListView1.SelectedObjects.Count > 0;

            return correctFileType;
        }

        public bool CanFocusSelected(bool rightNow)
        {
            bool correctFileType = ViewerType != ViewerType.None;
            if (rightNow)
                return correctFileType && objectListView1.SelectedObjects.Count > 0;

            return correctFileType;
        }

        public void ChangeVisibility()
        {
            HideShowSelected();
        }

        private void objectListView1_CanDrop(object sender, OlvDropEventArgs e)
        {
            if (e.DropTargetItem.RowObject is TableBlock)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void objectListView1_Dropped(object sender, OlvDropEventArgs e)
        {
            OLVDataObject o = e.DataObject as OLVDataObject;
            if (o != null)
            {
                List<TableBlock> blocks = new List<TableBlock>();
                foreach (TableBlock block in o.ModelObjects)
                    blocks.Add(block);

                StartMoveBlocks(blocks, e.DropTargetIndex);
            }
        }

        private void StartMoveBlocks(List<TableBlock> blocks, int targetIndex)
        {
            List<TableBlock> oldBlocks = new List<TableBlock>();
            List<TableBlock> newBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Count; i++)
            {
                //calculate correct insert position
                int newIndex = targetIndex + i;

                //decrease index if old blocks id is lower than the new index because they will be deleted first
                for (int j = i - newBlocks.Count; j < blocks.Count; j++)
                {
                    if (blocks[j].ID < newIndex)
                        newIndex--;
                }

                //skip block if the id was not changed
                if (blocks[i].ID != newIndex)
                {
                    newBlocks.Add(new TableBlock(newIndex, 0));
                    oldBlocks.Add(new TableBlock(blocks[i].ID, 0));
                }
            }

            if (oldBlocks.Count > 0)
                undoManager.Execute(new ChangedData { NewBlocks = newBlocks, OldBlocks = oldBlocks, Type = ChangedType.Move });
        }
    }

    public enum ViewerType
    {
        System,
        Universe,
        None
    }
}