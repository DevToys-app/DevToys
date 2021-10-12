#nullable enable

using DevToys.Api.Tools;
using DevToys.Views.Tools.StringUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevToys.ViewModels.Tools.StringUtilities
{
    [Export(typeof(StringUtilitiesToolViewModel))]
    public sealed class StringUtilitiesToolViewModel : ObservableRecipient, IToolViewModel
    {
        private string? _text;

        public Type View { get; } = typeof(StringUtilitiesToolPage);

        internal StringUtilitiesStrings Strings => LanguageManager.Instance.StringUtilities;

        internal string? Text
        {
            get => _text;
            set
            {
                SetProperty(ref _text, value);
                // QueueRegExMatch();
            }
        }
    }
}
