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
    }
}
