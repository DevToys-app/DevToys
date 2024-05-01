using SixLabors.ImageSharp;

namespace DevToys.Core.Settings;

public static class PredefinedSettings
{
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

            // Fonts included on some Linux distro.
            "Noto Mono",

            // Fonts included on Windows and MacOS.
            "Consolas",
            "Courier New",

            // Fonts included on Windows.
            "Segoe UI",
        };

    /// <summary>
    /// Indicates whether it is the first time the app starts or not.
    /// </summary>
    public static readonly SettingDefinition<bool> IsFirstStart
        = new(
            name: nameof(IsFirstStart),
            defaultValue: true);

    /// <summary>
    /// Equals to the last version the app runs. This setting is used to determine whether the user started the app
    /// for the first time after an update
    /// </summary>
    public static readonly SettingDefinition<string> LastVersionRan
        = new(
            name: nameof(LastVersionRan),
            defaultValue: string.Empty);

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

    /// <summary>
    /// Whether the application should automatically paste the content of the clipboard when selecting a tool that has been automatically recommended by Smart Detection.
    /// </summary>
    public static readonly SettingDefinition<bool> SmartDetectionPaste
        = new(
            name: nameof(SmartDetectionPaste),
            defaultValue: true);

    /// <summary>
    /// Whether the compact mode is enabled or not.
    /// </summary>
    public static readonly SettingDefinition<bool> CompactMode
        = new(
            name: nameof(CompactMode),
            defaultValue: false);

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
    /// The kind of EOL (End Of Line sequence) to use in the text editor (LF vs CRLF).
    /// </summary>
    public static readonly SettingDefinition<UITextEndOfLinePreference> TextEditorEndOfLinePreference
        = new(
            name: nameof(TextEditorEndOfLinePreference),
            defaultValue: UITextEndOfLinePreference.TextDefault);

    /// <summary>
    /// Whether when using the Paste command, the text in the editor should be replaced or appended.
    /// </summary>
    public static readonly SettingDefinition<bool> TextEditorPasteClearsText
        = new(
            name: nameof(TextEditorPasteClearsText),
            defaultValue: true);

    /// <summary>
    /// The size and position of the main window.
    /// </summary>
    public static readonly SettingDefinition<Rectangle?> MainWindowBounds
        = new(
            name: nameof(MainWindowBounds),
            defaultValue: null,
            serialize: SerializeRectangle,
            deserialize: DeserializeRectangle);

    /// <summary>
    /// Whether the main window should be maximized.
    /// </summary>
    public static readonly SettingDefinition<bool> MainWindowMaximized
        = new(
            name: nameof(MainWindowMaximized),
            defaultValue: false);

    internal static string SerializeRectangle(Rectangle? rectangle)
    {
        if (rectangle is null)
        {
            return string.Empty;
        }

        return $"{rectangle.Value.X},{rectangle.Value.Y},{rectangle.Value.Width},{rectangle.Value.Height}";
    }

    internal static Rectangle? DeserializeRectangle(string serializedRectangle)
    {
        if (string.IsNullOrEmpty(serializedRectangle))
        {
            return null;
        }

        string[] parts = serializedRectangle.Split(',');
        if (parts.Length != 4)
        {
            throw new ArgumentException("Invalid serialized rectangle format.");
        }

        int x = int.Parse(parts[0]);
        int y = int.Parse(parts[1]);
        int width = int.Parse(parts[2]);
        int height = int.Parse(parts[3]);

        return new Rectangle(x, y, width, height);
    }
}
