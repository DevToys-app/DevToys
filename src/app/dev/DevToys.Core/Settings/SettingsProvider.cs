using System.Collections;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Settings;

[Export(typeof(ISettingsProvider))]
internal sealed partial class SettingsProvider : ISettingsProvider
{
    private readonly ILogger _logger;
    private readonly ISettingsStorage _settingsStorage;

    [ImportingConstructor]
    public SettingsProvider(ISettingsStorage settingsStorage)
    {
        _logger = this.Log();
        _settingsStorage = settingsStorage;
    }

    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    public T GetSetting<T>(SettingDefinition<T> settingDefinition)
    {
        if (_settingsStorage.TryReadSetting(settingDefinition.Name, out object? settingValue))
        {
            if (settingDefinition.Deserialize is not null)
            {
                return settingDefinition.Deserialize(settingValue?.ToString() ?? string.Empty);
            }
            else if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), settingValue?.ToString() ?? string.Empty);
            }
            else if (
                typeof(DateTimeOffset).IsAssignableFrom(typeof(T))
                && DateTimeOffset.TryParse(settingValue?.ToString() ?? string.Empty, out DateTimeOffset parseResult))
            {
                if (parseResult is T test)
                {
                    return test;
                }
            }
            else if (typeof(IList).IsAssignableFrom(typeof(T)))
            {
                return JsonSerializer.Deserialize<T>(
                    settingValue?.ToString() ?? string.Empty,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })!;
            }

            object? result = Convert.ChangeType(settingValue, typeof(T), CultureInfo.InvariantCulture);
            if (result is T strongTypedResult)
            {
                return strongTypedResult;
            }
        }

        return settingDefinition.DefaultValue;
    }

    public void SetSetting<T>(SettingDefinition<T> settingDefinition, T value)
    {
        object? valueToSave = value;

        if (settingDefinition.Serialize is not null)
        {
            valueToSave = settingDefinition.Serialize(value);
        }
        else if (value is Enum valueEnum)
        {
            valueToSave = valueEnum.ToString();
        }
        else if (value is IList list)
        {
            valueToSave
                = JsonSerializer.Serialize(
                    list,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = false,
                    });
        }

        _settingsStorage.WriteSetting(settingDefinition.Name, valueToSave);

        LogSetSetting(settingDefinition.Name, valueToSave?.ToString() ?? "{null}");

        SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, value));
    }

    public void ResetSetting<T>(SettingDefinition<T> settingDefinition)
    {
        _settingsStorage.ResetSetting(settingDefinition.Name);

        SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, settingDefinition.DefaultValue));
    }

    [LoggerMessage(1, LogLevel.Information, "Setting '{settingName}' changed to '{newValue}'")]
    partial void LogSetSetting(string settingName, string newValue);
}
