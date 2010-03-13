using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FreelancerModStudio.SystemPresenter;
using System.Windows.Media.Media3D;
using HelixEngine;
using System.Windows.Forms.Integration;
using FreelancerModStudio.Data.IO;
using FreelancerModStudio.Data;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;

namespace FreelancerModStudio
{
    public partial class frmSystemEditor : WeifenLuo.WinFormsUI.Docking.DockContent, ContentInterface
    {
        SystemPresenter.Presenter systemPresenter = null;
        Thread universeLoadingThread = null;

        public delegate void SelectionChangedType(TableBlock block);
        public SelectionChangedType SelectionChanged;

        void OnSelectionChanged(TableBlock block)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(block);
        }

        public FreelancerModStudio.SystemPresenter.Presenter.FileOpenType FileOpen;

        void OnFileOpen(string file)
        {
            if (this.FileOpen != null)
                this.FileOpen(file);
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
            HelixView3D view = new HelixView3D();
            view.Background = System.Windows.Media.Brushes.Black;
            view.Foreground = System.Windows.Media.Brushes.White;
            view.FontSize = 16;
            view.FontWeight = System.Windows.FontWeights.Bold;
            view.ClipToBounds = false;
            view.ShowViewCube = true;
            view.Camera.NearPlaneDistance = 0.001;
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("init HelixView: " + st.ElapsedMilliseconds + "ms");
            st.Start();
#endif
            ElementHost host = new ElementHost();
            host.Child = view;
            host.Dock = DockStyle.Fill;
            this.Controls.Add(host);
#if DEBUG
            st.Stop();
            System.Diagnostics.Debug.WriteLine("init host: " + st.ElapsedMilliseconds + "ms");
#endif
            systemPresenter = new SystemPresenter.Presenter(view);
            systemPresenter.SelectionChanged += systemPresenter_SelectionChanged;
            systemPresenter.FileOpen += systemPresenter_FileOpen;
        }

        void systemPresenter_SelectionChanged(ContentBase content)
        {
            OnSelectionChanged(content.Block);
        }

        void systemPresenter_FileOpen(string file)
        {
            OnFileOpen(file);
        }

        public void ShowData(TableData data, string file, ArchtypeManager archtype)
        {
            Helper.Thread.Abort(ref universeLoadingThread, false);

            Clear();
            systemPresenter.Add(data.Blocks);

            if (archtype != null)
                DisplayUniverse(System.IO.Path.GetDirectoryName(file), archtype);
        }

        void DisplayUniverse(string path, ArchtypeManager archtype)
        {
            ThreadStart threadStart = new ThreadStart(delegate
            {
                systemPresenter.IsUniverse = true;

                int systemTemplate = Helper.Template.Data.Files.IndexOf("System");
                systemPresenter.DisplayUniverse(path, systemTemplate, archtype);
            });

            Helper.Thread.Start(ref universeLoadingThread, threadStart, ThreadPriority.Normal, true);
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
                if (systemPresenter.Objects.TryGetValue(block.ID, out content))
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

        #region ContentInterface Members

        public bool CanCopy()
        {
            throw new NotImplementedException();
        }

        public bool CanCut()
        {
            throw new NotImplementedException();
        }

        public bool CanPaste()
        {
            throw new NotImplementedException();
        }

        public bool CanAdd()
        {
            throw new NotImplementedException();
        }

        public bool CanAddMultiple()
        {
            throw new NotImplementedException();
        }

        public bool CanDelete()
        {
            throw new NotImplementedException();
        }

        public bool CanSelectAll()
        {
            throw new NotImplementedException();
        }

        public ToolStripDropDown MultipleAddDropDown()
        {
            throw new NotImplementedException();
        }

        public void Add(int index)
        {
            throw new NotImplementedException();
        }

        public void Copy()
        {
            throw new NotImplementedException();
        }

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void SelectAll()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
