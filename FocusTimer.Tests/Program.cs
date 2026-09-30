using FocusTimer.Models;
using FocusTimer.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;

var failures = new List<string>();
void Check(bool condition, string name)
{
    if (!condition) failures.Add(name);
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {name}");
}

var now = new DateTimeOffset(2026, 9, 30, 8, 0, 0, TimeSpan.Zero);
var settings = new TimerSettings
{
    FocusMinutes = 25,
    ShortBreakMinutes = 5,
    LongBreakMinutes = 15,
    CyclesBeforeLongBreak = 4
};
var stats = new DailyStats { PersistenceEnabled = false };
var timer = new TimerService(settings, stats, () => now);
var finishedRaised = false;
timer.Finished += _ => finishedRaised = true;

Check(timer.Phase == TimerPhase.Focus && timer.Remaining == TimeSpan.FromMinutes(25), "begint in focusfase");
timer.Start();
now = now.AddMinutes(7);
timer.Tick();
Check(timer.IsRunning && timer.Remaining == TimeSpan.FromMinutes(18), "timestamp bepaalt resterende tijd");
timer.Pause();
now = now.AddMinutes(30);
timer.Tick();
Check(!timer.IsRunning && timer.Remaining == TimeSpan.FromMinutes(18), "pauze bevriest timer");
timer.Start();
now = now.AddMinutes(18);
timer.Tick();
Check(timer.Phase == TimerPhase.ShortBreak && stats.CompletedPomodoros == 1 && finishedRaised, "focus voltooit, telt en meldt");
timer.Skip();
Check(timer.Phase == TimerPhase.Focus, "skip pauze gaat naar focus");
timer.Skip(); timer.Skip(); timer.Skip(); timer.Skip(); timer.Skip();
Check(timer.Phase == TimerPhase.LongBreak, "vierde cyclus gaat naar lange pauze");
timer.Reset();
Check(!timer.IsRunning && timer.Remaining == TimeSpan.FromMinutes(15), "reset herstelt huidige faseduur");

if (System.Windows.Application.ResourceAssembly is null)
    System.Windows.Application.ResourceAssembly = typeof(FocusTimer.App).Assembly;

var gdiBefore = 0;
var gdiAfter = 0;
using (var renderer = new TrayIconRenderer())
{
    var phases = new[] { TimerPhase.Focus, TimerPhase.ShortBreak, TimerPhase.LongBreak };
    foreach (var phase in phases)
    {
        using var icon = renderer.CreateIcon(phase, 0.75, isRunning: true);
        Check(icon.Width == 32 && icon.Height == 32, $"tray-icoon {phase} is 32×32");
    }

    using (var warmupIcon = renderer.CreateIcon(TimerPhase.Focus, 1, isRunning: false)) { }
    GC.Collect();
    GC.WaitForPendingFinalizers();
    gdiBefore = GetGuiResources(Process.GetCurrentProcess().Handle, 0);

    for (var index = 0; index < 500; index++)
    {
        using var icon = renderer.CreateIcon(phases[index % phases.Length], (index % 101) / 100d, index % 2 == 0);
    }

    GC.Collect();
    GC.WaitForPendingFinalizers();
    gdiAfter = GetGuiResources(Process.GetCurrentProcess().Handle, 0);
}
Check(gdiAfter <= gdiBefore + 4, $"tray-renderer lekt geen GDI-resources ({gdiBefore} → {gdiAfter})");

var settingsPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"FocusTimer-settings-{Guid.NewGuid():N}.json");
try
{
    settings.FocusMinutes = 42;
    settings.AutoStart = true;
    settings.Save(settingsPath);
    var loaded = TimerSettings.Load(settingsPath);
    Check(loaded.FocusMinutes == 42 && loaded.AutoStart, "instellingen opslaan en opnieuw laden");
}
finally
{
    if (System.IO.File.Exists(settingsPath)) System.IO.File.Delete(settingsPath);
}

if (failures.Count > 0)
{
    Console.Error.WriteLine($"{failures.Count} test(s) mislukt: {string.Join(", ", failures)}");
    return 1;
}

Console.WriteLine("Alle timer-tests geslaagd.");
return 0;

[DllImport("user32.dll")]
static extern int GetGuiResources(IntPtr process, int flags);
