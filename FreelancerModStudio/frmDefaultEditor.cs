using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public partial class frmDefaultEditor : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        private List<Settings.TemplateINIBlock> mData;

        public frmDefaultEditor(List<Settings.TemplateINIBlock> data)
        {
            InitializeComponent();
            this.mData = data;
        }

        public void ShowData()
        {
            objectListView1.Clear();
            objectListView1.ShowGroups = true;

            objectListView1.Columns.Add((ColumnHeader)new OLVColumn("Name", "1"));
            objectListView1.Columns.Add((ColumnHeader)new OLVColumn("Type", "1"));

            List<string> uniqueGroups = new List<string>();

            for (int i = 0; i < mData.Count; i++)
            {
                if (mData[i].Values.Count > 0)
                {
                    if (!uniqueGroups.Contains(mData[i].Name))
                    {
                        uniqueGroups.Add(mData[i].Name);
                        objectListView1.Groups.Add(mData[i].Name, mData[i].Name);
                    }

                    OLVListItem item = new OLVListItem(null, mData[i].Values[0].Value.ToString(), null);
                    item.SubItems.Add(mData[i].Name);
                    item.Tag = i;
                    item.Group = objectListView1.Groups[mData[i].Name];
                    objectListView1.Items.Add((ListViewItem)item);
                }
            }
        }

        public void ShowData(string filter)
        {
            objectListView1.Clear();
            objectListView1.ShowGroups = false;

            List<string> uniqueColumns = new List<string>();

            for (int i = 0; i < mData.Count; i++)
            {
                if (mData[i].Name == filter && mData[i].Values.Count > 0)
                {
                    OLVListItem item = null;
                    for (int j = 0; j < mData[i].Values.Count; j++)
                    {
                        if (!uniqueColumns.Contains(mData[i].Values[j].Name))
                        {
                            uniqueColumns.Add(mData[i].Values[j].Name);
                            objectListView1.Columns.Add((ColumnHeader)new OLVColumn(mData[i].Values[j].Name, "1"));
                        }

                        if (j == 0)
                            item = new OLVListItem(null, mData[i].Values[j].Value.ToString(), null);
                        else
                            item.SubItems.Add(mData[i].Values[j].Value.ToString());

                    }

                    if (item != null)
                    {
                        item.Tag = i;
                        objectListView1.Items.Add((ListViewItem)item);
                    }
                }
            }
        }

        public Settings.TemplateINIBlock GetSelectedData()
        {
            if (objectListView1.SelectedItem == null)
                return null;

            return mData[((int)objectListView1.SelectedItem.Tag)];
        }
    }
}