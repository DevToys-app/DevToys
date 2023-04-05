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
    /// Preferred default fonts. The app will the first font in this list that is available on the system.
    /// </summary>
    public static readonly string[] DefaultFonts
        = new[]
        {
            // Popular fonts developers install. If it's on the system, users likely want to use it.
            "Fira Code",
            "Source Code Pro",
            "DejaVu Sans Mono",
            "Hack",

            // Default Visual Studio fonts.
            "Cascadia Mono",
            "Cascadia Code",

            // Fonts included on MacOS.
            "Menlo",
            "Monaco",
            "SF Mono",
            "SF Pro",

            // Fonts included on Windows and MacOS.
            "Consolas",
            "Courier New",

            // Fonts included on Windows.
            "Segoe UI",
        };

    /// <summary>
    /// The font family name to use in the text editor.
    /// </summary>
    public static readonly SettingDefinition<string> TextEditorFont
        = new(
            name: nameof(TextEditorFont),
            defaultValue: string.Empty); // Default value will be defined by the app depending on the operating system.

    /// <summary>
    /// Whether the text in the text editor should wrap.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorTextWrapping
        = new(
            name: nameof(TextEditorTextWrapping),
            defaultValue: false);

    /// <summary>
    /// Whether the line numbers should be displayed in the text editor.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorLineNumbers
        = new(
            name: nameof(TextEditorLineNumbers),
            defaultValue: true);

    /// <summary>
    /// Whether the line where the caret is should be highlighted in the text editor.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorHighlightCurrentLine
        = new(
            name: nameof(TextEditorHighlightCurrentLine),
            defaultValue: true);

    /// <summary>
    /// Whether white spaces should be rendered in the text editor.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorRenderWhitespace
        = new(
            name: nameof(TextEditorRenderWhitespace),
            defaultValue: false);

    /// <summary>
    /// Whether when using the Paste command, the text in the editor should be replaced or appended.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorPasteClearsText
        = new(
            name: nameof(TextEditorPasteClearsText),
            defaultValue: true);
}
