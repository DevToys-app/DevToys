#nullable enable

using DevTools.Core;
using DevTools.Core.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Providers.Impl.Tools.RegEx
{
    public sealed partial class RegExToolPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(RegExToolViewModel),
                typeof(RegExToolPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public RegExToolViewModel ViewModel
        {
            get => (RegExToolViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public RegExToolPage()
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
                ViewModel = (RegExToolViewModel)parameters.ViewModel!;
                DataContext = ViewModel;

                ViewModel.MatchTextBox = MatchTextBox;
            }

            base.OnNavigatedTo(e);
        }
    }
}