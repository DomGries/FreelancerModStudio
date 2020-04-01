namespace PatchTime
{
    public delegate void ActionRequired(ActionType value);

    internal interface IAutoUpdateUi
    {
        void ShowUi();
        void SetPage(StatusType page, bool async);
        void SetProgress(long value, long total, int percent);
        event ActionRequired ActionRequired;
    }
}
