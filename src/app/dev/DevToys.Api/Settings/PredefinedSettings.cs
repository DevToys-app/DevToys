namespace DevToys.Api;

public static class PredefinedSettings
{
    /// <summary>
    /// Whether the compact mode is enabled or not.
    /// </summary>
    public static readonly SettingDefinition<bool> CompactMode
        = new(
            name: nameof(CompactMode),
            defaultValue: false);

    /// <summary>
    /// Whether when using the Paste command, the text in the editor should be replaced or appended.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorPasteClearsText
        = new(
            name: nameof(TextEditorPasteClearsText),
            defaultValue: true);
}
