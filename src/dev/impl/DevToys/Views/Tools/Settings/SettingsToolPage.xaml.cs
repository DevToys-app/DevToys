#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Core;
using DevToys.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.Settings
{
    public sealed partial class SettingsToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SettingsToolViewModel),
                typeof(SettingsToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public SettingsToolViewModel ViewModel
        {
            get => (SettingsToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public SettingsToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            // Set the view model
            Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
            ViewModel = (SettingsToolViewModel)parameters.ViewModel!;
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }
    }
}
