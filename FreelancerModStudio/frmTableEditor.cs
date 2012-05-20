using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;
using FreelancerModStudio.Controls;
using FreelancerModStudio.Data;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Properties;
using FreelancerModStudio.SystemPresenter;
using FreelancerModStudio.SystemPresenter.Content;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmTableEditor : DockContent, IDocumentForm, IContentForm
    {
        public TableData Data;
        public string File;
        public string DataPath { get; private set; }

        readonly bool _isBINI;

        readonly UndoManager<ChangedData> _undoManager = new UndoManager<ChangedData>();

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
            {
                DataChanged(data);
            }
        }

        void OnSelectionChanged(List<TableBlock> data, int templateIndex)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(data, templateIndex);
            }
        }

        void OnDataVisibilityChanged(TableBlock block)
        {
            if (DataVisibilityChanged != null)
            {
                DataVisibilityChanged(block);
            }
        }

        void OnContentChanged(IContentForm content)
        {
            if (ContentChanged != null)
            {
                ContentChanged(content);
            }
        }

        void OnDocumentChanged(IDocumentForm document)
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(document);
            }
        }

        public frmTableEditor(int templateIndex, string file)
        {
            InitializeComponent();

            LoadIcons();
            _undoManager.DataChanged += UndoManager_DataChanged;

            if (file != null)
            {
                FileManager fileManager = new FileManager(file);
                EditorINIData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

                Data = new TableData(iniContent);
                _isBINI = fileManager.IsBINI;

                SetFile(file);
            }
            else
            {
                Data = new TableData
                    {
                        TemplateIndex = templateIndex
                    };
                _isBINI = false;

                SetFile(string.Empty);
            }

            objectListView1.CellToolTip.InitialDelay = 1000;
            objectListView1.UnfocusedHighlightBackgroundColor = objectListView1.HighlightBackgroundColorOrDefault;
            objectListView1.UnfocusedHighlightForegroundColor = objectListView1.HighlightForegroundColorOrDefault;

            SimpleDropSink dropSink = objectListView1.DropSink as SimpleDropSink;
            if (dropSink != null)
            {
                dropSink.CanDropBetween = true;
                dropSink.CanDropOnItem = false;
            }

            RefreshSettings();
        }

        void LoadIcons()
        {
            Icon = Resources.FileINIIcon;

            // synchronized with ContentType enum
            ImageList imageList = new ImageList
                {
                    ColorDepth = ColorDepth.Depth32Bit
                };
            imageList.Images.AddRange(new Image[]
                {
                    Resources.System,
                    Resources.LightSource,
                    Resources.Construct,
                    Resources.Depot,
                    Resources.DockingRing,
                    Resources.JumpGate,
                    Resources.JumpHole,
                    Resources.Planet,
                    Resources.Satellite,
                    Resources.Ship,
                    Resources.Station,
                    Resources.Sun,
                    Resources.TradeLane,
                    Resources.WeaponsPlatform,
                    Resources.Zone,
                    Resources.ZoneCylinder,
                    Resources.ZoneBox,
                    Resources.ZoneExclusion,
                    Resources.ZoneCylinderExclusion,
                    Resources.ZoneBoxExclusion,
                    Resources.ZoneVignette,
                    Resources.ZonePath,
                    Resources.ZonePathTrade,
                    Resources.ZonePathTradeLane
                });
            objectListView1.SmallImageList = imageList;
        }

        public void RefreshSettings()
        {
            objectListView1.EmptyListMsg = Strings.FileEditorEmpty;

            //display modified rows in different color
            objectListView1.RowFormatter = delegate(OLVListItem lvi)
                {
                    TableBlock block = (TableBlock)lvi.RowObject;
                    if (block.Modified == TableModified.Changed)
                    {
                        lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedColor;
                    }
                    else if (block.Modified == TableModified.ChangedSaved)
                    {
                        lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedSavedColor;
                    }

                    if (block.ObjectType != ContentType.None && !block.Visibility)
                    {
                        lvi.ForeColor = Helper.Settings.Data.Data.General.EditorHiddenColor;
                    }
                };

            //refresh column text
            if (objectListView1.Columns.Count > 2)
            {
                objectListView1.Columns[0].Text = Strings.FileEditorColumnName;
                objectListView1.Columns[2].Text = Strings.FileEditorColumnType;
            }

            //update 'New file' to new language
            if (File.Length == 0)
            {
                SetFile(string.Empty);
            }

            objectListView1.Refresh();
        }

        string ShowSolarArchetypeSelector()
        {
            OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = string.Format(Strings.FileEditorOpenSolarArch, GetFileName()),
                    Filter = "Solar Archetype INI|*.ini"
                };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                return openFile.FileName;
            }

            return null;
        }

        public void LoadArchetypes()
        {
            string archetypeFile = ArchetypeManager.GetRelativeArchetype(File, Data.TemplateIndex, Helper.Template.Data.SolarArchetypeFile);

            //user interaction required to get the path of the archetype file
            if (archetypeFile == null)
            {
                archetypeFile = ShowSolarArchetypeSelector();
            }

            //set data path based on archetype file and not system file
            DataPath = Helper.Template.Data.GetDataPath(archetypeFile, Helper.Template.Data.SolarArchetypeFile);

            if (Archetype == null)
            {
                Archetype = new ArchetypeManager(archetypeFile, Helper.Template.Data.SolarArchetypeFile);
            }

            foreach (TableBlock block in Data.Blocks)
            {
                SystemParser.SetObjectType(block, Archetype);

                if (block.ObjectType != ContentType.None)
                {
                    block.Visibility = true;
                }
            }
        }

        public void ShowData()
        {
            if (Data.TemplateIndex == Helper.Template.Data.SystemFile)
            {
                ViewerType = ViewerType.System;
            }
            else if (Data.TemplateIndex == Helper.Template.Data.UniverseFile)
            {
                ViewerType = ViewerType.Universe;
            }
            else
            {
                ViewerType = ViewerType.None;
            }

            if (ViewerType != ViewerType.None)
            {
                LoadArchetypes();
            }

#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif
            AddColumns();

            objectListView1.SetObjects(Data.Blocks);

            //add block types to add menu
            for (int i = 0; i < Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Count; ++i)
            {
                ToolStripMenuItem addItem = new ToolStripMenuItem
                    {
                        Text = Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Values[i].Name,
                        Tag = i
                    };
                addItem.Click += mnuAddItem_Click;
                mnuAdd.DropDownItems.Add(addItem);
            }
#if DEBUG
            st.Stop();
            Debug.WriteLine("display " + objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        void AddColumns()
        {
            //clear all items and columns
            objectListView1.Clear();

            objectListView1.CheckBoxes = ViewerType == ViewerType.System;

            OLVColumn[] cols =
                {
                    new OLVColumn(Strings.FileEditorColumnName, "Name"),
                    new OLVColumn("#", "ID"),
                    new OLVColumn(Strings.FileEditorColumnType, "Group")
                };

            cols[0].Width = 150;
            cols[1].Width = cols[1].MinimumWidth = cols[1].MaximumWidth = 34;
            cols[2].MinimumWidth = 120;
            cols[2].FillsFreeSpace = true;

            if (ViewerType != ViewerType.None)
            {
                // content type icons
                cols[0].ImageGetter = delegate(object x)
                    {
                        // ellipsoid + sphere and cylinder + ring have same icon
                        ContentType type = ((TableBlock)x).ObjectType;
                        if (type >= ContentType.ZoneRingExclusion)
                        {
                            return (int)type - 5;
                        }
                        if (type >= ContentType.ZoneEllipsoidExclusion)
                        {
                            return (int)type - 4;
                        }
                        if (type >= ContentType.ZoneRing)
                        {
                            return (int)type - 3;
                        }
                        if (type >= ContentType.ZoneEllipsoid)
                        {
                            return (int)type - 2;
                        }
                        return (int)type - 1;
                    };
            }

            if (ViewerType == ViewerType.System)
            {
                cols[0].AspectGetter = x => ((TableBlock)x).Name;

                //checkboxes for hidden shown objects
                objectListView1.BooleanCheckStateGetter = x => ((TableBlock)x).Visibility;
                objectListView1.BooleanCheckStatePutter = delegate(object x, bool newValue)
                    {
                        TableBlock block = (TableBlock)x;
                        if (block.ObjectType != ContentType.None)
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
                        if (block.ObjectType != ContentType.None)
                        {
                            return block.ObjectType.ToString();
                        }

                        return block.Group;
                    };
            }
            else
            {
                objectListView1.BooleanCheckStateGetter = null;
                objectListView1.BooleanCheckStatePutter = null;
            }

            //show ID + 1
            cols[1].AspectGetter = x => ((TableBlock)x).Index + 1;

            //show all options of a block in the tooltip
            objectListView1.CellToolTipGetter = (col, x) => ((TableBlock)x).ToolTip;

            objectListView1.Columns.AddRange(cols);
        }

        void objectListView1_SelectionChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(GetSelectedBlocks(), Data.TemplateIndex);
            OnContentChanged(this);
        }

        void Save(string file)
        {
            FileManager fileManager = new FileManager(file, _isBINI)
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

            string title = Title;
            if (_undoManager.IsModified())
            {
                title += "*";
            }

            TabText = title;
            Text = title;
            ToolTipText = File;

            OnDocumentChanged(this);
        }

        void SetAsSaved()
        {
            if (_undoManager.IsModified())
            {
                _undoManager.SetAsSaved();

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
            if (_undoManager.IsModified())
            {
                DialogResult dialogResult = MessageBox.Show(String.Format(Strings.FileCloseSave, Title), Helper.Assembly.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                {
                    return true;
                }
                if (dialogResult == DialogResult.Yes)
                {
                    Save();
                }
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
                {
                    tableBlock.Modified = TableModified.Changed;
                }

                //set archetype of block
                if (tableBlock.Archetype == null)
                {
                    SystemParser.SetObjectType(tableBlock, Archetype);
                }

                bool existSingle = false;

                //check if block already exists if it is a single block
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < Data.Blocks.Count; ++j)
                    {
                        //block already exists
                        if (Data.Blocks[j].Block.TemplateIndex == block.Block.TemplateIndex)
                        {
                            tableBlock.Index = Data.Blocks[j].Index;
                            tableBlock.Id = Data.Blocks[j].Id;

                            //overwrite data if we add blocks which are single then they are overwritten which means they have to be changed to edit in the undo history
                            _undoManager.CurrentData[undoBlock] = new ChangedData
                                {
                                    Type = ChangedType.Edit,
                                    OldBlocks = new List<TableBlock>
                                        {
                                            Data.Blocks[j]
                                        },
                                    NewBlocks = new List<TableBlock>
                                        {
                                            tableBlock
                                        },
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
                    if (block.Index >= Data.Blocks.Count)
                    {
                        Data.Blocks.Add(block);
                    }
                    else
                    {
                        Data.Blocks.Insert(block.Index, block);
                    }
                }

                selectedData.Add(tableBlock);
            }

            Data.RefreshIndices(blocks[0].Index);

            objectListView1.SetObjects(Data.Blocks);
            objectListView1.SelectedObjects = selectedData;
            EnsureSelectionVisible();
        }

        void AddNewBlock(string blockName, int templateIndex)
        {
            Template.Block templateBlock = Helper.Template.Data.Files[Data.TemplateIndex].Blocks.Values[templateIndex];

            //add options to new block
            EditorINIBlock editorBlock = new EditorINIBlock(blockName, templateIndex);
            for (int i = 0; i < templateBlock.Options.Count; ++i)
            {
                Template.Option option = templateBlock.Options[i];
                editorBlock.Options.Add(new EditorINIOption(option.Name, i));

                if (templateBlock.Identifier != null && templateBlock.Identifier.Equals(editorBlock.Options[editorBlock.Options.Count - 1].Name, StringComparison.OrdinalIgnoreCase))
                {
                    editorBlock.MainOptionIndex = editorBlock.Options.Count - 1;
                    editorBlock.Options[editorBlock.Options.Count - 1].Values.Add(new EditorINIEntry(blockName));
                }
            }

            //add new block under selected one if it exists otherwise at the end
            int id;
            if (objectListView1.SelectedIndices.Count > 0)
            {
                id = objectListView1.SelectedIndices[objectListView1.SelectedIndices.Count - 1] + 1;
            }
            else
            {
                id = Data.Blocks.Count;
            }

            //add actual block
            _undoManager.Execute(new ChangedData
                {
                    NewBlocks = new List<TableBlock>
                        {
                            new TableBlock(id, Data.MaxId++, editorBlock, Data.TemplateIndex)
                        },
                    Type = ChangedType.Add
                });
        }

        public List<TableBlock> GetSelectedBlocks()
        {
            if (objectListView1.SelectedObjects.Count == 0)
            {
                return null;
            }

            List<TableBlock> blocks = new List<TableBlock>();
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
            {
                blocks.Add(tableData);
            }

            return blocks;
        }

        public void SetBlocks(PropertyBlock[] blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();
            List<TableBlock> oldBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Length; ++i)
            {
                oldBlocks.Add((TableBlock)objectListView1.SelectedObjects[i]);
                newBlocks.Add(ObjectClone.Clone(oldBlocks[i]));

                for (int j = 0; j < blocks[i].Count; ++j)
                {
                    List<EditorINIEntry> options = newBlocks[i].Block.Options[j].Values;

                    if (blocks[i][j].Value is PropertySubOptions)
                    {
                        options.Clear();

                        //loop all sub values in the sub value collection
                        foreach (PropertyOption value in (PropertySubOptions)blocks[i][j].Value)
                        {
                            string text = ((string)value.Value).Trim();
                            if (text.Length != 0)
                            {
                                if (text.Contains(Environment.NewLine))
                                {
                                    string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    List<object> subOptions = new List<object>();
                                    for (int k = 1; k < lines.Length; ++k)
                                    {
                                        subOptions.Add(lines[k].Trim());
                                    }

                                    options.Add(new EditorINIEntry(lines[0], subOptions));
                                }
                                else
                                {
                                    options.Add(new EditorINIEntry(text));
                                }
                            }
                        }
                    }
                    else
                    {
                        string text = ((string)blocks[i][j].Value).Trim();
                        if (text.Length != 0)
                        {
                            if (options.Count > 0)
                            {
                                //check if value is different
                                if (options[0].Value.ToString() != text)
                                {
                                    options[0].Value = text;
                                }
                            }
                            else
                            {
                                options.Add(new EditorINIEntry(text));
                            }
                        }
                        else
                        {
                            options.Clear();
                        }

                        //change data in listview
                        if (newBlocks[i].Block.MainOptionIndex == j)
                        {
                            newBlocks[i].Name = text;
                        }
                    }
                }

                newBlocks[i].Modified = TableModified.Changed;
            }

            foreach (TableBlock block in newBlocks)
            {
                SystemParser.SetObjectType(block, Archetype);
            }

            _undoManager.Execute(new ChangedData
                {
                    NewBlocks = newBlocks,
                    OldBlocks = oldBlocks,
                    Type = ChangedType.Edit
                });
        }

        void ChangeBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            for (int i = 0; i < oldBlocks.Count; ++i)
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

        void MoveBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            //remove all moved blocks first because otherwise inserted index would be wrong
            List<TableBlock> blocks = new List<TableBlock>();
            for (int i = oldBlocks.Count - 1; i >= 0; i--)
            {
                blocks.Add(Data.Blocks[oldBlocks[i].Index]);
                Data.Blocks.RemoveAt(oldBlocks[i].Index);
            }

            //insert blocks at new position
            for (int i = 0; i < oldBlocks.Count; ++i)
            {
                Data.Blocks.Insert(newBlocks[i].Index, blocks[oldBlocks.Count - i - 1]);
            }

            Data.RefreshIndices(Math.Min(oldBlocks[0].Index, newBlocks[0].Index));
            objectListView1.SetObjects(Data.Blocks);
            objectListView1.RefreshObjects(Data.Blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(blocks);
            EnsureSelectionVisible();
        }

        void DeleteBlocks(List<TableBlock> blocks)
        {
            IList selection = objectListView1.SelectedObjects;

            foreach (TableBlock tableBlock in blocks)
            {
                Data.Blocks.Remove(tableBlock);
            }

            Data.RefreshIndices(blocks[0].Index);
            objectListView1.RemoveObjects(blocks);

            //select objects which were selected before
            objectListView1.SelectObjects(selection);
            EnsureSelectionVisible();
        }

        void DeleteSelectedBlocks()
        {
            List<TableBlock> blocks = new List<TableBlock>();

            foreach (TableBlock block in objectListView1.SelectedObjects)
            {
                blocks.Add(block);
            }

            _undoManager.Execute(new ChangedData
                {
                    NewBlocks = blocks,
                    Type = ChangedType.Delete
                });
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
            if (File.Length == 0)
            {
                SaveAs();
            }
            else
            {
                Save(File);
            }
        }

        public void SaveAs()
        {
            SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = Strings.FileDialogFilter
                };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                Save(saveDialog.FileName);
            }
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

        string GetFileName()
        {
            if (File.Length == 0)
            {
                return File;
            }

            return Path.GetFileName(File);
        }

        public string Title
        {
            get
            {
                return File.Length == 0 ? Strings.FileEditorNewFile : Path.GetFileName(File);
            }
        }

        public void Copy()
        {
            EditorINIData data = new EditorINIData(Data.TemplateIndex);
            foreach (TableBlock tableData in objectListView1.SelectedObjects)
            {
                data.Blocks.Add(tableData.Block);
            }

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
                {
                    id = objectListView1.SelectedIndices[objectListView1.SelectedIndices.Count - 1] + 1;
                }
                else
                {
                    id = Data.Blocks.Count;
                }

                List<TableBlock> blocks = new List<TableBlock>();
                for (int i = 0; i < editorData.Blocks.Count; ++i)
                {
                    blocks.Add(new TableBlock(id + i, Data.MaxId++, editorData.Blocks[i], Data.TemplateIndex));
                }

                _undoManager.Execute(new ChangedData
                    {
                        NewBlocks = blocks,
                        Type = ChangedType.Add
                    });
            }
        }

        public bool UseDocument()
        {
            return false;
        }

        public bool CanUndo()
        {
            return _undoManager.CanUndo();
        }

        public bool CanRedo()
        {
            return _undoManager.CanRedo();
        }

        public void Undo()
        {
            _undoManager.Undo(1);
        }

        public void Redo()
        {
            _undoManager.Redo(1);
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
            for (int i = 0; i < data.Count; ++i)
            {
                ExecuteDataChanged(undo ? data[i].GetUndoData() : data[i], i, undo);
            }

            SetFile(File);
            //OnDocumentChanged(this); is already called in SetFile
        }

        public void Select(TableBlock block)
        {
            objectListView1.SelectedObject = block;
            EnsureSelectionVisible();
        }

        public void SelectItemIndex(int value)
        {
            objectListView1.SelectedIndex = value;
            EnsureSelectionVisible();
        }

        public void Select(int id)
        {
            int itemIndex = 0;
            foreach (TableBlock block in objectListView1.Objects)
            {
                if (block.Id == id)
                {
                    SelectItemIndex(itemIndex);
                    return;
                }
                ++itemIndex;
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
            {
                return;
            }

            bool visibility = !((TableBlock)objectListView1.SelectedObjects[0]).Visibility;

            foreach (TableBlock block in objectListView1.SelectedObjects)
            {
                if (block.ObjectType != ContentType.None && block.Visibility != visibility)
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
            {
                return correctFileType && objectListView1.SelectedObjects.Count > 0;
            }

            return correctFileType;
        }

        public bool CanFocusSelected(bool rightNow)
        {
            bool correctFileType = ViewerType != ViewerType.None;
            if (rightNow)
            {
                return correctFileType && objectListView1.SelectedObjects.Count > 0;
            }

            return correctFileType;
        }

        public void ChangeVisibility()
        {
            HideShowSelected();
        }

        void objectListView1_CanDrop(object sender, OlvDropEventArgs e)
        {
            if (e.DropTargetItem.RowObject is TableBlock)
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        void objectListView1_Dropped(object sender, OlvDropEventArgs e)
        {
            OLVDataObject o = e.DataObject as OLVDataObject;
            if (o != null)
            {
                List<TableBlock> blocks = new List<TableBlock>();
                foreach (TableBlock block in o.ModelObjects)
                {
                    blocks.Add(block);
                }

                StartMoveBlocks(blocks, e.DropTargetIndex);
            }
        }

        void StartMoveBlocks(List<TableBlock> blocks, int targetIndex)
        {
            List<TableBlock> oldBlocks = new List<TableBlock>();
            List<TableBlock> newBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Count; ++i)
            {
                //calculate correct insert position
                int newIndex = targetIndex + i;

                //decrease index if old blocks id is lower than the new index because they will be deleted first
                for (int j = i - newBlocks.Count; j < blocks.Count; ++j)
                {
                    if (blocks[j].Index < newIndex)
                    {
                        newIndex--;
                    }
                }

                //skip block if the id was not changed
                if (blocks[i].Index != newIndex)
                {
                    newBlocks.Add(new TableBlock(newIndex, 0));
                    oldBlocks.Add(new TableBlock(blocks[i].Index, 0));
                }
            }

            if (oldBlocks.Count > 0)
            {
                _undoManager.Execute(new ChangedData
                    {
                        NewBlocks = newBlocks,
                        OldBlocks = oldBlocks,
                        Type = ChangedType.Move
                    });
            }
        }
    }

    public enum ViewerType
    {
        System,
        Universe,
        None
    }
}
