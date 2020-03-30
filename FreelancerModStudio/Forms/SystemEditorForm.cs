namespace FreelancerModStudio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;
    using System.Windows.Media;

    using FreelancerModStudio.Data;
    using FreelancerModStudio.SystemDesigner;
    using FreelancerModStudio.SystemDesigner.Content;

    using HelixEngine;

    using WeifenLuo.WinFormsUI.Docking;

    public partial class SystemEditorForm : DockContent
    {
        private Presenter presenter;

        private Thread universeLoadingThread;

        public Presenter.SelectionChangedType SelectionChanged;

        private void OnSelectionChanged(TableBlock block, bool toggle)
        {
            this.SelectionChanged?.Invoke(block, toggle);
        }

        public Presenter.FileOpenType FileOpen;

        private void OnFileOpen(string file)
        {
            this.FileOpen?.Invoke(file);
        }

        public Presenter.DataManipulatedType DataManipulated;

        private void OnDataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            this.DataManipulated?.Invoke(newBlock, oldBlock);
        }

        public SystemEditorForm()
        {
            this.InitializeComponent();
            this.InitializeView();
        }

        public void RefreshSettings()
        {
            this.presenter.SetTitle();
        }

        private void InitializeView()
        {
#if DEBUG
            Stopwatch st = new Stopwatch();
            st.Start();
#endif

            // create viewport using the Helix Engine
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
            ElementHost host = new ElementHost { Child = viewport, Dock = DockStyle.Fill };
            this.Controls.Add(host);
#if DEBUG
            st.Stop();
            Debug.WriteLine("init host: " + st.ElapsedMilliseconds + "ms");
#endif
            this.presenter = new Presenter(viewport);
            this.presenter.SelectionChanged += this.SystemPresenterSelectionChanged;
            this.presenter.FileOpen += this.SystemPresenterFileOpen;
            this.presenter.DataManipulated += this.SystemPresenterDataManipulated;
        }

        private void SystemPresenterSelectionChanged(TableBlock block, bool toggle)
        {
            this.OnSelectionChanged(block, toggle);
        }

        private void SystemPresenterFileOpen(string file)
        {
            this.OnFileOpen(file);
        }

        private void SystemPresenterDataManipulated(TableBlock newBlock, TableBlock oldBlock)
        {
            this.OnDataManipulated(newBlock, oldBlock);
        }

        public void ShowViewer(ViewerType viewerType)
        {
            this.Clear(false, false);
            this.presenter.ViewerType = viewerType;
        }

        public void ShowData(TableData data)
        {
            this.presenter.Add(data.Blocks);
            this.presenter.Viewport.ZoomExtents(0);
        }

        public void ShowUniverseConnections(string file, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            if (File.Exists(file))
            {
                string path = Path.GetDirectoryName(file);

                ThreadStart threadStart = () => this.presenter.DisplayUniverse(path, Helper.Template.Data.SystemFile, blocks, archetype);

                Helper.Thread.Start(ref this.universeLoadingThread, threadStart, ThreadPriority.Normal, true);
            }
        }

        public new void Dispose()
        {
            if (this.presenter != null)
            {
                this.Clear(true, true);
            }

            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Clear(bool clearLight, bool waitForThread)
        {
            Helper.Thread.Abort(ref this.universeLoadingThread, waitForThread);
            this.presenter.SelectedContent = null;
            this.presenter.TrackedContent = null;
            this.presenter.ClearDisplay(clearLight);
        }

        public void Select(TableBlock block)
        {
            // return if object is already selected
            if (this.presenter.SelectedContent != null && this.presenter.SelectedContent.Block.Id == block.Id)
            {
                return;
            }

            if (block.Visibility)
            {
                bool isModelPreview = this.presenter.ViewerType == ViewerType.SolarArchetype || this.presenter.ViewerType == ViewerType.ModelPreview;
                if (isModelPreview)
                {
                    this.presenter.SelectedContent = null;
                    this.presenter.ClearDisplay(false);
                    this.presenter.Add(block);
                }

                // select object
                ContentBase content = this.presenter.FindContent(block);
                if (content != null)
                {
                    this.presenter.SelectedContent = content;

                    if (isModelPreview)
                    {
                        // focus and zoom into object
                        this.presenter.LookAtAndZoom(content, 1.25, false);
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
                    this.presenter.SelectedContent = content;
                    return;
                }
            }

            // deselect currect selection if nothing could be selected
            this.Deselect();
        }

        public void Deselect()
        {
            this.presenter.SelectedContent = null;

            if (this.presenter.ViewerType == ViewerType.SolarArchetype || this.presenter.ViewerType == ViewerType.ModelPreview)
            {
                this.presenter.ClearDisplay(false);
            }
        }

        public void SetVisibility(TableBlock block)
        {
            if (block.Visibility)
            {
                this.presenter.Add(block);
            }
            else
            {
                ContentBase content = this.presenter.FindContent(block);
                if (content != null)
                {
                    this.presenter.Delete(content);
                }
            }
        }

        public void SetValues(List<TableBlock> blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();

            foreach (TableBlock block in blocks)
            {
                ContentBase content = this.presenter.FindContent(block);
                if (content != null)
                {
                    // visual is visible as it was found so we need to remove it
                    if (block.Visibility)
                    {
                        this.presenter.ChangeValues(content, block);
                    }
                    else
                    {
                        this.presenter.Delete(content);
                    }
                }
                else
                {
                    newBlocks.Add(block);
                }
            }

            if (newBlocks.Count > 0)
            {
                this.Add(newBlocks);
            }
        }

        public void Add(List<TableBlock> blocks)
        {
            if (this.presenter.ViewerType == ViewerType.SolarArchetype || this.presenter.ViewerType == ViewerType.ModelPreview)
            {
                return;
            }

            this.presenter.Add(blocks);
        }

        public void Delete(List<TableBlock> blocks)
        {
            if (this.presenter.ViewerType == ViewerType.SolarArchetype || this.presenter.ViewerType == ViewerType.ModelPreview)
            {
                return;
            }

            foreach (TableBlock block in blocks)
            {
                ContentBase content = this.presenter.FindContent(block);
                if (content != null)
                {
                    this.presenter.Delete(content);
                }
            }
        }

        public bool IsModelMode
        {
            get
            {
                return this.presenter.IsModelMode;
            }

            set
            {
                if (this.presenter.IsModelMode == value)
                {
                    return;
                }

                // switch model mode
                this.presenter.IsModelMode = value;
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                this.presenter.ReloadModels();
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
                return this.presenter.ManipulationMode;
            }

            set
            {
                if (this.presenter.ManipulationMode == value)
                {
                    return;
                }

                // switch manipulation mode
                this.presenter.ManipulationMode = value;
            }
        }

        public void FocusSelected()
        {
            this.presenter.FocusSelected();
        }

        public void LookAtSelected()
        {
            this.presenter.LookAtSelected();
        }

        public void TrackSelected()
        {
            this.presenter.TrackSelected();
        }

        public string DataPath
        {
            get
            {
                return this.presenter.DataPath;
            }

            set
            {
                this.presenter.DataPath = value;
            }
        }
    }
}
