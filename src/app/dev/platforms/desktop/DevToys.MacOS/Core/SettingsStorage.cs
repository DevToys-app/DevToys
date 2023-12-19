using DevToys.Core.Settings;

namespace DevToys.MacOS.Core;

[Export(typeof(ISettingsStorage))]
internal sealed class SettingsStorage : ISettingsStorage
{
    private readonly object _lock = new();

    public void ResetSetting(string settingName)
    {
        lock (_lock)
        {
            using NSUserDefaults userDefaults = NSUserDefaults.StandardUserDefaults;
            if (userDefaults[settingName] != null)
            {
                userDefaults.RemoveObject(settingName);
            }
        }
    }

    public bool TryReadSetting(string settingName, out object? value)
    {
        lock (_lock)
        {
            using NSUserDefaults userDefaults = NSUserDefaults.StandardUserDefaults;

            if (userDefaults[settingName] == null)
            {
                value = null;
                return false;
            }

            value = userDefaults.StringForKey(settingName);
            return true;
        }
    }

    public void WriteSetting(string settingName, object? value)
    {
        lock (_lock)
        {
            using NSUserDefaults userDefaults = NSUserDefaults.StandardUserDefaults;

            string? valueString = value?.ToString();
            if (value == null)
            {
                if (userDefaults[settingName] != null)
                {
                    userDefaults.RemoveObject(settingName);
                }
                return;
            }

            userDefaults.SetString(valueString, settingName);
        }
    }
}
