using Forms = System.Windows.Forms;
namespace FocusTimer.Services;
public sealed class NotificationService
{
    private readonly Forms.NotifyIcon _icon;
    public NotificationService(Forms.NotifyIcon icon) => _icon = icon;

    public void ShowPhaseFinished(TimerPhase phase)
    {
        _icon.BalloonTipIcon = Forms.ToolTipIcon.Info;
        _icon.BalloonTipTitle = phase == TimerPhase.Focus ? "Pomodoro voltooid" : "Pauze voorbij";
        _icon.BalloonTipText = phase == TimerPhase.Focus
            ? "Goed gewerkt. Tijd voor een pauze."
            : "Tijd om weer te beginnen met focussen.";
        _icon.ShowBalloonTip(5000);
    }
}
