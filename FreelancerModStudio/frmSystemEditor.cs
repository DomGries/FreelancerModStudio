using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.SystemPresenter;
using FreelancerModStudio.SystemPresenter.Content;
using HelixEngine;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmSystemEditor : DockContent, IContentForm
    {
        Presenter _presenter;
        Thread _universeLoadingThread;

        public delegate void SelectionChangedType(TableBlock block);

        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(TableBlock block)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(block);
            }
        }

        public Presenter.FileOpenType FileOpen;

        void OnFileOpen(string file)
        {
            if (FileOpen != null)
            {
                FileOpen(file);
            }
        }

        public frmSystemEditor()
        {
            InitializeComponent();
            InitializeView();
        }

        void InitializeView()
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif
            //create viewport using the Helix Engine
            HelixViewport3D viewport = new HelixViewport3D
                {
                    Background = Brushes.Black,
                    Foreground = Brushes.Yellow,
                    FontSize = 14,
                    ClipToBounds = false,
                    ShowViewCube = true
                };
#if DEBUG
            st.Stop();
            Debug.WriteLine("init HelixView: " + st.ElapsedMilliseconds + "ms");
            st.Reset();
            st.Start();
#endif
            ElementHost host = new ElementHost
                {
                    Child = viewport,
                    Dock = DockStyle.Fill
                };
            Controls.Add(host);
#if DEBUG
            st.Stop();
            Debug.WriteLine("init host: " + st.ElapsedMilliseconds + "ms");
