using DevToys.Api;
using DevToys.Api.Core;
using DevToys.Api.Core.Theme;

namespace DevToys.UI.Framework.Helpers;

public static class Parts
{
    public static IMefProvider MefProvider { internal get; set; } = null!;

    public static IThemeListener ThemeListener => MefProvider!.Import<IThemeListener>();

    public static ISettingsProvider SettingsProvider => MefProvider!.Import<ISettingsProvider>();

    public static IClipboard Clipboard => MefProvider!.Import<IClipboard>();
}
