#nullable enable

using System;
using System.Collections;
using System.Composition;
using System.Globalization;
using DevToys.Api.Core.Settings;
using Newtonsoft.Json;
using Windows.Storage;

namespace DevToys.Core.Settings
{
    [Export(typeof(ISettingsProvider))]
    [Shared]
    internal sealed class SettingsProvider : ISettingsProvider
    {
        private readonly ApplicationDataContainer _roamingSettings = ApplicationData.Current.RoamingSettings;
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        public T GetSetting<T>(SettingDefinition<T> settingDefinition)
        {
            ApplicationDataContainer applicationDataContainer;
            if (settingDefinition.IsRoaming)
            {
                applicationDataContainer = _roamingSettings;
            }
            else
            {
                applicationDataContainer = _localSettings;
            }

            if (applicationDataContainer.Values.ContainsKey(settingDefinition.Name))
            {
                if (typeof(T).IsEnum)
                {
                    return (T)Enum.Parse(typeof(T), applicationDataContainer.Values[settingDefinition.Name]?.ToString() ?? string.Empty);
                }
                else if (typeof(IList).IsAssignableFrom(typeof(T)))
                {
                    return JsonConvert.DeserializeObject<T>(applicationDataContainer.Values[settingDefinition.Name]?.ToString() ?? string.Empty)!;
                }

                return (T)Convert.ChangeType(applicationDataContainer.Values[settingDefinition.Name], typeof(T), CultureInfo.InvariantCulture);
            }

            SetSetting(settingDefinition, settingDefinition.DefaultValue);
            return settingDefinition.DefaultValue;
        }

        public void SetSetting<T>(SettingDefinition<T> settingDefinition, T value)
        {
            object? valueToSave = value;
            if (value is Enum valueEnum)
            {
                valueToSave = valueEnum.ToString();
            }
            else if (value is IList list)
            {
                valueToSave = JsonConvert.SerializeObject(list, Formatting.None);
            }

            if (settingDefinition.IsRoaming)
            {
                _roamingSettings.Values[settingDefinition.Name] = valueToSave;
            }
            else
            {
                _localSettings.Values[settingDefinition.Name] = valueToSave;
            }

            SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, value));
        }

        public void ResetSetting<T>(SettingDefinition<T> settingDefinition)
        {
            if (settingDefinition.IsRoaming)
            {
                _roamingSettings.Values.Remove(settingDefinition.Name);
            }
            else
            {
                _localSettings.Values.Remove(settingDefinition.Name);
            }

            SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingDefinition.Name, settingDefinition.DefaultValue));
        }
    }
}
