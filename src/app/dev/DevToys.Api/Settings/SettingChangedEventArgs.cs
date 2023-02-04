namespace DevToys.Api;

/// <summary>
/// Represents the arguments of an event raised when an app setting change.
/// </summary>
public sealed class SettingChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the setting that changed.
    /// </summary>
    public string SettingName { get; }

    /// <summary>
    /// Gets the new value of the setting.
    /// </summary>
    public object? NewValue { get; }

    public SettingChangedEventArgs(string settingName, object? newValue)
    {
        SettingName = settingName;
        NewValue = newValue;
    }
}
