using System.Text.Json;

namespace DeathCounter;

internal sealed class CounterProfile
{
    public string Name { get; set; } = string.Empty;
    public int StartValue { get; set; }
    public int CounterValue { get; set; }

    public static CounterProfile Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var profile = JsonSerializer.Deserialize<CounterProfile>(json) ?? new CounterProfile();
        if (string.IsNullOrWhiteSpace(profile.Name))
        {
            profile.Name = Path.GetFileNameWithoutExtension(filePath);
        }

        if (profile.CounterValue < profile.StartValue)
        {
            profile.StartValue = profile.CounterValue;
        }

        return profile;
    }

    public void Save(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
