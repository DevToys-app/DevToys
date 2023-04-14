#nullable enable

using System;
using System.Diagnostics;
using DevToys.Api.Core.Settings;
using DevToys.Core.Settings;

namespace DevToys.ViewModels.Tools.Base64ImageEncoderDecoder
{
    internal class MockSettingsProvider : ISettingsProvider
    {
        private readonly ISettingsProvider _realSettingsProvider;

        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        public MockSettingsProvider(ISettingsProvider realSettingsProvider)
        {
            _realSettingsProvider = realSettingsProvider;
            _realSettingsProvider.SettingChanged += realSettingsProvider_SettingChanged;
        }

        private void realSettingsProvider_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            SettingChanged?.Invoke(this, e);
        }

        public T GetSetting<T>(SettingDefinition<T> settingDefinition)
        {
            if (settingDefinition.Name == PredefinedSettings.TextEditorTextWrapping.Name)
            {
                // Force to wrap the text of the code editor.
                Debug.Assert(typeof(T) == typeof(bool));
                return (T)(object)true;
            }
            else if (settingDefinition.Name == PredefinedSettings.TextEditorLineNumbers.Name)
            {
                // Force to hide the line numbers of the code editor.
                Debug.Assert(typeof(T) == typeof(bool));
                return (T)(object)false;
            }

            return _realSettingsProvider.GetSetting(settingDefinition);
        }

        public void ResetSetting<T>(SettingDefinition<T> settingDefinition)
        {
            throw new NotImplementedException();
        }

        public void SetSetting<T>(SettingDefinition<T> settingDefinition, T value)
        {
            throw new NotImplementedException();
        }
    }
}
