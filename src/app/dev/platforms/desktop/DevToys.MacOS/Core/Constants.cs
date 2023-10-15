using System.IO;

namespace DevToys.MacOS.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine(FileSystem.CacheDirectory, "com.etiennebaudoux.devtoys");

    internal static string PluginInstallationFolder => Path.Combine(AppCacheDirectory, "Plugins");

    internal static string AppTempFolder => Path.Combine(AppCacheDirectory, "Temp");
}
