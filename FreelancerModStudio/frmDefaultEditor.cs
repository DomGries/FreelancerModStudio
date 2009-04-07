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
        private Settings.TemplateINIData mData;
        private Timer selectDataTimer = new Timer();

        public frmDefaultEditor(Settings.TemplateINIData data)
        {
            InitializeComponent();

            this.mData = data;
            selectDataTimer.Interval = 1;
            selectDataTimer.Tick += new EventHandler(selectDataTimer_Tick);
        }

        public void ShowData()
        {
            objectListView1.Clear();
            objectListView1.ShowGroups = true;

            BrightIdeasSoftware.OLVColumn col = new BrightIdeasSoftware.OLVColumn("Name", "Name");
            col.FillsFreeSpace = true;
            objectListView1.Columns.Add(col);

            col = new BrightIdeasSoftware.OLVColumn("Type", "Type");
            col.Width = 150;
            objectListView1.Columns.Add(col);

            List<string> uniqueGroups = new List<string>();
            uniqueGroups.Add(Properties.Strings.INIDefaultGroup);
            objectListView1.Groups.Add(Properties.Strings.INIDefaultGroup, Properties.Strings.INIDefaultGroup);

            objectListView1.BeginUpdate();
            for (int i = 0; i < mData.Blocks.Count; i++)
            {
                if (mData.Blocks[i].Options.Count > 0)
                {
                    string blockName = mData.Blocks[i].Name;
                    string groupName = null;
                    if (Helper.Template.Data.Data.Files[mData.TemplateIndex].Blocks[mData.Blocks[i].TemplateIndex].Multiple)
                        groupName = blockName;
                    else
                        groupName = Properties.Strings.INIDefaultGroup;

                    if (!uniqueGroups.Contains(groupName))
                    {
                        uniqueGroups.Add(groupName);
                        objectListView1.Groups.Add(groupName, groupName);
                    }

                    BrightIdeasSoftware.OLVListItem item = new BrightIdeasSoftware.OLVListItem(null);
                    item.Tag = i;
                    item.Group = objectListView1.Groups[groupName];
                    item.SubItems.Add(blockName);

                    if (mData.Blocks[i].MainOptionIndex > -1 && mData.Blocks[i].Options.Count > mData.Blocks[i].MainOptionIndex + 1)
                        item.Text = mData.Blocks[i].Options[mData.Blocks[i].MainOptionIndex].Value.ToString();
                    else
                    {
                        if (Helper.Template.Data.Data.Files[mData.TemplateIndex].Blocks[mData.Blocks[i].TemplateIndex].Multiple)
                            item.Text = blockName + i.ToString();
                        else
                            item.Text = blockName;
                    }

                    objectListView1.Items.Add(item);
                }
            }
            objectListView1.EndUpdate();
        }

        public void ShowData(string filter)
        {
            //GENERAL  todo
            objectListView1.Clear();
            objectListView1.ShowGroups = false;

            List<string> uniqueColumns = new List<string>();

            for (int i = 0; i < mData.Blocks.Count; i++)
            {
                if (mData.Blocks[i].Name == filter && mData.Blocks[i].Options.Count > 0)
                {
                    ListViewItem item = new ListViewItem();
                    for (int j = 0; j < mData.Blocks[i].Options.Count; j++)
                    {
                        if (!uniqueColumns.Contains(mData.Blocks[i].Options[j].Name))
                        {
                            uniqueColumns.Add(mData.Blocks[i].Options[j].Name);

                            BrightIdeasSoftware.OLVColumn col = new BrightIdeasSoftware.OLVColumn(mData.Blocks[i].Options[j].Name, mData.Blocks[i].Options[j].Name);
                            col.FillsFreeSpace = true;
                            objectListView1.Columns.Add(col);
                        }

                        if (j == 0)
                            item.Text = mData.Blocks[i].Options[j].Value.ToString();
                        else
                            item.SubItems.Add(mData.Blocks[i].Options[j].Value.ToString());

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

            return mData.Blocks[((int)objectListView1.SelectedItem.Tag)];
        }

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectDataTimer.Start();
        }

        private void selectDataTimer_Tick(object sender, EventArgs e)
        {
            selectDataTimer.Stop();
        }
    }
}