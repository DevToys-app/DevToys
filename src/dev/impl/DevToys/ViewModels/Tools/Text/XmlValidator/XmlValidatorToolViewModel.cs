#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
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
        private readonly XmlValidatorStrings _localizedStrings;

        private string? _xsdSchemaData;
        private string? _xmlData;
        private XmlParsingResult? _xmlParsingResult;
        private XsdParsingResult? _xsdSchemeParsingResult;
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
                SetProperty(ref _xmlData, value);
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

            DisplayValidationInfoBar();
        }

        private void ProcessNewXmlData()
        {
            if (string.IsNullOrWhiteSpace(_xmlData))
            {
                _xmlParsingResult = null;
                DisplayValidationInfoBar();
                return;
            }

            XmlSchemaSet schemaSet = _xsdSchemeParsingResult?.SchemaSet ?? new XmlSchemaSet();
            XmlParser xmlParser = new("XML data", schemaSet);
            _xmlParsingResult = xmlParser.Parse(_xmlData ?? String.Empty);

            DisplayValidationInfoBar();
        }

        private void ProcessNewXsdData()
        {
            if (string.IsNullOrWhiteSpace(_xsdSchemaData))
            {
                _xsdSchemeParsingResult = null;
                DisplayValidationInfoBar();
                return;
            }

            XsdParser xsdParser = new("XSD data");
            _xsdSchemeParsingResult = xsdParser.Parse(_xsdSchemaData ?? string.Empty);

            DisplayValidationInfoBar();
        }

        private void DisplayValidationInfoBar()
        {
            if (_xsdSchemeParsingResult is null || _xmlParsingResult is null)
            {
                string validationImpossibleMsg = _localizedStrings.ValidationImpossibleMsg;
                ValidationResult = new InfoBarData(InfoBarSeverity.Informational, validationImpossibleMsg);
                return;
            }

            bool wasValidationPerformedWithoutErrors = string.IsNullOrEmpty(_xmlParsingResult.ErrorMessage) &&
                                                       string.IsNullOrEmpty(_xmlParsingResult.ErrorMessage);

            List<string> namespaceErrors = new();
            bool areAllNamespacesDefinedInXsd = DetectMissingNamespacesInXsd(_xsdSchemeParsingResult, _xmlParsingResult, out string? nsMissingInXsdErrorMessage);
            if (!areAllNamespacesDefinedInXsd)
            {
                namespaceErrors.Add(nsMissingInXsdErrorMessage!);
            }

            bool areAllNamespacesDefinedInXml = DetectMissingNamespacesInXml(_xsdSchemeParsingResult, _xmlParsingResult, out string? nsMissingInXmlErrorMessage);
            if (!areAllNamespacesDefinedInXml)
            {
                namespaceErrors.Add(nsMissingInXmlErrorMessage!);
            }
            
            InfoBarSeverity infoBarSeverity;
            string message = String.Empty;
            if (!_xsdSchemeParsingResult.IsValid || !_xmlParsingResult.IsValid)
            {
                infoBarSeverity = InfoBarSeverity.Error;
            }
            else if (!wasValidationPerformedWithoutErrors)
            {
                infoBarSeverity = InfoBarSeverity.Warning;
                message = _xmlParsingResult.ErrorMessage + Environment.NewLine + _xsdSchemeParsingResult.ErrorMessage;
            }
            else if (!areAllNamespacesDefinedInXsd || !areAllNamespacesDefinedInXml)
            {
                infoBarSeverity = InfoBarSeverity.Warning;
                message = string.Join(Environment.NewLine, namespaceErrors);
            }
            else
            {
                infoBarSeverity = InfoBarSeverity.Success;
                message = _localizedStrings.XmlIsValidMessage;
            }

            ValidationResult = new InfoBarData(infoBarSeverity, message);
        }

        private bool DetectMissingNamespacesInXsd(XsdParsingResult xsdParsingResult, XmlParsingResult xmlParsingResult, out string? errorMessage)
        {
            errorMessage = null;
            
            List<XmlNamespace> namespacesMissingInXsd = NamespaceHelper.GetMissingNamespacesInXsd(xsdParsingResult, xmlParsingResult);
            bool areAllNamespacesDefinedInXsd = !namespacesMissingInXsd.Any();
            if (!areAllNamespacesDefinedInXsd)
            {
                string missingNamespacesFormatted = FormatNamespaces(namespacesMissingInXsd);
                errorMessage =
                    string.Format(_localizedStrings.XsdNamespacesInconsistentMsg, missingNamespacesFormatted);
            }

            bool isTargetNamespaceReferenceMissingInXml = NamespaceHelper.DetectMissingTargetNamespaceInXml(xsdParsingResult, xmlParsingResult, out string? missingTargetNamespace);
            if (isTargetNamespaceReferenceMissingInXml)
            {
                string formattedErrorMsg = string.Format(_localizedStrings.TargetNamespaceNotDefinedInXml, missingTargetNamespace);

                if (errorMessage is null)
                {
                    errorMessage = formattedErrorMsg;
                }
                else
                {
                    errorMessage += Environment.NewLine + formattedErrorMsg;
                }
            }

            return areAllNamespacesDefinedInXsd && !isTargetNamespaceReferenceMissingInXml;
        }

        private bool DetectMissingNamespacesInXml(XsdParsingResult xsdParsingResult, XmlParsingResult xmlParsingResult,
            out string? errorMessage)
        {
            List<XmlNamespace> namespacesMissingInXml = NamespaceHelper.GetMissingNamespacesInXml(xsdParsingResult, xmlParsingResult);
            bool areAllNamespacesDefinedInXml = !namespacesMissingInXml.Any(); 
            if (!areAllNamespacesDefinedInXml)
            {
                string missingNamespacesFormatted = FormatNamespaces(namespacesMissingInXml);
                errorMessage = string.Format(_localizedStrings.XmlNamespacesInconsistentMsg, missingNamespacesFormatted);
                return false;
            }

            errorMessage = null;
            return true;
        }
        
        private static string FormatNamespaces(IEnumerable<XmlNamespace> namespaces)
        {
            List<string> missingNamespaces = new();
            foreach(XmlNamespace ns in namespaces)
            {
                string formattedPrefix = string.Empty;
                if (string.Equals(ns.Prefix, string.Empty))
                {
                    formattedPrefix = "xmlns";
                }
                else
                {
                    formattedPrefix = $"xmlns:{ns.Prefix}";
                }
                
                missingNamespaces.Add(formattedPrefix + $"=\"{ns.Uri}\"");
            }
                
            return string.Join(", ", missingNamespaces);
        }
    }
}
