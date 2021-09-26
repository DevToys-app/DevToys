#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Core;
using DevToys.ViewModels.Tools.Base64EncoderDecoder;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.Base64EncoderDecoder
{
    public sealed partial class Base64EncoderDecoderToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(Base64EncoderDecoderToolViewModel),
                typeof(Base64EncoderDecoderToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public Base64EncoderDecoderToolViewModel ViewModel
        {
            get => (Base64EncoderDecoderToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public Base64EncoderDecoderToolPage()
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
                ViewModel = (Base64EncoderDecoderToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            if (!string.IsNullOrWhiteSpace(parameters.ClipBoardContent))
            {
                ViewModel.ConversionMode = Base64EncoderDecoderToolViewModel.DecodeConversion;
                ViewModel.InputValue = parameters.ClipBoardContent;
            }

            base.OnNavigatedTo(e);
        }
    }
}
