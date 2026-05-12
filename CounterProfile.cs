using System.Text.Json;

namespace DeathCounter;

internal sealed class CounterProfile
{
    public string Name { get; set; } = string.Empty;
    public int CounterValue { get; set; }

    public static CounterProfile Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CounterProfile>(json) ?? new CounterProfile();
    }

    public void Save(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
