using System.Windows.Forms;

namespace FreelancerModStudio
{
    public interface IContentForm
    {
        bool CanDelete();
    }

    public interface IDocumentForm : IContentForm
    {
        bool CanSave();
        bool CanUndo();
        bool CanRedo();

        bool CanCopy();
        bool CanCut();
        bool CanPaste();
        bool CanAdd();
        bool CanSelectAll();

        bool CanChangeVisibility(bool rightNow);
        bool CanFocusSelected(bool rightNow);
        bool CanTrackSelected(bool rightNow);

        bool CanDisplay3DViewer();
        bool CanManipulatePosition();
        bool CanManipulateRotationScale();

        ToolStripDropDown MultipleAddDropDown();

        string GetTitle();
        string DataPath { get; }

        void Save();
        void SaveAs();
        void Undo();
        void Redo();

        void Copy();
        void Cut();
        void Paste();
        void Delete();
        void SelectAll();

        void ChangeVisibility();
    }
}
