namespace DeathCounter;

internal static class AppIconLoader
{
    public static Icon? LoadApplicationIcon()
    {
        try
        {
            using var extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            return extractedIcon?.Clone() as Icon;
        }
        catch
        {
            return null;
        }
    }
}
