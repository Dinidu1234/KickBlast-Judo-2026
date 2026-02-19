namespace KickBlastStudentUI.Views;

public interface IStatusAware
{
    void SetStatusAction(Action<string> statusAction);
}
