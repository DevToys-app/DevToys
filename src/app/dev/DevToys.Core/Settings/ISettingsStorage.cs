using System.Diagnostics.CodeAnalysis;

namespace DevToys.Core.Settings;

/// <summary>
/// Provides a platform-specific way to store and read app settings.
/// </summary>
public interface ISettingsStorage
{
    /// <summary>
    /// Saves an app setting.
    /// </summary>
    void WriteSetting(string settingName, object? value);

    /// <summary>
    /// Reads an app setting.
    /// </summary>
    bool TryReadSetting(string settingName, [MaybeNullWhen(false)] out object? value);

    /// <summary>
    /// Deletes the given setting.
    /// </summary>
    void ResetSetting(string settingName);
}
