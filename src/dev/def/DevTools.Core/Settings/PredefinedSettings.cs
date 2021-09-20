using DevTools.Core.Theme;

namespace DevTools.Core.Settings
{
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

        /// <summary>
        /// Whether the application should automatically detect the best tool to use based on the clipboard content.
        /// </summary>
        public static readonly SettingDefinition<bool> SmartDetection
            = new(
                name: nameof(SmartDetection),
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// The font family name to use in the text editor.
        /// </summary>
        public static readonly SettingDefinition<string> TextEditorFont
            = new(
                name: nameof(TextEditorFont),
                isRoaming: true,
                defaultValue: "CascadiaMono");

        /// <summary>
        /// Whether the text in the text editor should wrap.
        /// </summary>
        public static readonly SettingDefinition<bool> TextEditorTextWrapping
            = new(
                name: nameof(TextEditorTextWrapping),
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Whether the line numbers should be displayed in the text editor.
        /// </summary>
        public static readonly SettingDefinition<bool> TextEditorLineNumbers
            = new(
                name: nameof(TextEditorLineNumbers),
                isRoaming: true,
                defaultValue: true);

        /// <summary>
        /// Whether the line where the caret is should be highlighted in the text editor.
        /// </summary>
        public static readonly SettingDefinition<bool> TextEditorHighlightCurrentLine
            = new(
                name: nameof(TextEditorHighlightCurrentLine),
                isRoaming: true,
                defaultValue: true);
    }
}
