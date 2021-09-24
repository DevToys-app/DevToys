#nullable enable

using DevTools.Core;
using DevTools.Core.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Providers.Impl.Tools.HashGenerator
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
    }
}