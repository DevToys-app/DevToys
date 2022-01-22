#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Helpers;
using DevToys.Models;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using DevToys.ViewModels.Tools.NumberBaseConverter;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.NumberBaseConverter
{
    public sealed partial class NumberBaseConverterToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(NumberBaseConverterToolViewModel),
                typeof(NumberBaseConverterToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public NumberBaseConverterToolViewModel ViewModel
        {
            get => (NumberBaseConverterToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public NumberBaseConverterToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            if (ViewModel is null)
            {
                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (NumberBaseConverterToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                string clipBoardContent = NumberBaseFormatter.RemoveFormatting(parameters.ClipBoardContent).ToString();

                if (NumberBaseHelper.IsValidBinary(clipBoardContent!))
                {
                    ViewModel.InputBaseNumber = NumberBaseFormat.Binary;
                }
                else if (NumberBaseHelper.IsValidHexadecimal(clipBoardContent!))
                {
                    ViewModel.InputBaseNumber = NumberBaseFormat.Hexadecimal;
                }

                ViewModel.InputValue = parameters.ClipBoardContent;
            }

            base.OnNavigatedTo(e);
        }
    }
}
