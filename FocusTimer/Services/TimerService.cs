using FocusTimer.Models;

namespace FocusTimer.Services;

public enum TimerPhase { Focus, ShortBreak, LongBreak }

public sealed class TimerService
{
    private readonly TimerSettings _settings;
    private readonly DailyStats _stats;
    private readonly Func<DateTimeOffset> _now;
    private DateTimeOffset _endTime;

    public TimerService(TimerSettings settings, DailyStats stats, Func<DateTimeOffset>? now = null)
    {
        _settings = settings;
        _stats = stats;
        _now = now ?? (() => DateTimeOffset.UtcNow);
        Remaining = CurrentDuration;
    }

    public TimerPhase Phase { get; private set; } = TimerPhase.Focus;
    public TimeSpan Remaining { get; private set; }
    public bool IsRunning { get; private set; }
    public int CyclesCompleted { get; private set; }
    public int CompletedToday => _stats.CompletedPomodoros;
    public int FocusMinutesToday => _stats.FocusMinutes;
    public string PhaseLabel => Phase switch
    {
        TimerPhase.Focus => "FOCUS",
        TimerPhase.ShortBreak => "KORTE PAUZE",
        _ => "LANGE PAUZE"
    };

    public event Action? Changed;
    public event Action<TimerPhase>? Finished;

    private TimeSpan CurrentDuration => TimeSpan.FromMinutes(Phase switch
    {
        TimerPhase.Focus => _settings.FocusMinutes,
        TimerPhase.ShortBreak => _settings.ShortBreakMinutes,
        _ => _settings.LongBreakMinutes
    });

    public void Start()
    {
        if (IsRunning) return;
        _endTime = _now() + Remaining;
        IsRunning = true;
        Changed?.Invoke();
    }

    public void Pause()
    {
        if (!IsRunning) return;
        UpdateRemaining();
        IsRunning = false;
        Changed?.Invoke();
    }

    public void Reset()
    {
        IsRunning = false;
        Remaining = CurrentDuration;
        Changed?.Invoke();
    }

    public void Skip()
    {
        IsRunning = false;
        Advance(completedNaturally: false);
        Changed?.Invoke();
    }

    public void Tick()
    {
        if (!IsRunning) return;
        UpdateRemaining();
        if (Remaining > TimeSpan.Zero)
        {
            Changed?.Invoke();
            return;
        }

        var completedPhase = Phase;
        IsRunning = false;
        Advance(completedNaturally: true);
        Finished?.Invoke(completedPhase);
        if (_settings.AutoStart) Start();
        Changed?.Invoke();
    }

    public void ApplySettings()
    {
        _settings.Validate();
        if (!IsRunning) Remaining = CurrentDuration;
        Changed?.Invoke();
    }

    private void UpdateRemaining()
    {
        Remaining = _endTime - _now();
        if (Remaining < TimeSpan.Zero) Remaining = TimeSpan.Zero;
    }

    private void Advance(bool completedNaturally)
    {
        if (Phase == TimerPhase.Focus)
        {
            CyclesCompleted++;
            if (completedNaturally) _stats.RecordFocus(_settings.FocusMinutes);
            Phase = CyclesCompleted % _settings.CyclesBeforeLongBreak == 0
                ? TimerPhase.LongBreak
                : TimerPhase.ShortBreak;
        }
        else
        {
            Phase = TimerPhase.Focus;
        }

        Remaining = CurrentDuration;
    }
}
