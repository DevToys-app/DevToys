namespace DevToys.MacOS.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine(GetCacheDirectory(), "com.devtoys");

    internal static string PluginInstallationFolder => Path.Combine(GetLibraryDirectory(), "com.devtoys", "Plugins");

    internal static string AppTempFolder => Path.Combine(AppCacheDirectory, "Temp");

    private static string GetCacheDirectory() => GetDirectory(NSSearchPathDirectory.CachesDirectory);

    private static string GetLibraryDirectory() => GetDirectory(NSSearchPathDirectory.LibraryDirectory);

    private static string GetDirectory(NSSearchPathDirectory directory)
    {
        string[]? dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);

        Guard.IsNotNull(dirs);
        Guard.IsGreaterThan(dirs.Length, 0);

        return dirs[0];
    }
}
