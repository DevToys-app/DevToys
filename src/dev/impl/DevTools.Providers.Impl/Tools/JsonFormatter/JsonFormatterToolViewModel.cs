#nullable enable

using DevTools.Core.Settings;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Composition;

namespace DevTools.Providers.Impl.Tools.JsonFormatter
{
    [Export(typeof(JsonFormatterToolViewModel))]
    public class JsonFormatterToolViewModel : ObservableRecipient, IToolViewModel
    {
        public Type View { get; } = typeof(JsonFormatterToolPage);

        internal ISettingsProvider SettingsProvider { get; }

        [ImportingConstructor]
        public JsonFormatterToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
        }
    }
}
