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

namespace FreelancerModStudio
{
    public partial class frmSystemEditor : WeifenLuo.WinFormsUI.Docking.DockContent, ContentInterface
    {
        SystemPresenter.SystemPresenter systemPresenter = null;

        public frmSystemEditor()
        {
            InitializeComponent();

            //create viewport using the Helix Engine
            HelixView3D view = new HelixView3D();
            view.Background = System.Windows.Media.Brushes.Black;
            view.TextBrush = System.Windows.Media.Brushes.White;
            view.ShowViewCube = true;
            view.Camera.NearPlaneDistance = 0.001;

            ElementHost host = new ElementHost();
            host.Child = view;
            host.Dock = DockStyle.Fill;
            this.Controls.Add(host);

            systemPresenter = new SystemPresenter.SystemPresenter(view);
            systemPresenter.Lightning = new DefaultLightsVisual3D();
        }

        public new void Dispose()
        {
            base.Dispose();

            systemPresenter.ClearDisplay(true);
            systemPresenter.Objects.Clear();
        }

        public void ShowData(TableData data)
        {
            Clear();
            systemPresenter.Show(data);
        }

        public void Clear()
        {
            systemPresenter.ClearDisplay(false);
            systemPresenter.Objects.Clear();
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

        public void SetValues(TableBlock[] blocks)
        {
            foreach (TableBlock block in blocks)
            {
                ContentBase content;
                if (systemPresenter.Objects.TryGetValue(block.ID, out content))
                    systemPresenter.SetValues(content, block);
            }
        }

        //void PositionUpdater_Tick(object sender, EventArgs e)
        //{
        //    Vector3D position = (systemPresenter.Viewport.Camera.Position.ToVector3D() * 1000);
        //    string status = Math.Floor(position.X).ToString() + ", " + Math.Floor(position.Y).ToString() + ", " + Math.Floor(position.Z).ToString();

        //    systemPresenter.Viewport.Status = status;
        //}

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

        public bool CanAddMultiple()
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
    }
}
