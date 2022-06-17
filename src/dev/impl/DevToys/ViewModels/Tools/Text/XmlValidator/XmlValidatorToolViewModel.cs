# nullable enable

using System;
using System.Composition;
using System.Xml.Schema;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;
using DevToys.Views.Tools.XmlValidator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.XmlValidator
{
    [Export(typeof(XmlValidatorToolViewModel))]
    public class XmlValidatorToolViewModel : ObservableRecipient, IToolViewModel
    {
        private string? _xsdSchemaData;
        private string? _xmlData;
        private XmlParsingResult? _parsedXml;
        private XsdParsingResult? _parsedXsdScheme;
        private readonly XmlValidatorStrings _localizedStrings;
        private InfoBarData? _validationResult;

        internal ISettingsProvider SettingsProvider { get; }
        internal XmlValidatorStrings Strings => LanguageManager.Instance.XmlValidator;

        internal string? XsdSchema
        {
            get => _xsdSchemaData;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _xsdSchemaData, value);
                ProcessNewXsdData();
            }
        }
        
        internal string? XmlData
        {
            get => _xmlData;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _xmlData, value, broadcast: true);
                ProcessNewXmlData();
            }
        }

        public InfoBarData? ValidationResult
        {
            get => _validationResult;
            private set => SetProperty(ref _validationResult, value);
        }
        
        public Type View { get; } = typeof(XmlValidatorToolPage);
        
        [ImportingConstructor]
        public XmlValidatorToolViewModel(ISettingsProvider settingsProvider)
        {
            SettingsProvider = settingsProvider;
            _localizedStrings = new XmlValidatorStrings();
        }

        private void ProcessNewXmlData()
        {
            if (string.IsNullOrWhiteSpace(_xmlData))
            {
                _parsedXml = null;
                DisplayValidationResult();
                return;
            }
            
            XmlSchemaSet schemaSet = _parsedXsdScheme?.SchemaSet ?? new XmlSchemaSet();
            XmlParser xmlParser = new("XML data", schemaSet);
            _parsedXml = xmlParser.Parse(_xmlData ?? String.Empty);
            
            DisplayValidationResult();
        }

        private void ProcessNewXsdData()
        {
            if (string.IsNullOrWhiteSpace(_xsdSchemaData))
            {
                _parsedXsdScheme = null;
                DisplayValidationResult();
                return;
            }
            
            XsdParser xsdParser = new("XSD data");
            _parsedXsdScheme = xsdParser.Parse(_xsdSchemaData ?? string.Empty);
            
            DisplayValidationResult();
        }

        private void DisplayValidationResult()
        {
            if (_parsedXsdScheme is null || _parsedXml is null)
            {
                string validationImpossibleMsg = _localizedStrings.ValidationImpossibleMsg;
                ValidationResult = new InfoBarData(InfoBarSeverity.Informational, validationImpossibleMsg);
                return;
            }
            
            InfoBarSeverity infoBarSeverity;
            string message = _parsedXml.ErrorMessage + Environment.NewLine + _parsedXsdScheme.ErrorMessage;
            
            bool wasValidationPerformedWithoutErrors = string.IsNullOrEmpty(_parsedXml.ErrorMessage) &&
                                                       string.IsNullOrEmpty(_parsedXml.ErrorMessage);
            
            if (!_parsedXml.IsValid || !_parsedXml.IsValid)
            {
                infoBarSeverity = InfoBarSeverity.Error;
            }
            else if (wasValidationPerformedWithoutErrors is not true)
            {
                infoBarSeverity = InfoBarSeverity.Warning;
            }
            else
            {
                infoBarSeverity= InfoBarSeverity.Success;
                message = _localizedStrings.XmlIsValidMessage;
            }

            ValidationResult = new InfoBarData(infoBarSeverity, message);
        }
    }
}
