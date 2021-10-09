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
        private bool _inlineMode;
        private string? _oldText;
        private string? _newText;

        public Type View { get; } = typeof(TextDiffToolPage);

        internal ISettingsProvider SettingsProvider { get; }

        internal TextDiffStrings Strings => LanguageManager.Instance.TextDiff;

        internal bool InlineMode
        {
            get => _inlineMode;
            set => SetProperty(ref _inlineMode, value, broadcast: true);
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
