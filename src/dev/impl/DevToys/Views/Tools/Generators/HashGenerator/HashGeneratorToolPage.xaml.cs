#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Shared.Core;
using DevToys.ViewModels.Tools.HashGenerator;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views.Tools.HashGenerator
{
    public sealed partial class HashGeneratorToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(HashGeneratorToolViewModel),
                typeof(HashGeneratorToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public HashGeneratorToolViewModel ViewModel
        {
            get => (HashGeneratorToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public HashGeneratorToolPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (ViewModel is null)
            {
                var parameters = (NavigationParameter)e.Parameter;

                // Set the view model
                Assumes.NotNull(parameters.ViewModel, nameof(parameters.ViewModel));
                ViewModel = (HashGeneratorToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;
            }

            base.OnNavigatedTo(e);
        }

        private void OutputType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox)?.SelectedValue as string == "Base64")
            {
                IsUppercaseToggleSwitch.IsEnabled = false;
            }
            else
            {
                IsUppercaseToggleSwitch.IsEnabled = true;
            }
        }

        private void IsUsingHmacToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            if(toggleSwitch != null) 
            {
                ViewModel.IsHmacMode = toggleSwitch.IsOn;
                if (toggleSwitch.IsOn)
                {
                    SecretKeyInput.Visibility = Visibility.Visible;
                }
                else
                {
                    SecretKeyInput.Visibility = Visibility.Collapsed;
                    SecretKeyInput.Text = "";
                }
            }
        }
    }
}
