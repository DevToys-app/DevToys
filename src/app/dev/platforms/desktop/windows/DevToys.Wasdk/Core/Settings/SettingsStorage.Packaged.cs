#if MSIX_PACKAGED || __WASM__
using DevToys.Core.Settings;
using Windows.Storage;

namespace DevToys.Wasdk.Core.Settings;

[Export(typeof(ISettingsStorage))]
internal sealed class SettingsStorage : ISettingsStorage
{
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

    public void ResetSetting(string settingName)
    {
        _localSettings.Values.Remove(settingName);
    }

    public bool TryReadSetting(string settingName, out object? value)
    {
        if (_localSettings.Values.ContainsKey(settingName))
        {
            value = _localSettings.Values[settingName];
            return true;
        }

        value = null;
        return false;
    }

    public void WriteSetting(string settingName, object? value)
    {
        _localSettings.Values[settingName] = value;
    }
}
#endif
