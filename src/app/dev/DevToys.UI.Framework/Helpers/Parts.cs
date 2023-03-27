using DevToys.Api;
using DevToys.Api.Core;

namespace DevToys.UI.Framework.Helpers;

public static class Parts
{
    public static ISettingsProvider SettingsProvider { internal get; set; } = null!;

    public static IClipboard Clipboard { internal get; set; } = null!;
}
