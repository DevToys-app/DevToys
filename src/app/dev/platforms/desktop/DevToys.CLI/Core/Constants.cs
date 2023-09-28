namespace DevToys.CLI.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine(AppContext.BaseDirectory, "Cache");

    internal static string AppTempFolder => Path.Combine(AppCacheDirectory, "Temp");
}
