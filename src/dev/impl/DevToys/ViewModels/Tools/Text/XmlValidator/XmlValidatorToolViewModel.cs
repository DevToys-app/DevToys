using System;
using System.Composition;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Views.Tools.XmlValidator;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DevToys.ViewModels.Tools.XmlValidator
{
    [Export(typeof(XmlValidatorToolViewModel))]
    public class XmlValidatorToolViewModel : ObservableRecipient, IToolViewModel
    {
        private string? _xsdSchema;
        private string? _xmlData;

        public Type View { get; } = typeof(XmlValidatorToolPage);

        internal ISettingsProvider SettingsProvider { get; }

        internal XmlValidatorStrings Strings => LanguageManager.Instance.XmlValidator;

        internal string? XsdSchema
        {
            get => _xsdSchema;
            set => SetProperty(ref _xsdSchema, value, broadcast: true);
        }

        internal string? XmlData
        {
            get => _xmlData;
            set => SetProperty(ref _xmlData, value, broadcast: true);
        }

        [ImportingConstructor]
        public XmlValidatorToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
        }
    }
}
