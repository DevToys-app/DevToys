using DevToys.Core.Settings;

namespace DevToys.CLI.Core.Settings;

[Export(typeof(ISettingsStorage))]
internal sealed class SettingsStorage : ISettingsStorage
{
    public void ResetSetting(string settingName)
    {
        throw new NotImplementedException();
    }

    public bool TryReadSetting(string settingName, out object? value)
    {
        throw new NotImplementedException();
    }

    public void WriteSetting(string settingName, object? value)
    {
        throw new NotImplementedException();
    }
}
