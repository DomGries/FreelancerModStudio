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

        System.Windows.Forms.ToolStripDropDown MultipleAddDropDown();

        void Add(int index);
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

        string GetTitle();

        void Save();
        void SaveAs();
        void Undo();
        void Redo();

        void ChangeVisibility();
    }
}
