#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Api.Core.Theme;

namespace DevToys.Core.Settings
{
    public static class PredefinedSettings
    {
        /// <summary>
        /// Equals to the last version the app runs. This setting is used to determine whether the user started the app
        /// for the first time after an update
        /// </summary>
        public static readonly SettingDefinition<string> LastVersionRan
            = new(
                name: nameof(LastVersionRan),
                isRoaming: false,
                defaultValue: string.Empty);

        /// <summary>
        /// Allows to know if it's the first time the app is started after being installed.
        /// </summary>
        public static readonly SettingDefinition<bool> FirstTimeStart
            = new(
                name: nameof(FirstTimeStart),
                isRoaming: false,
                defaultValue: true);

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

        public static readonly string[] SupportedFonts
            = new[]
            {
                "Fira Code",
                "Source Code Pro",
                "DejaVu Sans Mono",
                "Menlo",
                "Hack",
                "Monaco",
                "Cascadia Code",
                "Cascadia Mono",
                "Consolas",
                "Courier New",
                "Segoe UI",
            };

        /// <summary>
        /// The font family name to use in the text editor.
        /// </summary>
        public static readonly SettingDefinition<string> TextEditorFont
            = new(
                name: nameof(TextEditorFont),
                isRoaming: true,
                defaultValue: "Cascadia Mono");

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

        /// <summary>
        /// Whether white spaces should be rendered in the text editor.
        /// </summary>
        public static readonly SettingDefinition<bool> TextEditorRenderWhitespace
            = new(
                name: nameof(TextEditorRenderWhitespace),
                isRoaming: true,
                defaultValue: false);
    }
}
