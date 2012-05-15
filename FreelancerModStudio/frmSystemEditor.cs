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

        public void ShowData(TableData data, string file, ArchetypeManager archetype)
        {
            Helper.Thread.Abort(ref _universeLoadingThread, false);

            Clear();
            _presenter.Add(data.Blocks);

            if (archetype != null)
            {
                _presenter.IsUniverse = true;
                DisplayUniverse(file, data.Blocks, archetype);
            }
            else
            {
                _presenter.IsUniverse = false;
            }
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
            base.Dispose();

            if (_presenter != null)
            {
                Clear(true);
            }
        }

        void Clear(bool clearLight)
        {
            Helper.Thread.Abort(ref _universeLoadingThread, true);

            _presenter.ClearDisplay(clearLight);
        }

        public void Clear()
        {
            Clear(false);
        }

        ContentBase GetContent(TableBlock block)
        {
            for (int i = _presenter.GetContentStartId(); i < _presenter.Viewport.Children.Count; ++i)
            {
                ContentBase content = (ContentBase)_presenter.Viewport.Children[i];
                if (content.Block.Id == block.Id)
                {
                    return content;
                }
            }
            return null;
        }

        public void Select(TableBlock block)
        {
            // return if object is already selected
            if (_presenter.SelectedContent != null && _presenter.SelectedContent.Block == block)
            {
                return;
            }

            ContentBase content = GetContent(block);
            if (content != null)
            {
                _presenter.SelectedContent = content;
            }
            else
            {
                Deselect();
            }
        }

        public void Deselect()
        {
            _presenter.SelectedContent = null;
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
                    // visual is visibile as it was found so we need to remove it
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
            _presenter.Add(blocks);
        }

        public void AddModel(Visual3D v)
        {
            _presenter.Viewport.Children.Add(v);
        }

        public void Delete(List<TableBlock> blocks)
        {
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
            set
            {
                if (_presenter.IsModelMode == value)
                {
                    return;
                }

                // switch model mode
                _presenter.IsModelMode = value;

                for (int i = _presenter.GetContentStartId(); i < _presenter.Viewport.Children.Count; ++i)
                {
                    ContentBase content = (ContentBase)_presenter.Viewport.Children[i];
                    _presenter.LoadModel(content);
                }
            }
        }

        public void FocusSelected()
        {
            if (_presenter.SelectedContent != null)
            {
                _presenter.LookAt(_presenter.SelectedContent);
            }
        }

        public string DataPath
        {
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

        public void Add(int index)
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
