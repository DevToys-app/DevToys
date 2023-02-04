using DevToys.Api;

namespace DevToys.Core.Settings;

public static class PredefinedSettings
{
    /// <summary>
    /// The language to use for the texts in the user interface.
    /// </summary>
    public static readonly SettingDefinition<string> Language
        = new(
            name: nameof(Language),
            isRoaming: false,
            defaultValue: "default");

    /// <summary>
    /// The color theme of the application.
    /// </summary>
    public static readonly SettingDefinition<AppTheme> Theme
        = new(
            name: nameof(Theme),
            isRoaming: false,
            defaultValue: AppTheme.Default);
}
