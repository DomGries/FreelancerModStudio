namespace FreelancerModStudio.AutoUpdate
{
    public delegate void ActionRequired(ActionType value);

    interface IAutoUpdateUI
    {
        void ShowUI();
        void SetPage(StatusType page, bool async);
        void SetProgress(long value, long total, int percent);
        event ActionRequired ActionRequired;
    }
}
