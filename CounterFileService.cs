namespace DeathCounter;

internal static class CounterFileService
{
    public static IReadOnlyList<string> GetCounterFiles(string countersDirectory)
    {
        Directory.CreateDirectory(countersDirectory);
        return Directory
            .GetFiles(countersDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static string BuildCounterFilePath(string countersDirectory, string counterName)
    {
        var safeName = SanitizeFileName(counterName);
        if (string.IsNullOrWhiteSpace(safeName))
        {
            throw new InvalidOperationException("Counter name is empty.");
        }

        return Path.Combine(countersDirectory, $"{safeName}.json");
    }

    public static string SanitizeFileName(string counterName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(counterName.Trim().Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        return cleaned.Trim();
    }
}
