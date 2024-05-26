using DevToys.Core;

namespace DevToys.CLI.Core;

internal static class Constants
{
    internal static readonly string AppCacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppHelper.IsPreviewVersion.Value ? "DevToys-CLI-preview" : "DevToys-CLI");

    internal static string AppTempFolder => Path.Combine(AppCacheDirectory, "Temp");
}
