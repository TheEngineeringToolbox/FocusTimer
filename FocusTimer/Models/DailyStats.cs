using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace FocusTimer.Models;

public sealed class DailyStats
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public int CompletedPomodoros { get; set; }
    public int FocusMinutes { get; set; }
    [JsonIgnore] public bool PersistenceEnabled { get; set; } = true;

    public static string Path => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FocusTimer", "daily-stats.json");

    public static DailyStats Load()
    {
        try
        {
            var value = JsonSerializer.Deserialize<DailyStats>(File.ReadAllText(Path));
            return value?.Date == DateOnly.FromDateTime(DateTime.Today) ? value : new();
        }
        catch (IOException) { return new(); }
        catch (UnauthorizedAccessException) { return new(); }
        catch (JsonException) { return new(); }
    }

    public void RecordFocus(int minutes)
    {
        CompletedPomodoros++;
        FocusMinutes += minutes;
        if (PersistenceEnabled)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
            File.WriteAllText(Path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
