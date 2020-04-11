namespace FreelancerModStudio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using BrightIdeasSoftware;

    using FLUtils;

    using FreelancerModStudio.Controls;
    using FreelancerModStudio.Data;
    using FreelancerModStudio.Data.INI;
    using FreelancerModStudio.Data.IO;
    using FreelancerModStudio.Properties;
    using FreelancerModStudio.SystemDesigner;
    using FreelancerModStudio.SystemDesigner.Content;

    using WeifenLuo.WinFormsUI.Docking;

    using Clipboard = Data.Clipboard;

    public partial class FrmTableEditor : DockContent, IDocumentForm
    {
        public static FrmTableEditor Instance;

        public TableData Data;
        public string File { get; private set; }
        public string DataPath { get; private set; }

        internal readonly UndoManager<ChangedData> undoManager = new UndoManager<ChangedData>();

        private readonly bool isBini;

        public ViewerType ViewerType { get; set; }
        public ArchetypeManager Archetype { get; set; }

        public delegate void DataChangedType(ChangedData data);

        public DataChangedType DataChanged;

        public delegate void SelectionChangedType(List<TableBlock> data, int templateIndex);

        public SelectionChangedType SelectionChanged;

        public delegate void DataVisibilityChangedType(TableBlock block);

        public DataVisibilityChangedType DataVisibilityChanged;

        public delegate void DocumentChangedType(IDocumentForm document);

        public DocumentChangedType DocumentChanged;

        private void OnDataChanged(ChangedData data) => this.DataChanged?.Invoke(data);

        private void OnSelectionChanged(List<TableBlock> data, int templateIndex) => this.SelectionChanged?.Invoke(data, templateIndex);

        private void OnDataVisibilityChanged(TableBlock block) => this.DataVisibilityChanged?.Invoke(block);

        private void OnDocumentChanged(IDocumentForm document) => this.DocumentChanged?.Invoke(document);

        public FrmTableEditor(int templateIndex, string file)
        {
            FrmTableEditor.Instance = this;
            this.InitializeComponent();

            this.LoadIcons();
            this.undoManager.DataChanged += this.UndoManagerDataChanged;
            Helper.Settings.LoadTemplates();

            if (file != null)
            {
                FileManager fileManager = new FileManager(file)
                    {
                        ReadWriteComments = true, // always read comments
                    };
                EditorIniData iniContent = fileManager.Read(FileEncoding.Automatic, templateIndex);

                this.Data = new TableData(iniContent);
                this.isBini = fileManager.IsBini;

                this.SetFile(file);
            }
            else
            {
                this.Data = new TableData
                    {
                        TemplateIndex = templateIndex
                    };

                this.SetFile(string.Empty);
            }

            this.objectListView1.CellToolTip.InitialDelay = 1000;
            this.objectListView1.UnfocusedHighlightBackgroundColor = this.objectListView1.HighlightBackgroundColorOrDefault;
            this.objectListView1.UnfocusedHighlightForegroundColor = this.objectListView1.HighlightForegroundColorOrDefault;

            SimpleDropSink dropSink = this.objectListView1.DropSink as SimpleDropSink;
            if (dropSink != null)
            {
                dropSink.CanDropBetween = true;
                dropSink.CanDropOnItem = false;
            }

            this.RefreshSettings();
        }

        private void LoadIcons()
        {
            this.Icon = Resources.FileINIIcon;

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
            this.objectListView1.SmallImageList = imageList;
        }

        public void RefreshSettings()
        {
            this.objectListView1.EmptyListMsg = Strings.FileEditorEmpty;

            // display modified rows in different color
            this.objectListView1.RowFormatter = delegate(OLVListItem lvi)
                {
                    TableBlock block = (TableBlock)lvi.RowObject;
                    switch (block.Modified)
                    {
                        case TableModified.ChangedAdded:
                            lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedAddedColor;
                            break;
                        case TableModified.Changed:
                            lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedColor;
                            break;
                        case TableModified.ChangedSaved:
                            lvi.BackColor = Helper.Settings.Data.Data.General.EditorModifiedSavedColor;
                            break;
                    }

                    if (this.ViewerType == ViewerType.System && block.ObjectType != ContentType.None && !block.Visibility)
                    {
                        lvi.ForeColor = Helper.Settings.Data.Data.General.EditorHiddenColor;
                    }
                };

            // refresh column text
            if (this.objectListView1.Columns.Count > 2)
            {
                this.objectListView1.Columns[0].Text = Strings.FileEditorColumnName;
                this.objectListView1.Columns[2].Text = Strings.FileEditorColumnType;
            }

            // update 'New file' to new language
            // also needed to reset title after culture changer changed it
            this.SetFile(this.File);

            this.objectListView1.Refresh();
        }

        private string ShowSolarArchetypeSelector()
        {
            OpenFileDialog openFile = new OpenFileDialog
                {
                    Title = string.Format(Strings.FileEditorOpenSolarArch, this.GetFileName()),
                    Filter = "Solar Archetype INI|*.ini"
                };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                return openFile.FileName;
            }

            return null;
        }

        private void LoadArchetypes()
        {
            string archetypeFile = ArchetypeManager.GetRelativeArchetype(this.File, this.Data.TemplateIndex);

            // try to fallback to default DATA path
            if (archetypeFile == null)
            {
                archetypeFile = ArchetypeManager.GetRelativeArchetype(Helper.Settings.Data.Data.General.DefaultDataDirectory);

                // user interaction required to get the path of the archetype file
                if (archetypeFile == null)
                {
                    archetypeFile = this.ShowSolarArchetypeSelector();
                }
            }

            // set data path based on archetype file and not system file
            this.DataPath = Helper.Template.Data.GetDataPath(archetypeFile, Helper.Template.Data.SolarArchetypeFile);

            if (this.Archetype == null)
            {
                this.Archetype = new ArchetypeManager(archetypeFile, Helper.Template.Data.SolarArchetypeFile);
            }

            this.SetAllBlockTypes();
        }

        private void SetAllBlockTypes()
        {
            foreach (TableBlock block in this.Data.Blocks)
            {
                this.SetBlockType(block);
                block.SetVisibleIfPossible();
            }
        }

        private void SetBlockType(TableBlock block)
        {
            switch (this.ViewerType)
            {
                case ViewerType.System:
                    SystemParser.SetObjectType(block, this.Archetype);
                    break;
                case ViewerType.Universe:
                    SystemParser.SetUniverseObjectType(block);
                    break;
                case ViewerType.SolarArchetype:
                    SystemParser.SetSolarArchetypeObjectType(block);
                    break;
                case ViewerType.ModelPreview:
                    SystemParser.SetModelPreviewObjectType(block);
                    break;
            }
        }

        private void SetViewerType()
        {
            if (this.Data.TemplateIndex == Helper.Template.Data.SystemFile)
            {
                this.ViewerType = ViewerType.System;
            }
            else if (this.Data.TemplateIndex == Helper.Template.Data.UniverseFile)
            {
                this.ViewerType = ViewerType.Universe;
            }
            else if (this.Data.TemplateIndex == Helper.Template.Data.SolarArchetypeFile)
            {
                this.ViewerType = ViewerType.SolarArchetype;
            }
            else if (this.Data.TemplateIndex == Helper.Template.Data.AsteroidArchetypeFile || this.Data.TemplateIndex == Helper.Template.Data.ShipArchetypeFile || this.Data.TemplateIndex == Helper.Template.Data.EquipmentFile || this.Data.TemplateIndex == Helper.Template.Data.EffectExplosionsFile)
            {
                this.ViewerType = ViewerType.ModelPreview;
            }
            else
            {
                this.ViewerType = ViewerType.None;
            }
        }

        public void ShowData()
        {
            this.SetViewerType();
            switch (this.ViewerType)
            {
                case ViewerType.SolarArchetype:
                case ViewerType.ModelPreview:
                    this.DataPath = Helper.Template.Data.GetDataPath(this.File, Helper.Template.Data.SolarArchetypeFile);
                    this.SetAllBlockTypes();
                    break;
                case ViewerType.System:
                case ViewerType.Universe:
                    this.LoadArchetypes();
                    break;
            }

#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif
            this.AddColumns();

            this.objectListView1.SetObjects(this.Data.Blocks);

            // add block types to add menu
            for (int i = 0; i < Helper.Template.Data.Files[this.Data.TemplateIndex].Blocks.Count; ++i)
            {
                ToolStripMenuItem addItem = new ToolStripMenuItem
                    {
                        Text = Helper.Template.Data.Files[this.Data.TemplateIndex].Blocks.Values[i].Name,
                        Tag = i
                    };
                addItem.Click += this.MnuAddItemClick;
                this.mnuAdd.DropDownItems.Add(addItem);
            }

#if DEBUG
            st.Stop();
            Debug.WriteLine("display " + this.objectListView1.Items.Count + " data: " + st.ElapsedMilliseconds + "ms");
#endif
        }

        private void AddColumns()
        {
            // clear all items and columns
            this.objectListView1.Clear();

            this.objectListView1.CheckBoxes = this.ViewerType == ViewerType.System;

            OLVColumn[] cols =
                {
                    new OLVColumn(Strings.FileEditorColumnName, "Name"),
                    new OLVColumn("#", "ID"),
                    new OLVColumn(Strings.FileEditorColumnType, "Group")
                };

            cols[0].Width = 150;

            cols[1].Width = 36;

            cols[2].MinimumWidth = 120;
            cols[2].FillsFreeSpace = true;

            if (this.ViewerType != ViewerType.None)
            {
                // content type icons
                cols[0].ImageGetter = delegate(object x)
                    {
                        // basic model + system and ellipsoid + sphere and cylinder + ring share the same icon
                        ContentType type = ((TableBlock)x).ObjectType;
                        if (type == ContentType.ModelPreview)
                        {
                            return 0;
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

            if (this.ViewerType == ViewerType.System)
            {
                cols[0].AspectGetter = x => ((TableBlock)x).Name;

                // checkboxes for hidden shown objects
                this.objectListView1.BooleanCheckStateGetter = x => ((TableBlock)x).Visibility;
                this.objectListView1.BooleanCheckStatePutter = delegate(object x, bool newValue)
                    {
                        TableBlock block = (TableBlock)x;
                        if (block.ObjectType != ContentType.None)
                        {
                            block.Visibility = newValue;
                            this.OnDataVisibilityChanged(block);
                            return newValue;
                        }

                        return block.Visibility;
                    };
            }
            else
            {
                this.objectListView1.BooleanCheckStateGetter = null;
                this.objectListView1.BooleanCheckStatePutter = null;
            }

            if (this.ViewerType == ViewerType.System || this.ViewerType == ViewerType.SolarArchetype)
            {
                // show content type if possible otherwise group
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

            // show ID + 1
            cols[1].AspectGetter = x => ((TableBlock)x).Index + 1;

            // show all options of a block in the tooltip
            this.objectListView1.CellToolTipGetter = (col, x) => ((TableBlock)x).ToolTip;

            this.objectListView1.Columns.AddRange(cols);
        }

        private void ObjectListView1SelectionChanged(object sender, EventArgs e)
        {
            this.OnSelectionChanged(this.GetSelectedBlocks(), this.Data.TemplateIndex);
            this.OnDocumentChanged(this);
        }

        private void Save(string file)
        {
            FileManager fileManager = new FileManager(file, this.isBini)
                {
                    WriteSpaces = Helper.Settings.Data.Data.General.FormattingSpaces,
                    WriteEmptyLine = Helper.Settings.Data.Data.General.FormattingEmptyLine,
                    ReadWriteComments = Helper.Settings.Data.Data.General.FormattingComments,
                };
            fileManager.Write(this.Data.GetEditorData());

            this.SetAsSaved();

            try
            {
                this.SetFile(file);
            }
            catch (Exception ex)
            {
                Helper.Exceptions.Show(ex);
            }
        }

        private void SetFile(string file)
        {
            this.File = file;
            this.ToolTipText = this.File;

            string title = this.GetTitle();
            if (this.undoManager.IsModified())
            {
                title += "*";
            }

            if (this.Text == title)
            {
                return;
            }

            this.Text = title;
            this.TabText = title;

            this.OnDocumentChanged(this);
        }

        private void SetAsSaved()
        {
            if (this.undoManager.IsModified())
            {
                this.undoManager.SetAsSaved();

                // set objects in listview as unmodified
                foreach (TableBlock tableData in this.objectListView1.Objects)
                {
                    if (tableData.Modified == TableModified.Changed ||
                        tableData.Modified == TableModified.ChangedAdded)
                    {
                        tableData.Modified = TableModified.ChangedSaved;
                        this.objectListView1.RefreshObject(tableData);
                    }
                }
            }
        }

        private bool CancelClose()
        {
            if (this.undoManager.IsModified())
            {
                DialogResult dialogResult = MessageBox.Show(string.Format(Strings.FileCloseSave, this.GetTitle()),  AssemblyUtils.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Cancel)
                {
                    return true;
                }

                if (dialogResult == DialogResult.Yes)
                {
                    this.Save();
                }
            }

            return false;
        }

        private void BlocksAdded(List<TableBlock> blocks)
        {
            List<TableBlock> selectedData = new List<TableBlock>();
            foreach (TableBlock block in blocks)
            {
                if (block.Index >= this.Data.Blocks.Count)
                {
                    this.Data.Blocks.Add(block);
                }
                else
                {
                    this.Data.Blocks.Insert(block.Index, block);
                }

                selectedData.Add(block);
            }

            this.Data.RefreshIndices(blocks[0].Index);

            this.objectListView1.SetObjects(this.Data.Blocks);
            this.objectListView1.SelectedObjects = selectedData;
            this.EnsureSelectionVisible();
        }

        private void AddBlock(string blockName, int templateIndex)
        {
            Template.Block templateBlock = Helper.Template.Data.Files[this.Data.TemplateIndex].Blocks.Values[templateIndex];

            // add options to new block
            EditorIniBlock editorBlock = new EditorIniBlock(blockName, templateIndex);
            for (int i = 0; i < templateBlock.Options.Count; ++i)
            {
                Template.Option option = templateBlock.Options[i];
                editorBlock.Options.Add(new EditorIniOption(option.Name, i));

                if (templateBlock.Identifier != null && templateBlock.Identifier.Equals(editorBlock.Options[editorBlock.Options.Count - 1].Name, StringComparison.OrdinalIgnoreCase))
                {
                    editorBlock.MainOptionIndex = editorBlock.Options.Count - 1;
                    editorBlock.Options[editorBlock.Options.Count - 1].Values.Add(new EditorIniEntry(blockName));
                }
            }

            // add actual block
            this.AddBlocks(new List<TableBlock>
                {
                    new TableBlock(this.GetNewBlockId(), this.Data.MaxId++, editorBlock, this.Data.TemplateIndex)
                });
        }

        public List<TableBlock> GetSelectedBlocks()
        {
            if (this.objectListView1.SelectedObjects.Count == 0)
            {
                return null;
            }

            List<TableBlock> blocks = new List<TableBlock>();
            foreach (TableBlock tableData in this.objectListView1.SelectedObjects)
            {
                blocks.Add(tableData);
            }

            return blocks;
        }

        /// <summary>
        /// A basic function to check whether a value is of the right type as indicated by Template.xml. Defaults to a string.
        /// </summary>
        /// <param name="val">The value casted to a string</param>
        /// <param name="type">The thing we want to see if it can parse as</param>
        /// <returns>True if it is in the right format, false otherwise.</returns>
        private bool CanCast(string val, Template.OptionType type)
        {
            val = val.Replace(" ", string.Empty);
            if (val.Length == 0) // Allow people to delete values
                return true;

            switch (type)
            {
                // Freelancer is a bit weird. 0, 1, true, and false are all allowed for Bool.
                case Template.OptionType.Bool:
                    return val == "true" || val == "false" || val == "0" || val == "1";

                // Default case
                case Template.OptionType.String:
                    return true;

                // We parse long because sometimes it would be uint and other times int. I couldn't be arsed writing a case for both.
                case Template.OptionType.Int:
                    return long.TryParse(val, out long i);

                // Points in Freelancer are almost always floats, rather than ints. 
                case Template.OptionType.Point:
                    if (val.Count(s => s == ',') != 1)
                        return false;

                    string[] point = val.Split(',');
                    return float.TryParse(point[0], out float f) && float.TryParse(point[1], out float ff);

                case Template.OptionType.Double:
                    return double.TryParse(val, out double d);

                case Template.OptionType.Path:
                    return !(val.Contains(Path.GetInvalidFileNameChars().ToString()) || val.Contains(Path.GetInvalidPathChars().ToString()));

                case Template.OptionType.Vector:
                    if (val.Count(s => s == ',') != 2)
                        return false;

                    string[] vec = val.Split(',');
                    return double.TryParse(vec[0], out double dd) && double.TryParse(vec[1], out double ddd) && double.TryParse(vec[2], out double dddd);

                case Template.OptionType.Rgb:
                    if (val.Count(s => s == ',') != 2)
                        return false;

                    string[] rgb = val.Split(',');
                    return byte.TryParse(rgb[0], out byte b) && byte.TryParse(rgb[1], out byte bb) && byte.TryParse(rgb[2], out byte bbb);

                case Template.OptionType.StringArray:
                    return val.Contains(',');

                case Template.OptionType.IntArray:
                    if (!val.Contains(','))
                        return false;

                    var intArr = val.Split(',');
                    foreach (var intVal in intArr)
                        if (!long.TryParse(intVal, out long iVal))
                            return false;
                    return true;

                case Template.OptionType.DoubleArray:
                    if (!val.Contains(','))
                        return false;

                    var douArr = val.Split(',');
                    foreach (var douVal in douArr)
                        if (!double.TryParse(douVal, out double dVal))
                            return false;
                    return true;

                default:
                    return false;
            }
        }

        public void ChangeBlocks(PropertyBlock[] propertyBlocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();
            List<TableBlock> oldBlocks = new List<TableBlock>();

            for (int i = 0; i < propertyBlocks.Length; ++i)
            {
                TableBlock oldBlock = (TableBlock)this.objectListView1.SelectedObjects[i];
                TableBlock newBlock = ObjectClone.Clone(oldBlock);

                oldBlocks.Add(oldBlock);
                newBlocks.Add(newBlock);

                PropertyBlock propertyBlock = propertyBlocks[i];
                
                // set comments (last property option)
                string comments = (string)propertyBlock[propertyBlock.Count - 1].Value;
                newBlock.Block.Comments = comments;

                // set options
                for (int j = 0; j < propertyBlock.Count - 1; ++j)
                {
                    List<EditorIniEntry> options = newBlock.Block.Options[j].Values;

                    if (propertyBlock[j].Value is PropertySubOptions propertyOptions)
                    {
                        options.Clear();

                        // loop all sub values in the sub value collection
                        foreach (PropertyOption value in propertyOptions)
                        {
                            string text = ((string)value.Value).Trim();

                            if (!this.CanCast(text, value.Type))
                            {
                                if (Helper.Settings.Data.Data.General.AlertIncorrectPropertyType)
                                    MessageBox.Show(string.Format(Strings.InvalidIniPropertyNotificationText, value.Name, value.Type.ToString()), Strings.InvalidIniPropertyNotificationCaption, MessageBoxButtons.OK);
                                this.ChangeBlocks(newBlocks, oldBlocks);
                                return;
                            }

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

                                    options.Add(new EditorIniEntry(lines[0], subOptions));
                                }
                                else
                                {
                                    options.Add(new EditorIniEntry(text));
                                }
                            }
                        }
                    }
                    else
                    {
                        var blk = propertyBlock[j];
                        string text = ((string)blk.Value).Trim();
                        if (!this.CanCast(text, propertyBlock[j].Type))
                        {
                            if (Helper.Settings.Data.Data.General.AlertIncorrectPropertyType)
                                MessageBox.Show(string.Format(Strings.InvalidIniPropertyNotificationText, blk.Name, blk.Type.ToString()), Strings.InvalidIniPropertyNotificationCaption, MessageBoxButtons.OK);
                            this.ChangeBlocks(newBlocks, oldBlocks);
                            return;
                        }

                        if (text.Length != 0)
                        {
                            if (options.Count > 0)
                            {
                                // check if value is different
                                if (options[0].Value.ToString() != text)
                                {
                                    options[0].Value = text;
                                }
                            }
                            else
                            {
                                options.Add(new EditorIniEntry(text));
                            }
                        }
                        else
                        {
                            options.Clear();
                        }

                        // change data in listview
                        if (newBlock.Block.MainOptionIndex == j)
                        {
                            newBlock.Name = text;
                        }
                    }
                }

                // update block object type
                this.SetBlockType(newBlock);

                // make block visible if it can be made visible now
                if (oldBlock.ObjectType == ContentType.None)
                {
                    newBlock.SetVisibleIfPossible();
                }

                // mark block as modified
                newBlock.SetModifiedChanged();
            }

            this.ChangeBlocks(newBlocks, oldBlocks);
        }

        public void ChangeBlocks(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            this.undoManager.Execute(new ChangedData
                {
                    NewBlocks = newBlocks,
                    OldBlocks = oldBlocks,
                    Type = ChangedType.Edit
                });
        }

        private void BlocksChanged(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            for (int i = 0; i < oldBlocks.Count; ++i)
            {
                // Technically there is a possibility someone copying a table from just the right place in just the right way would crash this
                // But only way to fix the change in rounding
                int index = this.Data.Blocks.FindIndex(x => x.Index == oldBlocks[i].Index && x.Name == oldBlocks[i].Name);
                this.Data.Blocks[index] = newBlocks[i];
            }

            this.objectListView1.SetObjects(this.Data.Blocks);
            this.objectListView1.RefreshObjects(this.Data.Blocks);

            // select objects which were selected before
            this.objectListView1.SelectObjects(newBlocks);
            this.EnsureSelectionVisible();
        }

        private void BlocksMoved(List<TableBlock> newBlocks, List<TableBlock> oldBlocks)
        {
            // remove all moved blocks first because otherwise inserted index would be wrong
            List<TableBlock> blocks = new List<TableBlock>();
            for (int i = oldBlocks.Count - 1; i >= 0; i--)
            {
                blocks.Add(this.Data.Blocks[oldBlocks[i].Index]);
                this.Data.Blocks.RemoveAt(oldBlocks[i].Index);
            }

            // insert blocks at new position
            for (int i = 0; i < oldBlocks.Count; ++i)
            {
                this.Data.Blocks.Insert(newBlocks[i].Index, blocks[oldBlocks.Count - i - 1]);
            }

            this.Data.RefreshIndices(Math.Min(oldBlocks[0].Index, newBlocks[0].Index));
            this.objectListView1.SetObjects(this.Data.Blocks);
            this.objectListView1.RefreshObjects(this.Data.Blocks);

            // select objects which were selected before
            this.objectListView1.SelectObjects(blocks);
            this.EnsureSelectionVisible();
        }

        private void BlocksDeleted(List<TableBlock> blocks)
        {
            IList selectedObjects = this.objectListView1.SelectedObjects;

            foreach (TableBlock tableBlock in blocks)
            {
                this.Data.Blocks.Remove(tableBlock);
            }

            this.Data.RefreshIndices(blocks[0].Index);
            this.objectListView1.RemoveObjects(blocks);

            // select objects which were selected before
            this.objectListView1.SelectObjects(selectedObjects);
            this.EnsureSelectionVisible();
        }

        private void DeleteSelectedBlocks()
        {
            List<TableBlock> blocks = new List<TableBlock>();

            foreach (TableBlock block in this.objectListView1.SelectedObjects)
            {
                blocks.Add(block);
            }

            this.undoManager.Execute(new ChangedData
                {
                    NewBlocks = blocks,
                    Type = ChangedType.Delete
                });
        }

        private void EnsureSelectionVisible()
        {
            if (this.objectListView1.SelectedObjects.Count > 0)
            {
                this.objectListView1.EnsureVisible(this.objectListView1.IndexOf(this.objectListView1.SelectedObjects[this.objectListView1.SelectedObjects.Count - 1]));
                this.objectListView1.EnsureVisible(this.objectListView1.IndexOf(this.objectListView1.SelectedObjects[0]));
            }
        }

        private void FrmDefaultEditorFormClosing(object sender, FormClosingEventArgs e) => e.Cancel = this.CancelClose();

        public void SelectAll() => this.objectListView1.SelectAll();

        private void ContextMenuStrip1Opening(object sender, CancelEventArgs e) => this.SetContextMenuEnabled();

        private void SetContextMenuEnabled()
        {
            bool active = this.ObjectSelected();
            this.mnuCut.Visible = active;
            this.mnuCut.Enabled = active;
            this.mnuCopy.Visible = active;
            this.mnuCopy.Enabled = active;
            this.mnuCreateTemplateFrom.Visible = active;
            this.mnuCreateTemplateFrom.Enabled = active;

            this.mnuPaste.Enabled = this.CanPaste();

            active = this.CanDelete();
            this.mnuDelete.Visible = active;
            this.mnuDelete.Enabled = active;
        }

        private void MnuAddItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            this.AddBlock(menuItem.Text, (int)menuItem.Tag);
        }

        private void MnuDeleteClick(object sender, EventArgs e) => this.DeleteSelectedBlocks();

        private void MnuCutClick(object sender, EventArgs e) => this.Cut();

        private void MnuCopyClick(object sender, EventArgs e) => this.Copy();

        private void MnuPasteClick(object sender, EventArgs e) => this.Paste();

        public bool CanSave() => true;

        public bool ObjectSelected() => this.objectListView1.SelectedObjects.Count > 0;

        public bool CanPaste() => Clipboard.CanPaste(typeof(EditorIniData));

        public bool CanAdd() => true;

        public bool CanDelete() => this.objectListView1.SelectedObjects.Count > 0;

        public bool CanSelectAll() => true;

        public void Save()
        {
            if (this.File.Length == 0)
                this.SaveAs();
            else
                this.Save(this.File);
        }

        public void SaveAs()
        {
            SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = Strings.FileDialogFilter
                };
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                this.Save(saveDialog.FileName);
            }
        }

        public void AddTemplate(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            var template = Helper.Settings.Data.Data.General.Templates.Templates.Where(p => p.Options.Any(x => x.Name == "nickname" && x.Values.Any(y => y.Value.ToString() == item.Text))).First();

            TableBlock block = new TableBlock(this.GetNewBlockId(), this.Data.MaxId++, template, this.Data.TemplateIndex);
            block.SetVisibleIfPossible();

            if (this.Data.Blocks.Find(x => x.Block.Options.Any(y => y.Name == "nickname" && y.Values.Any(z => z.Value.ToString() == item.Text))) != null)
            {
                MessageBox.Show("An item with the nickname of the template already exists within this document.");
                return;
            }

            // add actual block
            this.AddBlocks(new List<TableBlock> { block });
        }

        public void Delete() => this.DeleteSelectedBlocks();

        public ToolStripDropDown MultipleAddDropDown() => this.mnuAdd.DropDown;

        private string GetFileName() => this.File.Length == 0 ? this.File : Path.GetFileName(this.File);

        public string GetTitle() => this.File.Length == 0 ? Strings.FileEditorNewFile : Path.GetFileName(this.File);

        public void Copy()
        {
            EditorIniData data = new EditorIniData(this.Data.TemplateIndex);
            foreach (TableBlock tableData in this.objectListView1.SelectedObjects)
            {
                data.Blocks.Add(tableData.Block);
            }

            Clipboard.Copy(data, typeof(EditorIniData));

            this.OnDocumentChanged(this);
        }

        public void Cut()
        {
            this.Copy();
            this.DeleteSelectedBlocks();
        }

        public void Paste()
        {
            EditorIniData editorData = (EditorIniData)Clipboard.Paste(typeof(EditorIniData));

            if (editorData.TemplateIndex == this.Data.TemplateIndex)
            {
                int id = this.GetNewBlockId();

                List<TableBlock> blocks = new List<TableBlock>();
                for (int i = 0; i < editorData.Blocks.Count; ++i)
                {
                    blocks.Add(new TableBlock(id + i, this.Data.MaxId++, editorData.Blocks[i], this.Data.TemplateIndex));
                }

                this.AddBlocks(blocks);
            }
        }

        private void AddBlocks(List<TableBlock> blocks)
        {
            List<TableBlock> newAdditionalBlocks = new List<TableBlock>();
            List<TableBlock> oldBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Count; ++i)
            {
                TableBlock block = blocks[i];

                // set block to be modified
                block.Modified = TableModified.ChangedAdded;

                // set archetype of block and make visible if possible
                if (block.Archetype == null)
                {
                    this.SetBlockType(block);
                    block.SetVisibleIfPossible();
                }

                // check if block which can only exist once already exists
                Template.Block templateBlock = Helper.Template.Data.Files[this.Data.TemplateIndex].Blocks.Values[block.Block.TemplateIndex];
                if (!templateBlock.Multiple)
                {
                    for (int j = 0; j < this.Data.Blocks.Count; ++j)
                    {
                        TableBlock existingBlock = this.Data.Blocks[j];

                        // block already exists
                        if (existingBlock.Block.TemplateIndex == block.Block.TemplateIndex)
                        {
                            block.Index = existingBlock.Index;
                            block.Id = existingBlock.Id;

                            // overwrite block if it can only exist once
                            newAdditionalBlocks.Add(block);
                            oldBlocks.Add(existingBlock);

                            // remove overwritten block from new ones
                            blocks.RemoveAt(i);
                            --i;
                        }
                    }
                }
            }

            if (newAdditionalBlocks.Count == 0)
            {
                this.undoManager.Execute(new ChangedData
                    {
                        Type = ChangedType.Add,
                        NewBlocks = blocks,
                    });
            }
            else
            {
                if (blocks.Count == 0)
                {
                    this.undoManager.Execute(new ChangedData
                        {
                            Type = ChangedType.Edit,
                            NewBlocks = newAdditionalBlocks,
                            OldBlocks = oldBlocks,
                        });
                }
                else
                {
                    this.undoManager.Execute(new ChangedData
                        {
                            Type = ChangedType.AddAndEdit,
                            NewBlocks = blocks,
                            NewAdditionalBlocks = newAdditionalBlocks,
                            OldBlocks = oldBlocks,
                        });
                }
            }
        }

        public bool CanUndo()
        {
            return this.undoManager.CanUndo();
        }

        public bool CanRedo()
        {
            return this.undoManager.CanRedo();
        }

        public void Undo()
        {
            this.undoManager.Undo(1);
        }

        public void Redo()
        {
            this.undoManager.Redo(1);
        }

        public bool CanDisplay3DViewer()
        {
            return this.ViewerType != ViewerType.None;
        }

        public bool CanManipulatePosition()
        {
            return this.ViewerType == ViewerType.System || this.ViewerType == ViewerType.Universe;
        }

        public bool CanManipulateRotationScale()
        {
            return this.ViewerType == ViewerType.System;
        }

        private void ExecuteDataChanged(ChangedData data, bool undo)
        {
            switch (data.Type)
            {
                case ChangedType.Add:
                    this.BlocksAdded(data.NewBlocks);
                    break;
                case ChangedType.Edit:
                    this.BlocksChanged(data.NewBlocks, data.OldBlocks);
                    break;
                case ChangedType.Move:
                    this.BlocksMoved(data.NewBlocks, data.OldBlocks);
                    break;
                case ChangedType.Delete:
                    this.BlocksDeleted(data.NewBlocks);
                    break;
                case ChangedType.AddAndEdit:
                    this.BlocksAdded(data.NewBlocks);
                    this.BlocksChanged(data.NewAdditionalBlocks, data.OldBlocks);
                    break;
                case ChangedType.DeleteAndEdit:
                    this.BlocksDeleted(data.NewBlocks);
                    this.BlocksChanged(data.NewAdditionalBlocks, data.OldBlocks);
                    break;
            }

            this.OnDataChanged(data);
        }

        private void UndoManagerDataChanged(ChangedData data, bool undo)
        {
            this.ExecuteDataChanged(undo ? data.GetUndoData() : data, undo);

            this.SetFile(this.File);

            // OnDocumentChanged(this); is already called in SetFile
        }

        public void Select(TableBlock block, bool toggle)
        {
            if (toggle)
            {
                IList selectedObjects = this.objectListView1.SelectedObjects;

                if (this.objectListView1.IsSelected(block))
                {
                    selectedObjects.Remove(block);
                }
                else
                {
                    selectedObjects.Add(block);
                }

                this.objectListView1.SelectedObjects = selectedObjects;
            }
            else
            {
                this.objectListView1.SelectedObject = block;
            }

            this.EnsureSelectionVisible();
        }

        public void SelectItemIndex(int value)
        {
            this.objectListView1.SelectedIndex = value;
            this.EnsureSelectionVisible();
        }

        public void Select(int id)
        {
            int itemIndex = 0;
            foreach (TableBlock block in this.objectListView1.Objects)
            {
                if (block.Id == id)
                {
                    this.SelectItemIndex(itemIndex);
                    return;
                }

                ++itemIndex;
            }
        }

        // overwrite to add extra information to layout.xml
        protected override string GetPersistString()
        {
            return typeof(FrmTableEditor) + "," + this.File + "," + this.Data.TemplateIndex;
        }

        public void HideShowSelected()
        {
            IList selectedObjects = this.objectListView1.SelectedObjects;
            if (selectedObjects.Count == 0)
            {
                return;
            }

            bool visibility = !((TableBlock)selectedObjects[0]).Visibility;

            foreach (TableBlock block in selectedObjects)
            {
                if (block.ObjectType != ContentType.None && block.Visibility != visibility)
                {
                    block.Visibility = visibility;
                    this.OnDataVisibilityChanged(block);
                }
            }

            this.objectListView1.RefreshObjects(selectedObjects);
        }

        public bool CanChangeVisibility(bool rightNow)
        {
            bool correctFileType = this.ViewerType == ViewerType.System;
            if (rightNow)
            {
                return correctFileType && this.objectListView1.SelectedObjects.Count > 0;
            }

            return correctFileType;
        }

        public bool CanFocusSelected(bool rightNow)
        {
            bool correctFileType = this.ViewerType != ViewerType.None;
            if (rightNow)
            {
                return correctFileType && this.objectListView1.SelectedObjects.Count > 0;
            }

            return correctFileType;
        }

        public bool CanTrackSelected(bool rightNow)
        {
            bool correctFileType = this.ViewerType == ViewerType.System;
            if (rightNow)
            {
                return correctFileType && this.objectListView1.SelectedObjects.Count > 0;
            }

            return correctFileType;
        }

        public void ChangeVisibility()
        {
            this.HideShowSelected();
        }

        private void ObjectListView1CanDrop(object sender, OlvDropEventArgs e)
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

        private void ObjectListView1Dropped(object sender, OlvDropEventArgs e)
        {
            OLVDataObject o = e.DataObject as OLVDataObject;
            if (o != null)
            {
                List<TableBlock> blocks = new List<TableBlock>();
                foreach (TableBlock block in o.ModelObjects)
                {
                    blocks.Add(block);
                }

                this.MoveBlocks(blocks, e.DropTargetIndex);
            }
        }

        private void MoveBlocks(List<TableBlock> blocks, int targetIndex)
        {
            List<TableBlock> oldBlocks = new List<TableBlock>();
            List<TableBlock> newBlocks = new List<TableBlock>();

            for (int i = 0; i < blocks.Count; ++i)
            {
                // calculate correct insert position
                int newIndex = targetIndex + i;

                // decrease index if old blocks id is lower than the new index because they will be deleted first
                for (int j = i - newBlocks.Count; j < blocks.Count; ++j)
                {
                    if (blocks[j].Index < newIndex)
                    {
                        newIndex--;
                    }
                }

                // skip block if the id was not changed
                if (blocks[i].Index != newIndex)
                {
                    newBlocks.Add(new TableBlock(newIndex, 0));
                    oldBlocks.Add(new TableBlock(blocks[i].Index, 0));
                }
            }

            if (oldBlocks.Count > 0)
            {
                this.undoManager.Execute(new ChangedData
                    {
                        NewBlocks = newBlocks,
                        OldBlocks = oldBlocks,
                        Type = ChangedType.Move
                    });
            }
        }

        // add new block under selected one if it exists otherwise at the end
        private int GetNewBlockId() => (this.objectListView1.SelectedIndices.Count > 0 && !Helper.Settings.Data.Data.General.OnlyInsertObjectsAtIniBottom) 
                                           ? this.objectListView1.SelectedIndices[this.objectListView1.SelectedIndices.Count - 1] + 1
                                           : this.Data.Blocks.Count;

        private void mnuCreateTemplateFrom_Click(object sender, EventArgs e)
        {
            var selection = this.GetSelectedBlocks();
            if (selection.Count == 0)
                return;

            if (selection.Count > 1)
            {
                var result = MessageBox.Show(
                    "This will create a template for all the selected objects. Do you want to continue?",
                    "Are you sure?",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    return;
            }

            foreach (TableBlock block in selection)
                Helper.Settings.Data.Data.General.Templates.Templates.Add(block.Block);

            Helper.Settings.LoadTemplates();
            Helper.Settings.Save();
        }
    }
}
