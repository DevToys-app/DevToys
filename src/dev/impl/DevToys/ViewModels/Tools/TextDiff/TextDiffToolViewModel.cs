#nullable enable

using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Views.Tools.TextDiff;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevToys.ViewModels.Tools.TextDiff
{
    [Export(typeof(TextDiffToolViewModel))]
    public sealed class TextDiffToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the text difference should be displayed inlined or side by side.
        /// </summary>
        private static readonly SettingDefinition<bool> Inline
            = new(
                name: $"{nameof(TextDiffToolViewModel)}.{nameof(Inline)}",
                isRoaming: true,
                defaultValue: false);

        private string? _oldText;
        private string? _newText;

        public Type View { get; } = typeof(TextDiffToolPage);

        internal ISettingsProvider SettingsProvider { get; }

        internal TextDiffStrings Strings => LanguageManager.Instance.TextDiff;

        internal bool InlineMode
        {
            get => SettingsProvider.GetSetting(Inline);
            set
            {
                if (SettingsProvider.GetSetting(Inline) != value)
                {
                    SettingsProvider.SetSetting(Inline, value);
                    OnPropertyChanged();
                }
            }
        }

        internal string? OldText
        {
            get => _oldText;
            set => SetProperty(ref _oldText, value, broadcast: true);
        }

        internal string? NewText
        {
            get => _newText;
            set => SetProperty(ref _newText, value, broadcast: true);
        }

        [ImportingConstructor]
        public TextDiffToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
        }
    }
}
