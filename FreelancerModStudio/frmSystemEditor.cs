using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using FreelancerModStudio.Data;
using FreelancerModStudio.SystemPresenter;
using FreelancerModStudio.SystemPresenter.Content;
using HelixEngine;
using WeifenLuo.WinFormsUI.Docking;

namespace FreelancerModStudio
{
    public partial class frmSystemEditor : DockContent
    {
        Presenter _presenter;
        Thread _universeLoadingThread;

        public Presenter.SelectionChangedType SelectionChanged;

        void OnSelectionChanged(TableBlock block, bool toggle)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(block, toggle);
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

        public Presenter.DataManipulatedType DataManipulated;

        void OnDataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            if (DataManipulated != null)
            {
                DataManipulated(newBlock, oldBlock);
            }
        }

        public frmSystemEditor()
        {
            InitializeComponent();
            InitializeView();
        }

        public void RefreshSettings()
        {
            _presenter.SetTitle();
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
            _presenter.DataManipulated += systemPresenter_DataManipulated;
        }

        void systemPresenter_SelectionChanged(TableBlock block, bool toggle)
        {
            OnSelectionChanged(block, toggle);
        }

        void systemPresenter_FileOpen(string file)
        {
            OnFileOpen(file);
        }

        void systemPresenter_DataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            OnDataManipulated(newBlock, oldBlock);
        }

        public void ShowViewer(ViewerType viewerType)
        {
            Clear(false, false);
            _presenter.ViewerType = viewerType;
        }

        public void ShowData(TableData data)
        {
            _presenter.Add(data.Blocks);
            _presenter.Viewport.ZoomExtents(0);
        }

        public void ShowUniverseConnections(string file, List<TableBlock> blocks, ArchetypeManager archetype)
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
            _presenter.TrackedContent = null;
            _presenter.ClearDisplay(clearLight);
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
                ContentBase content = _presenter.FindContent(block);
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
                ContentBase content = _presenter.FindContent(block);
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
                ContentBase content = _presenter.FindContent(block);
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
                ContentBase content = _presenter.FindContent(block);
                if (content != null)
                {
                    _presenter.Delete(content);
                }
            }
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

        public ManipulationMode ManipulationMode
        {
            get
            {
                return _presenter.ManipulationMode;
            }
            set
            {
                if (_presenter.ManipulationMode == value)
                {
                    return;
                }

                // switch manipulation mode
                _presenter.ManipulationMode = value;
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
                        zoomFactor = 1.75;
                        break;
                    default:
                        zoomFactor = 1.25;
                        break;
                }
                _presenter.LookAtAndZoom(_presenter.SelectedContent, zoomFactor, true);
            }
        }

        public void TrackSelected()
        {
            // change tracked object
            if (_presenter.SelectedContent == _presenter.TrackedContent)
            {
                _presenter.TrackedContent = null;
            }
            else
            {
                _presenter.TrackedContent = _presenter.SelectedContent;
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
    }
}
