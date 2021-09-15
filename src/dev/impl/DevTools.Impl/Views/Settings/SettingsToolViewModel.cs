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
        private readonly ISettingsProvider _settingsProvider;

        public Type View { get; } = typeof(SettingsToolPage);

        internal SettingsStrings Strings => LanguageManager.Instance.Settings;

        internal AppTheme Theme
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.Theme);
            set => _settingsProvider.SetSetting(PredefinedSettings.Theme, value);
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

        internal bool TextEditorTextWrapping
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorTextWrapping);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorTextWrapping, value);
        }

        internal bool TextEditorLineNumbers
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorLineNumbers);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorLineNumbers, value);
        }

        internal bool TextEditorHighlightCurrentLine
        {
            get => _settingsProvider.GetSetting(PredefinedSettings.TextEditorHighlightCurrentLine);
            set => _settingsProvider.SetSetting(PredefinedSettings.TextEditorHighlightCurrentLine, value);
        }

        [ImportingConstructor]
        public SettingsToolViewModel(
            ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }
    }
}
