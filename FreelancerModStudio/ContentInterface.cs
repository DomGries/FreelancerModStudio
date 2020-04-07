namespace FreelancerModStudio
{
    using System.Windows.Forms;

    public interface IContentForm
    {
        bool CanDelete();
    }

    public interface IDocumentForm : IContentForm
    {
        bool CanSave();
        bool CanUndo();
        bool CanRedo();

        bool ObjectSelected();
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
        string File { get; }
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
