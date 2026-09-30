using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using FocusTimer.Models;
using FocusTimer.Services;
using FocusTimer.Views;
using Forms = System.Windows.Forms;

namespace FocusTimer;

public partial class MainWindow : Window
{
    private readonly TimerSettings _settings;
    private readonly TimerService _timer;
    private readonly Forms.NotifyIcon _tray;
    private readonly Forms.ContextMenuStrip _trayMenu;
    private readonly NotificationService _notifications;
    private readonly TrayIconRenderer _trayIconRenderer;
    private readonly System.Windows.Threading.DispatcherTimer _clock;
    private Icon _currentTrayIcon;
    private int _lastTraySecond = int.MinValue;
    private TimerPhase? _lastTrayPhase;
    private bool? _lastTrayRunning;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        _settings = TimerSettings.Load();
        _timer = new TimerService(_settings, DailyStats.Load());

        _trayIconRenderer = new TrayIconRenderer();
        _currentTrayIcon = _trayIconRenderer.CreateIcon(TimerPhase.Focus, 1, isRunning: false);
        _trayMenu = new Forms.ContextMenuStrip();
        _tray = new Forms.NotifyIcon
        {
            Icon = _currentTrayIcon,
            Visible = true,
            Text = "FOCUS – 25:00 resterend",
            ContextMenuStrip = _trayMenu
        };
        _notifications = new NotificationService(_tray);
        BuildTrayMenu();
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(ShowWindow);

        _timer.Changed += Refresh;
        _timer.Finished += OnPhaseFinished;
        _clock = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _clock.Tick += (_, _) => _timer.Tick();
        _clock.Start();
        Refresh();
    }

    private void BuildTrayMenu()
    {
        _trayMenu.Items.Add("Start / Pauze", null, (_, _) => Dispatcher.Invoke(Toggle));
        _trayMenu.Items.Add("Volgende fase", null, (_, _) => Dispatcher.Invoke(_timer.Skip));
        _trayMenu.Items.Add("Open timer", null, (_, _) => Dispatcher.Invoke(ShowWindow));
        _trayMenu.Items.Add("Instellingen", null, (_, _) => Dispatcher.Invoke(OpenSettings));
        _trayMenu.Items.Add(new Forms.ToolStripSeparator());
        _trayMenu.Items.Add("Afsluiten", null, (_, _) => Dispatcher.Invoke(CloseApp));
    }

    private void Refresh()
    {
        var remaining = _timer.Remaining < TimeSpan.Zero ? TimeSpan.Zero : _timer.Remaining;
        TimeText.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
        PhaseText.Text = _timer.PhaseLabel;
        ProgressText.Text = $"{_timer.CyclesCompleted % _settings.CyclesBeforeLongBreak} / {_settings.CyclesBeforeLongBreak}";
        TodayText.Text = $"Vandaag: {_timer.CompletedToday} Pomodoro's / {_timer.FocusMinutesToday} minuten focus";
        StartButton.Content = _timer.IsRunning ? "Pauze" : "Start";
        _tray.Text = $"{_timer.PhaseLabel} – {TimeText.Text} resterend";

        var durationMinutes = _timer.Phase switch
        {
            TimerPhase.Focus => _settings.FocusMinutes,
            TimerPhase.ShortBreak => _settings.ShortBreakMinutes,
            _ => _settings.LongBreakMinutes
        };
        var durationSeconds = TimeSpan.FromMinutes(durationMinutes).TotalSeconds;
        var remainingProgress = durationSeconds <= 0 ? 0 : remaining.TotalSeconds / durationSeconds;
        TaskbarProgress.ProgressState = _timer.IsRunning ? TaskbarItemProgressState.Normal : TaskbarItemProgressState.Paused;
        TaskbarProgress.ProgressValue = Math.Clamp(1 - remainingProgress, 0, 1);
        UpdateTrayIcon((int)Math.Ceiling(remaining.TotalSeconds), remainingProgress);
    }

    private void UpdateTrayIcon(int remainingSecond, double remainingProgress)
    {
        if (_lastTraySecond == remainingSecond &&
            _lastTrayPhase == _timer.Phase &&
            _lastTrayRunning == _timer.IsRunning)
            return;

        var nextIcon = _trayIconRenderer.CreateIcon(_timer.Phase, remainingProgress, _timer.IsRunning);
        var previousIcon = _currentTrayIcon;
        _currentTrayIcon = nextIcon;
        _tray.Icon = nextIcon;
        previousIcon.Dispose();

        _lastTraySecond = remainingSecond;
        _lastTrayPhase = _timer.Phase;
        _lastTrayRunning = _timer.IsRunning;
    }

    private void OnPhaseFinished(TimerPhase phase)
    {
        if (_settings.Sound) System.Media.SystemSounds.Asterisk.Play();
        _notifications.ShowPhaseFinished(phase);
    }

    private void Toggle()
    {
        if (_timer.IsRunning) _timer.Pause(); else _timer.Start();
    }

    private void OpenSettings()
    {
        ShowWindow();
        var dialog = new SettingsWindow(_settings) { Owner = this };
        if (dialog.ShowDialog() == true) _timer.ApplySettings();
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Focus();
    }

    private void CloseApp()
    {
        _isExiting = true;
        Close();
    }

    private void Start_Click(object sender, RoutedEventArgs e) => Toggle();
    private void Reset_Click(object sender, RoutedEventArgs e) => _timer.Reset();
    private void Skip_Click(object sender, RoutedEventArgs e) => _timer.Skip();
    private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettings();

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExiting && _settings.MinimizeToTray)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        _clock.Stop();
        _timer.Changed -= Refresh;
        _timer.Finished -= OnPhaseFinished;
        _tray.Visible = false;
        _tray.Dispose();
        _currentTrayIcon.Dispose();
        _trayIconRenderer.Dispose();
        _trayMenu.Dispose();
        base.OnClosing(e);
        System.Windows.Application.Current.Shutdown();
    }

    private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Space) Toggle();
        else if (e.Key == Key.R) _timer.Reset();
        else if (e.Key == Key.N) _timer.Skip();
    }
}
