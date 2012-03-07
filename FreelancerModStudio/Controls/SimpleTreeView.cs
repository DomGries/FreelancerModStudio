using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FreelancerModStudio.Controls
{
    public interface ITreeViewItem
    {
        string Path { get; set; }
    }

    public class SimpleTreeView : TreeView
    {
        public SimpleTreeView()
        {
            //this.NodeMouseClick += new TreeNodeMouseClickEventHandler(SimpleTreeView_NodeMouseClick);
            ItemDrag += SimpleTreeView_ItemDrag;
            DragEnter += SimpleTreeView_DragEnter;
            DragOver += tree_DragOver;
            DragDrop += SimpleTreeView_DragDrop;
        }
        
        void tree_DragOver(object sender, DragEventArgs e)
        {
            //string format = typeof(TreeNode).ToString();

            //// Is it a valid format?
            //if (e.Data.GetDataPresent(format, false))
            //{
            //    // Is the mouse over a valid node?
            //    TreeNode node = this.GetNodeAt(PointToClient(new Point(e.X, e.Y)));
            //    System.Diagnostics.Debug.WriteLine(this.SelectedNode);
            //    if (node != null && node != this.SelectedNode && !this.SelectedNode.Nodes.Contains(node))
            //    {
            //        e.Effect = e.AllowedEffect;
            //        SelectSingleNode(node, false);
            //        return;
            //        //tree.SelectedNode = node;
            //    }
            //}

            //e.Effect = DragDropEffects.None;
        }

        void SimpleTreeView_DragDrop(object sender, DragEventArgs e)
        {
            //TreeNode NewNode;
            //string format = typeof(TreeNode).ToString();

            //if (e.Data.GetDataPresent(format, false))
            //{
            //    Point pt = this.PointToClient(new Point(e.X, e.Y));
            //    TreeNode DestinationNode = this.GetNodeAt(pt);
            //    NewNode = (TreeNode)e.Data.GetData(format);
            //    if (DestinationNode != NewNode)
            //    {
            //        DestinationNode.Nodes.Add((TreeNode)NewNode.Clone());
            //        DestinationNode.Expand();

            //        //remove original node
            //        NewNode.Remove();
            //    }
            //}
        }

        void SimpleTreeView_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
        }

        void SimpleTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        public void SetObjects(IEnumerable collection)
        {
            //this.BeginUpdate();
            Nodes.Clear();
            AddObjects(collection);
            //this.EndUpdate();
        }

        public void AddObjects(IEnumerable collection)
        {
            foreach (ITreeViewItem item in collection)
                AddNode(Nodes, item.Path, item);
        }

        void AddNode(TreeNodeCollection nodes, string path, ITreeViewItem item)
        {
            string[] structure = path.Split(new[] { Path.DirectorySeparatorChar }, 2, StringSplitOptions.RemoveEmptyEntries);
            string root = structure[0];
            string nextPath = null;
            if (structure.Length > 1)
                nextPath = structure[1];

            if (nodes.ContainsKey(root))
                AddNode(nodes[root].Nodes, nextPath, item);
            else
            {
                ITreeViewItem value = null;
                if (nextPath == null)
                    value = item;

                TreeNode node = new TreeNode { Name = root, Text = root, Tag = value };
                if (nextPath != null)
                    AddNode(node.Nodes, nextPath, item);

                nodes.Add(node);
            }
        }

        public object GetSelectedObject()
        {
            if (SelectedNode == null)
                return null;

            return SelectedNode.Tag;
        }

        public object[] GetSelectedObjects()
        {
            List<object> objects = new List<object>();
            foreach (TreeNode node in SelectedNodes)
            {
                if (node.Tag != null)
                    objects.Add(node.Tag);
            }
            return objects.ToArray();
        }

        void RemoveNode(TreeNodeCollection nodes, string key)
        {
            int index = nodes.IndexOfKey(key);

            if (index != -1)
                nodes.RemoveAt(index);
        }

        //void SimpleTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        //{
        //    //also select node if node was clicked with right mouse
        //    if (e.Button == MouseButtons.Right)
        //        this.SelectedNode = e.Node;
        //}

        List<TreeNode> m_SelectedNodes = new List<TreeNode>();
        public List<TreeNode> SelectedNodes
        {
            get
            {
                return m_SelectedNodes;
            }
            set
            {
                ClearSelectedNodes();
                if (value != null)
                {
                    foreach (TreeNode node in value)
                        ToggleNode(node, true, false);
                }
            }
        }

        // Note we use the new keyword to Hide the native treeview's SelectedNode property.
        TreeNode m_SelectedNode;
        public new TreeNode SelectedNode
        {
            get { return m_SelectedNode; }
            set
            {
                ClearSelectedNodes();
                if (value != null)
                    SelectNode(value);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            // Make sure at least one node has a selection
            // this way we can tab to the ctrl and use the 
            // keyboard to select nodes
            if (m_SelectedNode == null && TopNode != null)
                SelectSingleNode(TopNode, true);
            else
                HighlightSelection();

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            // Handle if HideSelection property is in use.
            if (HideSelection)
                DimSelection();

            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            TreeNode node = GetNodeAt(e.Location);
            if (node != null)
            {
                int leftBound = node.Bounds.X; // - 20; // Allow user to click on image
                int rightBound = node.Bounds.Right + 10; // Give a little extra room
                if (e.Location.X > leftBound && e.Location.X < rightBound)
                {
                    if (ModifierKeys == Keys.None && (m_SelectedNodes.Contains(node)))
                    {
                        // Potential Drag Operation
                        //this.DoDragDrop(node, DragDropEffects.Move);
                        //System.Diagnostics.Debug.WriteLine("move");
                        // Let Mouse Up do select
                    }
                    else
                        SelectNode(node);
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // If the clicked on a node that WAS previously
            // selected then, reselect it now. This will clear
            // any other selected nodes. e.g. A B C D are selected
            // the user clicks on B, now A C & D are no longer selected.
            // Check to see if a node was clicked on 
            TreeNode node = GetNodeAt(e.Location);
            if (node != null)
            {
                if (ModifierKeys == Keys.None && m_SelectedNodes.Contains(node))
                {
                    int leftBound = node.Bounds.X; // -20; // Allow user to click on image
                    int rightBound = node.Bounds.Right + 10; // Give a little extra room
                    if (e.Location.X > leftBound && e.Location.X < rightBound)
                        SelectNode(node);
                }
            }

            base.OnMouseUp(e);
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            // If the user drags a node and the node being dragged is NOT
            // selected, then clear the active selection, select the
            // node being dragged and drag it. Otherwise if the node being
            // dragged is selected, drag the entire selection.
            TreeNode node = e.Item as TreeNode;
            if (node != null)
            {
                if (!m_SelectedNodes.Contains(node))
                    SelectSingleNode(node, true);
            }

            base.OnItemDrag(e);
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            e.Cancel = true;

            base.OnBeforeSelect(e);
            OnAfterSelect(new TreeViewEventArgs(e.Node));
        }

        //protected override void OnAfterSelect(TreeViewEventArgs e)
        //{
        //    base.OnAfterSelect(e);
        //}

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle all possible key strokes for the control.
            // including navigation, selection, etc.

            base.OnKeyDown(e);

            if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey)
                return;

            //this.BeginUpdate();
            bool bShift = (ModifierKeys == Keys.Shift);

            // Nothing is selected in the tree, this isn't a good state
            // select the top node
            if (m_SelectedNode == null && TopNode != null)
                ToggleNode(TopNode, true, true);

            // Nothing is still selected in the tree, this isn't a good state, leave.
            if (m_SelectedNode == null)
                return;

            if (e.KeyCode == Keys.Left)
            {
                if (m_SelectedNode.IsExpanded && m_SelectedNode.Nodes.Count > 0)
                {
                    // Collapse an expanded node that has children
                    m_SelectedNode.Collapse();
                }
                else if (m_SelectedNode.Parent != null)
                {
                    // Node is already collapsed, try to select its parent.
                    SelectSingleNode(m_SelectedNode.Parent, true);
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (!m_SelectedNode.IsExpanded)
                {
                    // Expand a collpased node's children
                    m_SelectedNode.Expand();
                }
                else
                {
                    // Node was already expanded, select the first child
                    SelectSingleNode(m_SelectedNode.FirstNode, true);
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                // Select the previous node
                if (m_SelectedNode.PrevVisibleNode != null)
                    SelectNode(m_SelectedNode.PrevVisibleNode);
            }
            else if (e.KeyCode == Keys.Down)
            {
                // Select the next node
                if (m_SelectedNode.NextVisibleNode != null)
                    SelectNode(m_SelectedNode.NextVisibleNode);
            }
            else if (e.KeyCode == Keys.Home)
            {
                if (bShift)
                {
                    if (m_SelectedNode.Parent == null)
                    {
                        // Select all of the root nodes up to this point 
                        if (Nodes.Count > 0)
                            SelectNode(Nodes[0]);
                    }
                    else
                    {
                        // Select all of the nodes up to this point under this nodes parent
                        SelectNode(m_SelectedNode.Parent.FirstNode);
                    }
                }
                else
                {
                    // Select this first node in the tree
                    if (Nodes.Count > 0)
                        SelectSingleNode(Nodes[0], true);
                }
            }
            else if (e.KeyCode == Keys.End)
            {
                if (bShift)
                {
                    if (m_SelectedNode.Parent == null)
                    {
                        // Select the last ROOT node in the tree
                        if (Nodes.Count > 0)
                            SelectNode(Nodes[Nodes.Count - 1]);
                    }
                    else
                    {
                        // Select the last node in this branch
                        SelectNode(m_SelectedNode.Parent.LastNode);
                    }
                }
                else
                {
                    if (Nodes.Count > 0)
                    {
                        // Select the last node visible node in the tree.
                        // Don't expand branches incase the tree is virtual
                        TreeNode ndLast = Nodes[0].LastNode;
                        while (ndLast.IsExpanded && (ndLast.LastNode != null))
                            ndLast = ndLast.LastNode;

                        SelectSingleNode(ndLast, true);
                    }
                }
            }
            else if (e.KeyCode == Keys.PageUp)
            {
                // Select the highest node in the display
                int nCount = VisibleCount;
                TreeNode ndCurrent = m_SelectedNode;
                while ((nCount) > 0 && (ndCurrent.PrevVisibleNode != null))
                {
                    ndCurrent = ndCurrent.PrevVisibleNode;
                    nCount--;
                }
                SelectSingleNode(ndCurrent, true);
            }
            else if (e.KeyCode == Keys.PageDown)
            {
                // Select the lowest node in the display
                int nCount = VisibleCount;
                TreeNode ndCurrent = m_SelectedNode;
                while ((nCount) > 0 && (ndCurrent.NextVisibleNode != null))
                {
                    ndCurrent = ndCurrent.NextVisibleNode;
                    nCount--;
                }
                SelectSingleNode(ndCurrent, true);
            }
            else
            {
                // Assume this is a search character a-z, A-Z, 0-9, etc.
                // Select the first node after the current node that 
                // starts with this character
                string search = ((char)e.KeyValue).ToString().ToLower();

                TreeNode ndStart = m_SelectedNode;
                TreeNode ndCurrent = m_SelectedNode.NextVisibleNode;
                bool top = false;

                while (ndCurrent != ndStart)
                {

                    if (ndCurrent == null)
                    {
                        if (!top)
                        {
                            ndCurrent = TopNode;
                            top = true;
                        }
                        else
                            break;
                    }

                    if (ndCurrent.Text.ToLower().StartsWith(search))
                    {
                        SelectSingleNode(ndCurrent, true);
                        break;
                    }

                    ndCurrent = ndCurrent.NextVisibleNode;
                }
            }
            EndUpdate();
        }

        void SelectNode(TreeNode node)
        {
            BeginUpdate();

            if (m_SelectedNode == null || ModifierKeys == Keys.Control)
            {
                // Ctrl+Click selects an unselected node, or unselects a selected node.
                bool isSelected = m_SelectedNodes.Contains(node);
                ToggleNode(node, !isSelected, isSelected);
            }
            else if (ModifierKeys == Keys.Shift)
            {
                // Shift+Click selects nodes between the selected node and here.
                TreeNode ndStart = m_SelectedNode;
                TreeNode ndEnd = node;
                SelectSingleNode(ndStart, false); 

                if (ndStart.Parent == ndEnd.Parent)
                {
                    // Selected node and clicked node have same parent, easy case.
                    if (ndStart.Index < ndEnd.Index)
                    {
                        // If the selected node is beneath the clicked node walk down
                        // selecting each Visible node until we reach the end.
                        while (ndStart != ndEnd)
                        {
                            ndStart = ndStart.NextVisibleNode;
                            if (ndStart == null)
                                break;

                            ToggleNode(ndStart, true, false);
                        }
                    }
                    else if (ndStart.Index == ndEnd.Index)
                    {
                        // Clicked same node, do nothing
                    }
                    else
                    {
                        // If the selected node is above the clicked node walk up
                        // selecting each Visible node until we reach the end.
                        while (ndStart != ndEnd)
                        {
                            ndStart = ndStart.PrevVisibleNode;
                            if (ndStart == null)
                                break;

                            ToggleNode(ndStart, true, false);
                        }
                    }
                }
                else
                {
                    // Selected node and clicked node have different parent, hard case.
                    // We need to find a common parent to determine if we need
                    // to walk down selecting, or walk up selecting.

                    TreeNode ndStartP = ndStart;
                    TreeNode ndEndP = ndEnd;
                    int startDepth = Math.Min(ndStartP.Level, ndEndP.Level);

                    // Bring lower node up to common depth
                    while (ndStartP.Level > startDepth)
                        ndStartP = ndStartP.Parent;

                    // Bring lower node up to common depth
                    while (ndEndP.Level > startDepth)
                        ndEndP = ndEndP.Parent;

                    // Walk up the tree until we find the common parent
                    while (ndStartP.Parent != ndEndP.Parent)
                    {
                        ndStartP = ndStartP.Parent;
                        ndEndP = ndEndP.Parent;
                    }

                    // Select the node
                    if (ndStartP.Index < ndEndP.Index)
                    {
                        // If the selected node is beneath the clicked node walk down
                        // selecting each Visible node until we reach the end.
                        while (ndStart != ndEnd)
                        {
                            ndStart = ndStart.NextVisibleNode;
                            if (ndStart == null)
                                break;

                            ToggleNode(ndStart, true, false);
                        }
                    }
                    else if (ndStartP.Index == ndEndP.Index)
                    {
                        if (ndStart.Level < ndEnd.Level)
                        {
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.NextVisibleNode;
                                if (ndStart == null)
                                    break;

                                ToggleNode(ndStart, true, false);
                            }
                        }
                        else
                        {
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.PrevVisibleNode;
                                if (ndStart == null)
                                    break;

                                ToggleNode(ndStart, true, false);
                            }
                        }
                    }
                    else
                    {
                        // If the selected node is above the clicked node walk up
                        // selecting each Visible node until we reach the end.
                        while (ndStart != ndEnd)
                        {
                            ndStart = ndStart.PrevVisibleNode;
                            if (ndStart == null)
                                break;

                            ToggleNode(ndStart, true, false);
                        }
                    }
                }
            }
            else
            {
                // Just clicked a node, select it
                SelectSingleNode(node, true);
            }

            //OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
            EndUpdate();
        }

        void ClearSelectedNodes()
        {
            foreach (TreeNode node in m_SelectedNodes)
            {
                node.BackColor = BackColor;
                node.ForeColor = ForeColor;
            }

            m_SelectedNodes.Clear();
            m_SelectedNode = null;
        }

        void SelectSingleNode(TreeNode node, bool change)
        {
            if (node == null)
                return;

            ClearSelectedNodes();
            ToggleNode(node, true, change);
            node.EnsureVisible();
        }

        void HighlightSelection()
        {
            foreach (TreeNode node in m_SelectedNodes)
            {
                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
        }

        void DimSelection()
        {
            foreach (TreeNode node in m_SelectedNodes)
            {
                node.BackColor = SystemColors.Control;
                node.ForeColor = ForeColor;
            }
        }

        void ToggleNode(TreeNode node, bool selectNode, bool change)
        {
            if (selectNode)
            {
                if (m_SelectedNode == null || change)
                    m_SelectedNode = node;

                if (!m_SelectedNodes.Contains(node))
                    m_SelectedNodes.Add(node);

                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                m_SelectedNodes.Remove(node);
                node.BackColor = BackColor;
                node.ForeColor = ForeColor;
            }
        }
    }
}
