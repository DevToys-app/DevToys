using System.IO;

namespace DevToys.Linux.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine("var", "lib", "devtoys");

    internal static string PluginInstallationFolder => Path.Combine(AppCacheDirectory, "Plugins");
}