#endif
            _presenter = new Presenter(viewport);
            _presenter.SelectionChanged += systemPresenter_SelectionChanged;
            _presenter.FileOpen += systemPresenter_FileOpen;
        }

        void systemPresenter_SelectionChanged(ContentBase content)
        {
            OnSelectionChanged(content.Block);
        }

        void systemPresenter_FileOpen(string file)
        {
            OnFileOpen(file);
        }

        public void ShowViewer(ViewerType viewerType)
        {
            Clear(false, false);
            _presenter.ViewerType = viewerType;
        }

        public void ShowData(TableData data)
        {
            _presenter.Add(data.Blocks);
        }

        public void ShowData(TableData data, string file, ArchetypeManager archetype)
        {
            _presenter.Add(data.Blocks);
            DisplayUniverse(file, data.Blocks, archetype);
        }

        void DisplayUniverse(string file, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            if (File.Exists(file))
            {
                string path = Path.GetDirectoryName(file);

                ThreadStart threadStart = () => _presenter.DisplayUniverse(path, Helper.Template.Data.SystemFile, blocks, archetype);

                Helper.Thread.Start(ref _universeLoadingThread, threadStart, ThreadPriority.Normal, true);
            }
        }

        public new void Dispose()
        {
            if (_presenter != null)
            {
                Clear(true, true);
            }

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Clear(bool clearLight, bool waitForThread)
        {
            Helper.Thread.Abort(ref _universeLoadingThread, waitForThread);
            _presenter.SelectedContent = null;
            _presenter.ClearDisplay(false);
        }

        ContentBase GetContent(TableBlock block)
        {
            for (int i = _presenter.GetContentStartId(); i < _presenter.Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)_presenter.Viewport.Children[i];
                if (content.Block != null && content.Block.Id == block.Id)
                {
                    return content;
                }
            }
            return null;
        }

        public void Select(TableBlock block)
        {
            // return if object is already selected
            if (_presenter.SelectedContent != null && _presenter.SelectedContent.Block.Id == block.Id)
            {
                return;
            }

            if (block.Visibility)
            {
                bool isModelPreview = _presenter.ViewerType == ViewerType.SolarArchetype || _presenter.ViewerType == ViewerType.ModelPreview;
                if (isModelPreview)
                {
                    _presenter.SelectedContent = null;
                    _presenter.ClearDisplay(false);
                    _presenter.Add(block);
                }

                // select object
                ContentBase content = GetContent(block);
                if (content != null)
                {
                    _presenter.SelectedContent = content;

                    if (isModelPreview)
                    {
                        // focus and zoom into object
                        _presenter.LookAtAndZoom(content, 1.25, false);
                    }
                    return;
                }
            }
            else
            {
                // show selection box for invisible objects
                ContentBase content = Presenter.CreateContent(block.ObjectType);
                if (content != null)
                {
                    SystemParser.SetValues(content, block, false);
                    _presenter.SelectedContent = content;
                    return;
                }
            }

            // deselect currect selection if nothing could be selected
            Deselect();
        }

        public void Deselect()
        {
            _presenter.SelectedContent = null;

            if (_presenter.ViewerType == ViewerType.SolarArchetype || _presenter.ViewerType == ViewerType.ModelPreview)
            {
                _presenter.ClearDisplay(false);
            }
        }

        public void SetVisibility(TableBlock block)
        {
            if (block.Visibility)
            {
                _presenter.Add(block);
            }
            else
            {
                ContentBase content = GetContent(block);
                if (content != null)
                {
                    _presenter.Delete(content);
                }
            }
        }

        public void SetValues(List<TableBlock> blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();

            foreach (TableBlock block in blocks)
            {
                ContentBase content = GetContent(block);
                if (content != null)
                {
                    // visual is visible as it was found so we need to remove it
                    if (block.Visibility)
                    {
                        _presenter.ChangeValues(content, block);
                    }
                    else
                    {
                        _presenter.Delete(content);
                    }
                }
                else
                {
                    newBlocks.Add(block);
                }
            }

            if (newBlocks.Count > 0)
            {
                Add(newBlocks);
            }
        }

        public void Add(List<TableBlock> blocks)
        {
            if (_presenter.ViewerType == ViewerType.SolarArchetype || _presenter.ViewerType == ViewerType.ModelPreview)
            {
                return;
            }

            _presenter.Add(blocks);
        }

        public void Delete(List<TableBlock> blocks)
        {
            if (_presenter.ViewerType == ViewerType.SolarArchetype || _presenter.ViewerType == ViewerType.ModelPreview)
            {
                return;
            }

            foreach (TableBlock block in blocks)
            {
                ContentBase content = GetContent(block);
                if (content != null)
                {
                    _presenter.Delete(content);
                }
            }
        }

        public bool UseDocument()
        {
            return true;
        }

        public bool IsModelMode
        {
            get
            {
                return _presenter.IsModelMode;
            }
            set
            {
                if (_presenter.IsModelMode == value)
                {
                    return;
                }

                // switch model mode
                _presenter.IsModelMode = value;
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                _presenter.ReloadModels();
#if DEBUG
                sw.Stop();
                Debug.WriteLine("loading models: " + sw.ElapsedMilliseconds + "ms");
#endif
            }
        }

        public void FocusSelected()
        {
            if (_presenter.SelectedContent != null)
            {
                double zoomFactor;
                switch (_presenter.ViewerType)
                {
                    case ViewerType.Universe:
                        zoomFactor = 20;
                        break;
                    case ViewerType.System:
                        zoomFactor = 2;
                        break;
                    default:
                        zoomFactor = 1.25;
                        break;
                }
                _presenter.LookAtAndZoom(_presenter.SelectedContent, zoomFactor, true);
            }
        }

        public string DataPath
        {
            get
            {
                return _presenter.DataPath;
            }
            set
            {
                _presenter.DataPath = value;
            }
        }

        #region ContentInterface Members

        public bool CanCopy()
        {
            return false;
        }

        public bool CanCut()
        {
            return false;
        }

        public bool CanPaste()
        {
            return false;
        }

        public bool CanAdd()
        {
            return false;
        }

        public bool CanDelete()
        {
            return false;
        }

        public bool CanSelectAll()
        {
            return false;
        }

        public ToolStripDropDown MultipleAddDropDown()
        {
            return null;
        }

        public void Add()
        {
        }

        public void Copy()
        {
        }

        public void Cut()
        {
        }

        public void Paste()
        {
        }

        public void Delete()
        {
        }

        public void SelectAll()
        {
        }

        #endregion
    }
}
