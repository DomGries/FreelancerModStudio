using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio
{
    public interface ContentInterface
    {
        bool CanSave();
        bool CanAdd();
        bool CanAddMultiple();
        bool CanDelete();
        bool CanSelectAll();

        System.Windows.Forms.ToolStripDropDown MultipleAddDropDown();
        string GetTitle();

        void Add(int index);
        void Delete();
        void SelectAll();
    }
}
