using DevTools.Core.Theme;

namespace DevTools.Core.Settings
{
    public static class PredefinedSettings
    {
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
    }
}
