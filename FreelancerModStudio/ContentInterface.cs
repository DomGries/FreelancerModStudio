using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio
{
    public interface ContentInterface
    {
        bool UseDocument();

        bool CanCopy();
        bool CanCut();
        bool CanPaste();
        bool CanAdd();
        bool CanAddMultiple();
        bool CanDelete();
        bool CanSelectAll();

        System.Windows.Forms.ToolStripDropDown MultipleAddDropDown();

        void Add(int index);
        void Copy();
        void Cut();
        void Paste();
        void Delete();
        void SelectAll();
    }

    public interface DocumentInterface
    {
        bool CanSave();
        bool CanUndo();
        bool CanRedo();

        bool CanChangeVisibility();
        bool CanChangeVisibilityRightNow();
        bool CanFocusSelected();
        bool CanFocusSelectedRightNow();

        bool CanDisplay3DViewer();

        string GetTitle();

        void Save();
        void SaveAs();
        void Undo();
        void Redo();

        void ChangeVisibility();
    }
}
