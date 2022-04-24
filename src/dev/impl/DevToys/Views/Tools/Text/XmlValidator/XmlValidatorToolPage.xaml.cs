#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.XmlValidator;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;

namespace DevToys.Views.Tools.XmlValidator
{
    public sealed partial class XmlValidatorToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(XmlValidatorToolViewModel),
                typeof(XmlValidatorToolPage),
                new PropertyMetadata(null));

        private bool _xmlIsValid;
        private readonly XmlValidatorStrings _localizedStrings;

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public XmlValidatorToolViewModel ViewModel
        {
            get => (XmlValidatorToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public XmlValidatorToolPage()
        {
            InitializeComponent();
            _localizedStrings = new XmlValidatorStrings();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel is null)
            {
                var parameters = (NavigationParameter)e.Parameter;

                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (XmlValidatorToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.XmlData) || string.IsNullOrEmpty(ViewModel.XsdSchema))
            {
                string errorMessage = _localizedStrings.ValidationImpossibleMsg;
                DisplayValidationResult(false, errorMessage);

                return;
            }

            ResetGui();

            XsdParser xsdParser = new("XSD content");
            XsdParsingResult xsdParsingResult = xsdParser.Parse(ViewModel.XsdSchema!);
            if (!xsdParsingResult.IsValid)
            {
                DisplayValidationResult(false, xsdParsingResult.ErrorMessage!);
                return;
            }

            XmlParser xmlParser = new XmlParser("XML content", xsdParsingResult.SchemaSet!);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(ViewModel.XmlData!);
            if (!xmlParsingResult.IsValid)
            {
                DisplayValidationResult(false, xmlParsingResult.ErrorMessage!);
                return;
            }

            _xmlIsValid = true;
            DisplayValidationResult(_xmlIsValid, _localizedStrings.XmlIsValidMessage);
        }

        private void ResetGui()
        {
            _xmlIsValid = false;
            ValidationResult.Visibility = Visibility.Collapsed;
            ValidationMessages.Text = string.Empty;
        }

        private void DisplayValidationResult(bool validationSucceeded, string validationMessages)
        {
            string iconSourcePath = "../../../../Assets/Icons/CheckMark.svg";
            if (!validationSucceeded)
            {
                iconSourcePath = "../../../../Assets/Icons/ExclamationMark.svg";
            }

            var iconUri = new Uri(BaseUri, iconSourcePath);
            ResultIcon.Source = new SvgImageSource(iconUri);
            ValidationMessages.Text = validationMessages;
            ValidationResult.Visibility = Visibility.Visible;
        }
    }
}
