namespace DevTools.Core.Settings
{
    /// <summary>
    /// Provides a set of methods to manage the application's settings.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Gets the value of a defined setting.
        /// </summary>
        /// <typeparam name="T">The type of value that will be retrieved.</typeparam>
        /// <param name="settingDefinition">The <see cref="SettingDefinition{T}"/> that defines the targetted setting.</param>
        /// <returns>Return the value of the setting or its default value.</returns>
        T GetSetting<T>(SettingDefinition<T> settingDefinition);

        /// <summary>
        /// Sets the value of a given setting.
        /// </summary>
        /// <typeparam name="T">The type of value that will be set.</typeparam>
        /// <param name="settingDefinition">The <see cref="SettingDefinition{T}"/> that defines the targetted setting.</param>
        /// <param name="value">The value to set</param>
        void SetSetting<T>(SettingDefinition<T> settingDefinition, T value);

        /// <summary>
        /// Resets a given setting to its default value.
        /// </summary>
        void ResetSetting<T>(SettingDefinition<T> settingDefinition);
    }
}
