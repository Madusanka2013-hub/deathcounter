using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeathCounter;

internal sealed class AppSettings
{
    private const string AppFolderName = "DeathCounter";

    private static readonly string AppRootDirectory = ResolveAppRootDirectory();
    private static readonly string LegacyPortableCounterDirectory = Path.Combine(AppRootDirectory, "counter");
    private static readonly string LegacyPortableMp3Directory = Path.Combine(AppRootDirectory, "mp3");
    private static readonly string LegacyPortableSettingsPath = Path.Combine(AppRootDirectory, "settings.json");

    private static readonly string PersistentDataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    private static readonly string PersistentCounterDirectory = Path.Combine(PersistentDataDirectory, "counter");
    private static readonly string PersistentMp3Directory = Path.Combine(PersistentDataDirectory, "mp3");
    private static readonly string PersistentSettingsPath = Path.Combine(PersistentDataDirectory, "settings.json");

    public HotkeyBinding IncreaseHotkey { get; set; } = new(Keys.Up);
    public HotkeyBinding DecreaseHotkey { get; set; } = new(Keys.Down);
    public HotkeyBinding ResetHotkey { get; set; } = new(Keys.Delete);
    public HotkeyBinding StopSoundHotkey { get; set; } = new(Keys.End);
    public int BackgroundArgb { get; set; } = Color.LimeGreen.ToArgb();
    public string CountersDirectory { get; set; } = PersistentCounterDirectory;
    public string Mp3Directory { get; set; } = PersistentMp3Directory;
    public int SoundVolumePercent { get; set; } = 70;
    public string LastCounterFilePath { get; set; } = string.Empty;
    public string LastCounterName { get; set; } = string.Empty;
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
            EnsureStorageInitialized();

            if (!File.Exists(PersistentSettingsPath))
            {
                var freshSettings = new AppSettings();
                freshSettings.Normalize();
                return freshSettings;
            }

            var json = File.ReadAllText(PersistentSettingsPath);
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
        Directory.CreateDirectory(PersistentDataDirectory);
        Directory.CreateDirectory(CountersDirectory);
        Directory.CreateDirectory(Mp3Directory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(PersistentSettingsPath, json);
    }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(CountersDirectory) || IsLegacyPortablePath(CountersDirectory, LegacyPortableCounterDirectory))
        {
            CountersDirectory = PersistentCounterDirectory;
        }

        if (string.IsNullOrWhiteSpace(Mp3Directory) || IsLegacyPortablePath(Mp3Directory, LegacyPortableMp3Directory))
        {
            Mp3Directory = PersistentMp3Directory;
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

    private static void EnsureStorageInitialized()
    {
        Directory.CreateDirectory(PersistentDataDirectory);
        Directory.CreateDirectory(PersistentCounterDirectory);
        Directory.CreateDirectory(PersistentMp3Directory);

        if (!File.Exists(PersistentSettingsPath) && File.Exists(LegacyPortableSettingsPath))
        {
            File.Copy(LegacyPortableSettingsPath, PersistentSettingsPath, overwrite: false);
        }

        CopyMissingFiles(LegacyPortableCounterDirectory, PersistentCounterDirectory);
        CopyMissingFiles(LegacyPortableMp3Directory, PersistentMp3Directory);
    }

    private static void CopyMissingFiles(string sourceDirectory, string targetDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return;
        }

        Directory.CreateDirectory(targetDirectory);

        foreach (var sourceFilePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(sourceFilePath);
            var targetFilePath = Path.Combine(targetDirectory, fileName);
            if (!File.Exists(targetFilePath))
            {
                File.Copy(sourceFilePath, targetFilePath);
            }
        }
    }

    private static bool IsLegacyPortablePath(string path, string legacyPath) =>
        string.Equals(
            Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(legacyPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase);

    private static string ResolveAppRootDirectory() =>
        AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
