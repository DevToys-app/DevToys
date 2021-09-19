#nullable enable

using DevTools.Core.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Providers.Impl.Tools.Base64EncoderDecoder
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

            // Set the view model
            ViewModel = (Base64EncoderDecoderToolViewModel)parameters.Parameter[0]!;
            DataContext = ViewModel;
            if (!string.IsNullOrWhiteSpace((string)parameters.Parameter[1]))
            {
                ViewModel.ConversionMode = "Decode";
                ViewModel.InputValue = (string)parameters.Parameter[1];
            }

            base.OnNavigatedTo(e);
        }
    }
}
