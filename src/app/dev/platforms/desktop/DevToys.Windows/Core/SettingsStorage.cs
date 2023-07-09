using System.Collections.Concurrent;
using System.IO;
using DevToys.Api;
using DevToys.Core.Settings;

namespace DevToys.Windows.Core;

[Export(typeof(ISettingsStorage))]
internal sealed class SettingsStorage : ISettingsStorage
{
    private const string SettingsFileName = "settings.ini";

    private readonly IFileStorage _fileStorage;
    private readonly Lazy<ConcurrentDictionary<string, string>> _settings;

    [ImportingConstructor]
    public SettingsStorage(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;

        _settings
            = new Lazy<ConcurrentDictionary<string, string>>(
                LoadAllSettings());
    }

    public void ResetSetting(string settingName)
    {
        _settings.Value.Remove(settingName, out _);
    }

    public bool TryReadSetting(string settingName, out object? value)
    {
        if (_settings.Value.TryGetValue(settingName, out string? valueString))
        {
            value = valueString;
            return true;
        }

        value = null;
        return false;
    }

    public void WriteSetting(string settingName, object? value)
    {
        _settings.Value[settingName] = value?.ToString() ?? string.Empty;
        SaveAllSettingsAsync().Forget();
    }

    private ConcurrentDictionary<string, string> LoadAllSettings()
    {
        var result = new ConcurrentDictionary<string, string>();

        if (_fileStorage.FileExists(SettingsFileName))
        {
            using Stream stream = _fileStorage.OpenReadFile(SettingsFileName);
            using TextReader textReader = new StreamReader(stream);

            string? line = null;
            do
            {
                line = textReader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    int equalSign = line.IndexOf('=');
                    if (equalSign > 0)
                    {
                        string settingName = line.Substring(0, equalSign);
                        string settingValue = line.Substring(equalSign + 1);
                        result[settingName] = settingValue;
                    }
                }
            } while (line != null);
        }

        return result;
    }

    private Task SaveAllSettingsAsync()
    {
        return Task.Run(() =>
        {
            lock (_fileStorage)
            {
                using Stream stream = _fileStorage.OpenWriteFile(SettingsFileName, replaceIfExist: true);
                using TextWriter textWriter = new StreamWriter(stream);

                foreach (KeyValuePair<string, string> item in _settings.Value)
                {
                    textWriter.WriteLine($"{item.Key}={item.Value}");
                }
            }
        });
    }
}
