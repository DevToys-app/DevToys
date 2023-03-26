using DevToys.Api;
using DevToys.Api.Core.Theme;

namespace DevToys.Core.Settings;

public static class PredefinedSettings
{
    /// <summary>
    /// The language to use for the texts in the user interface.
    /// </summary>
    public static readonly SettingDefinition<string> Language
        = new(
            name: nameof(Language),
            defaultValue: "default");

    /// <summary>
    /// The color theme of the application.
    /// </summary>
    public static readonly SettingDefinition<AvailableApplicationTheme> Theme
        = new(
            name: nameof(Theme),
            defaultValue: AvailableApplicationTheme.Default);

    /// <summary>
    /// The internal name of the tool the most recently used.
    /// </summary>
    public static readonly SettingDefinition<string> RecentTool1
        = new(
            name: nameof(RecentTool1),
            defaultValue: string.Empty);

    /// <summary>
    /// The internal name of the tool the second most recently used.
    /// </summary>
    public static readonly SettingDefinition<string> RecentTool2
        = new(
            name: nameof(RecentTool2),
            defaultValue: string.Empty);

    /// <summary>
    /// The internal name of the tool the third most recently used.
    /// </summary>
    public static readonly SettingDefinition<string> RecentTool3
        = new(
            name: nameof(RecentTool3),
            defaultValue: string.Empty);

    /// <summary>
    /// Whether the application should automatically detect the best tool to use based on the clipboard content.
    /// </summary>
    public static readonly SettingDefinition<bool> SmartDetection
        = new(
            name: nameof(SmartDetection),
            defaultValue: true);
}
