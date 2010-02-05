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
    public partial class frmSystemEditor : WeifenLuo.WinFormsUI.Docking.DockContent, ContentInterface, DocumentInterface
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
            systemPresenter.Show(data);
        }

        public void Clear()
        {
            systemPresenter.ClearDisplay(false);
            systemPresenter.Objects.Clear();
        }

        public void Select(EditorINIBlock block)
        {
            foreach (ContentBase content in systemPresenter.Objects)
            {
                if (content.Block == block)
                    systemPresenter.SelectedContent = content;
            }
        }

        public void SetVisibility(EditorINIBlock[] blocks, bool value)
        {
            foreach (EditorINIBlock block in blocks)
            {
                foreach (ContentBase content in systemPresenter.Objects)
                {
                    if (content.Block == block)
                        systemPresenter.SetVisibility(content, value);
                }
            }
        }

        //void PositionUpdater_Tick(object sender, EventArgs e)
        //{
        //    Vector3D position = (systemPresenter.Viewport.Camera.Position.ToVector3D() * 1000);
        //    string status = Math.Floor(position.X).ToString() + ", " + Math.Floor(position.Y).ToString() + ", " + Math.Floor(position.Z).ToString();

        //    systemPresenter.Viewport.Status = status;
        //}

        public bool CanSave()
        {
            return false;
        }

        public bool CanUndo()
        {
            return false;
        }

        public bool CanRedo()
        {
            return false;
        }

        public string GetTitle()
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void SaveAs()
        {
            throw new NotImplementedException();
        }

        public void Undo()
        {
            throw new NotImplementedException();
        }

        public void Redo()
        {
            throw new NotImplementedException();
        }

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
