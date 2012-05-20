using System.Windows.Forms;

namespace FreelancerModStudio
{
    public interface IContentForm
    {
        bool UseDocument();

        bool CanCopy();
        bool CanCut();
        bool CanPaste();
        bool CanAdd();
        bool CanDelete();
        bool CanSelectAll();

        ToolStripDropDown MultipleAddDropDown();

        void Add();
        void Copy();
        void Cut();
        void Paste();
        void Delete();
        void SelectAll();
    }

    public interface IDocumentForm
    {
        bool CanSave();
        bool CanUndo();
        bool CanRedo();

        bool CanChangeVisibility(bool rightNow);
        bool CanFocusSelected(bool rightNow);

        bool CanDisplay3DViewer();

        string Title { get; }
        string DataPath { get; }

        void Save();
        void SaveAs();
        void Undo();
        void Redo();

        void ChangeVisibility();
    }
}
