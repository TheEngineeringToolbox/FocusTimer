using System.Text.Json;
using System.IO;
namespace FocusTimer.Models;
public sealed class TimerSettings
{
    public int FocusMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int CyclesBeforeLongBreak { get; set; } = 4;
    public bool AutoStart { get; set; }
    public bool Sound { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;

    public static string Path => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FocusTimer", "settings.json");

    public static TimerSettings Load(string? filePath = null)
    {
        filePath ??= Path;
        try
        {
            var settings = JsonSerializer.Deserialize<TimerSettings>(File.ReadAllText(filePath)) ?? new();
            settings.Validate();
            return settings;
        }
        catch (IOException) { return new(); }
        catch (UnauthorizedAccessException) { return new(); }
        catch (JsonException) { return new(); }
    }

    public void Save(string? filePath = null)
    {
        filePath ??= Path;
        Validate();
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Validate()
    {
        FocusMinutes = Math.Clamp(FocusMinutes, 1, 240);
        ShortBreakMinutes = Math.Clamp(ShortBreakMinutes, 1, 120);
        LongBreakMinutes = Math.Clamp(LongBreakMinutes, 1, 240);
        CyclesBeforeLongBreak = Math.Clamp(CyclesBeforeLongBreak, 1, 12);
    }
}
