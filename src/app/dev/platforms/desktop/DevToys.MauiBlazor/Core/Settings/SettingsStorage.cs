using DevToys.Core.Settings;

namespace DevToys.MauiBlazor.Core.Settings;

[Export(typeof(ISettingsStorage))]
internal sealed class SettingsStorage : ISettingsStorage
{
    public void ResetSetting(string settingName)
    {
        Preferences.Default.Remove(settingName);
    }

    public bool TryReadSetting(string settingName, out object? value)
    {
        if (Preferences.Default.ContainsKey(settingName))
        {
            value = Preferences.Default.Get<object?>(settingName, default);
            return true;
        }

        value = null;
        return false;
    }

    public void WriteSetting(string settingName, object? value)
    {
        Preferences.Default.Set(settingName, value);
    }
}
