#nullable enable

using DevTools.Common;
using DevTools.Core.Settings;
using DevTools.Core.Theme;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevTools.Impl.Views.Settings
{
    [Export(typeof(SettingsToolViewModel))]
    public sealed class SettingsToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly IThemeListener _themeListener;
        private readonly ISettingsProvider _settingsProvider;

        public Type View { get; } = typeof(SettingsToolPage);

        internal SettingsStrings Strings => LanguageManager.Instance.Settings;

        internal AppTheme Theme
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.Theme);
            set
            {
                _settingsProvider.SetSetting(PredefinedSettings.Theme, value);
                _themeListener.ApplyDesiredColorTheme();
            }
        }

        internal bool SmartDetection
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.SmartDetection);
            set => _settingsProvider.SetSetting(PredefinedSettings.SmartDetection, value);
        }

        internal string TextEditorFont
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorFont, value);
        }

        [ImportingConstructor]
        public SettingsToolViewModel(
            IThemeListener themeListener,
            ISettingsProvider settingsProvider)
        {
            _themeListener = themeListener;
            _settingsProvider = settingsProvider;
        }
    }
}
