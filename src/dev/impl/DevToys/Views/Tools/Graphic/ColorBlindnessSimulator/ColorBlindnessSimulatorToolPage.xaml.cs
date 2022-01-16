#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.ColorBlindnessSimulator;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.ColorBlindnessSimulator
{
    public sealed partial class ColorBlindnessSimulatorToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
          = DependencyProperty.Register(
              nameof(ViewModel),
              typeof(ColorBlindnessSimulatorToolViewModel),
              typeof(ColorBlindnessSimulatorToolPage),
              new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public ColorBlindnessSimulatorToolViewModel ViewModel
        {
            get => (ColorBlindnessSimulatorToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ColorBlindnessSimulatorToolPage()
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
                ViewModel = (ColorBlindnessSimulatorToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }
    }
}
