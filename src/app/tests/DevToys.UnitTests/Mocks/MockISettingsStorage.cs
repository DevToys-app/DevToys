using System.ComponentModel.Composition;
using DevToys.Core.Settings;

namespace DevToys.UnitTests.Mocks;

[Export(typeof(ISettingsStorage))]
internal class MockISettingsStorage : ISettingsStorage
{
    private readonly Dictionary<string, object> _settings = new();

    public void ResetSetting(string settingName)
    {
        throw new NotImplementedException();
    }

    public bool TryReadSetting(string settingName, out object value)
    {
        return _settings.TryGetValue(settingName, out value);
    }

    public void WriteSetting(string settingName, object value)
    {
        _settings[settingName] = value;
    }
}
