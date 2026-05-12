using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeathCounter;

internal sealed class AppSettings
{
    private static readonly string AppRootDirectory = ResolveAppRootDirectory();
    private static readonly string PortableCounterDirectory = Path.Combine(AppRootDirectory, "counter");
    private static readonly string PortableMp3Directory = Path.Combine(AppRootDirectory, "mp3");
    private static readonly string PortableSettingsPath = Path.Combine(AppRootDirectory, "settings.json");

    public HotkeyBinding IncreaseHotkey { get; set; } = new(Keys.Up);
    public HotkeyBinding DecreaseHotkey { get; set; } = new(Keys.Down);
    public HotkeyBinding ResetHotkey { get; set; } = new(Keys.Delete);
    public HotkeyBinding StopSoundHotkey { get; set; } = new(Keys.End);
    public int BackgroundArgb { get; set; } = Color.LimeGreen.ToArgb();
    public string CountersDirectory { get; set; } = PortableCounterDirectory;
    public string Mp3Directory { get; set; } = PortableMp3Directory;
    public int SoundVolumePercent { get; set; } = 70;
    public string LastCounterFilePath { get; set; } = string.Empty;
    public bool UseThreeDigitCounterFormat { get; set; }
    public string CounterFontFamily { get; set; } = "Segoe UI";
    public float CounterFontSize { get; set; } = 96f;
    public bool EnableCounterAnimation { get; set; } = true;
    public int CounterFillArgb { get; set; } = Color.White.ToArgb();
    public int CounterOutlineArgb { get; set; } = Color.Black.ToArgb();
    public bool CounterFontBold { get; set; } = true;
    public bool ShowCounterOutline { get; set; } = true;

    [JsonIgnore]
    public Color BackgroundColor => Color.FromArgb(BackgroundArgb);

    [JsonIgnore]
    public Color CounterFillColor => Color.FromArgb(CounterFillArgb);

    [JsonIgnore]
    public Color CounterOutlineColor => Color.FromArgb(CounterOutlineArgb);

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(PortableSettingsPath))
            {
                var freshSettings = new AppSettings();
                freshSettings.Normalize();
                return freshSettings;
            }

            var json = File.ReadAllText(PortableSettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            var fallbackSettings = new AppSettings();
            fallbackSettings.Normalize();
            return fallbackSettings;
        }
    }

    public void Save()
    {
        Normalize();
        Directory.CreateDirectory(AppRootDirectory);
        Directory.CreateDirectory(CountersDirectory);
        Directory.CreateDirectory(PortableMp3Directory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(PortableSettingsPath, json);
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(CountersDirectory))
        {
            CountersDirectory = PortableCounterDirectory;
        }

        if (string.IsNullOrWhiteSpace(Mp3Directory))
        {
            Mp3Directory = PortableMp3Directory;
        }

        SoundVolumePercent = Math.Clamp(SoundVolumePercent, 0, 100);
        CounterFontSize = Math.Clamp(CounterFontSize, 12f, 300f);

        Directory.CreateDirectory(CountersDirectory);
        Directory.CreateDirectory(Mp3Directory);

        if (!string.IsNullOrWhiteSpace(LastCounterFilePath) && !File.Exists(LastCounterFilePath))
        {
            LastCounterFilePath = string.Empty;
        }
    }

    private static string ResolveAppRootDirectory() =>
        AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
