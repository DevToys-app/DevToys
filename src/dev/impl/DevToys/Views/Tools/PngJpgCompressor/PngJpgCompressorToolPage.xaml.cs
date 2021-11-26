#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.PngJpgCompressor;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.PngJpgCompressor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PngJpgCompressorToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
       = DependencyProperty.Register(
           nameof(ViewModel),
           typeof(PngJpgCompressorToolViewModel),
           typeof(PngJpgCompressorToolPage),
           new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public PngJpgCompressorToolViewModel ViewModel
        {
            get => (PngJpgCompressorToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public PngJpgCompressorToolPage()
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
                ViewModel = (PngJpgCompressorToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }
    }
}
