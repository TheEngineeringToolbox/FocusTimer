using System.Windows;
using FocusTimer.Models;
using FocusTimer.Services;

namespace FocusTimer.Views;

public partial class SettingsWindow : Window
{
    private readonly TimerSettings _settings;

    public SettingsWindow(TimerSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        FocusBox.Text = settings.FocusMinutes.ToString();
        ShortBox.Text = settings.ShortBreakMinutes.ToString();
        LongBox.Text = settings.LongBreakMinutes.ToString();
        CyclesBox.Text = settings.CyclesBeforeLongBreak.ToString();
        AutoStartBox.IsChecked = settings.AutoStart;
        SoundBox.IsChecked = settings.Sound;
        StartupBox.IsChecked = settings.StartWithWindows;
        MinimizeBox.IsChecked = settings.MinimizeToTray;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(FocusBox.Text, out var focus) ||
            !int.TryParse(ShortBox.Text, out var shortBreak) ||
            !int.TryParse(LongBox.Text, out var longBreak) ||
            !int.TryParse(CyclesBox.Text, out var cycles))
        {
            System.Windows.MessageBox.Show(this, "Vul voor alle tijden en aantallen een heel getal in.", "Ongeldige invoer");
            return;
        }

        _settings.FocusMinutes = focus;
        _settings.ShortBreakMinutes = shortBreak;
        _settings.LongBreakMinutes = longBreak;
        _settings.CyclesBeforeLongBreak = cycles;
        _settings.AutoStart = AutoStartBox.IsChecked == true;
        _settings.Sound = SoundBox.IsChecked == true;
        _settings.StartWithWindows = StartupBox.IsChecked == true;
        _settings.MinimizeToTray = MinimizeBox.IsChecked == true;
        _settings.Save();
        StartupService.Apply(_settings.StartWithWindows);
        DialogResult = true;
    }

    private void StartupBox_Checked(object sender, RoutedEventArgs e)
    {

    }
}
