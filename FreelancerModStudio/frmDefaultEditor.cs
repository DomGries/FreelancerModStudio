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
        private List<Settings.INIBlock> mData;

        public frmDefaultEditor()
        {
            InitializeComponent();
        }

        public void ShowData(List<Settings.INIBlock> data)
        {
            mData = data;

            objectListView1.Clear();
            objectListView1.Columns.Add((ColumnHeader)new OLVColumn("Name", "1"));
            //objectListView1.Columns.Add((ColumnHeader)new OLVColumn("Group", "1"));
            objectListView1.Columns.Add((ColumnHeader)new OLVColumn("Type", "1"));

            List<string> uniqueGroups = new List<string>();

            for (int i = 0; i < data.Count; i++)
            {
                if (!uniqueGroups.Contains(data[i].Name))
                {
                    uniqueGroups.Add(data[i].Name);
                    objectListView1.Groups.Add(data[i].Name, data[i].Name);
                }

                OLVListItem item = new OLVListItem(null, data[i].Values[0].Value, null);
                //item.SubItems.Add("Default");
                item.SubItems.Add(data[i].Name);
                item.Tag = i;
                item.Group = objectListView1.Groups[data[i].Name];
                objectListView1.Items.Add((ListViewItem)item);
            }
        }

        public Settings.INIBlock GetSelectedData()
        {
            if (objectListView1.SelectedItem == null)
                return null;

            return mData[((int)objectListView1.SelectedItem.Tag)];
        }
    }
}