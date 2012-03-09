using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Media3D;
using FreelancerModStudio.Data;
using FreelancerModStudio.SystemPresenter;
using HelixEngine;

namespace FreelancerModStudio
{
    public partial class frmSystemEditor : WeifenLuo.WinFormsUI.Docking.DockContent, IContentForm
    {
        Presenter systemPresenter;
        Thread universeLoadingThread;

        public delegate void SelectionChangedType(int id);
        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(int id)
        {
            if (SelectionChanged != null)
                SelectionChanged(id);
        }

        public Presenter.FileOpenType FileOpen;

        void OnFileOpen(string file)
        {
            if (FileOpen != null)
                FileOpen(file);
        }

        public frmSystemEditor()
        {
            InitializeComponent();
            InitializeView();
        }

        void InitializeView()
        {
#if DEBUG
            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            st.Start();
#endif
            //create viewport using the Helix Engine
            var view = new HelixView3D
            {
                Background = System.Windows.Media.Brushes.Black,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                FontWeight = System.Windows.FontWeights.Bold,
                ClipToBounds = false
            };
            view.Camera.NearPlaneDistance = 0.001;
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("init HelixView: " + st.ElapsedMilliseconds + "ms");
            st.Start();
#endif
            ElementHost host = new ElementHost { Child = view, Dock = DockStyle.Fill };
            Controls.Add(host);
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("init host: " + st.ElapsedMilliseconds + "ms");
#endif
            systemPresenter = new Presenter(view);
            systemPresenter.SelectionChanged += systemPresenter_SelectionChanged;
            systemPresenter.FileOpen += systemPresenter_FileOpen;
        }

        void systemPresenter_SelectionChanged(ContentBase content)
        {
            OnSelectionChanged(content.ID);
        }

        void systemPresenter_FileOpen(string file)
        {
            OnFileOpen(file);
        }

        public void ShowData(TableData data, string file, ArchetypeManager archetype)
        {
            Helper.Thread.Abort(ref universeLoadingThread, false);

            Clear();
            systemPresenter.Add(data.Blocks);

            if (archetype != null)
                DisplayUniverse(file, data.Blocks, archetype);
            else
                systemPresenter.IsUniverse = false;
        }

        void DisplayUniverse(string file, List<TableBlock> blocks, ArchetypeManager archetype)
        {
            systemPresenter.IsUniverse = true;
            if (System.IO.File.Exists(file))
            {
                string path = System.IO.Path.GetDirectoryName(file);

                ThreadStart threadStart = delegate
                                              {
                                                  systemPresenter.DisplayUniverse(path, Helper.Template.Data.SystemFile, blocks, archetype);
                                              };

                Helper.Thread.Start(ref universeLoadingThread, threadStart, ThreadPriority.Normal, true);
            }
        }

        public new void Dispose()
        {
            base.Dispose();

            if (systemPresenter != null)
                Clear(true);
        }

        void Clear(bool clearLight)
        {
            Helper.Thread.Abort(ref universeLoadingThread, true);

            systemPresenter.ClearDisplay(clearLight);
            systemPresenter.Objects.Clear();
        }

        public void Clear()
        {
            Clear(false);
        }

        public void Select(int id)
        {
            ContentBase content;
            if (systemPresenter.Objects.TryGetValue(id, out content))
                systemPresenter.SelectedContent = content;
            else
                Deselect();
        }

        public void Deselect()
        {
            systemPresenter.SelectedContent = null;
        }

        public void SetVisibility(int id, bool value)
        {
            ContentBase content;
            if (systemPresenter.Objects.TryGetValue(id, out content))
                systemPresenter.SetVisibility(content, value);
        }

        public void SetValues(List<TableBlock> blocks)
        {
            List<TableBlock> newBlocks = new List<TableBlock>();

            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (systemPresenter.Objects.TryGetValue(block.UniqueID, out content))
                    systemPresenter.ChangeValues(content, block);
                else
                    newBlocks.Add(block);
            }

            if (newBlocks.Count > 0)
                Add(newBlocks);
        }

        public void Add(List<TableBlock> blocks)
        {
            systemPresenter.Add(blocks);
        }

        public void AddModel(Visual3D v)
        {
            systemPresenter.Viewport.Children.Add(v);
        }

        public void Delete(List<TableBlock> blocks)
        {
            systemPresenter.Delete(blocks);
        }

        //void PositionUpdater_Tick(object sender, EventArgs e)
        //{
        //    Vector3D position = (systemPresenter.Viewport.Camera.Position.ToVector3D() * 1000);
        //    string status = Math.Floor(position.X).ToString() + ", " + Math.Floor(position.Y).ToString() + ", " + Math.Floor(position.Z).ToString();

        //    systemPresenter.Viewport.Status = status;
        //}

        public bool UseDocument()
        {
            return true;
        }

        public void FocusSelected()
        {
            if (systemPresenter.SelectedContent != null)
                systemPresenter.LookAt(systemPresenter.SelectedContent);
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
