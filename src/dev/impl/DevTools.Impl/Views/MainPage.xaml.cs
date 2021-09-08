#nullable enable

using DevTools.Core;
using DevTools.Core.Navigation;
using DevTools.Core.Threading;
using DevTools.Impl.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Impl.Views
{
    public sealed partial class MainPage : Page
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(MainPageViewModel),
                typeof(MainPage),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        public MainPageViewModel ViewModel
        {
            get => (MainPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public MainPage()
        {
            InitializeComponent();

            // Set custom title bar dragging area
            Window.Current.SetTitleBar(AppTitleBar);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            // Setup the title bar.
            parameters.ExportProvider.Import<ITitleBar>().SetupTitleBarAsync().Forget();

            // Set the view model
            ViewModel = parameters.ExportProvider.Import<MainPageViewModel>();
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private void SearchBoxKeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            SearchBox.Focus(FocusState.Keyboard);
        }
    }
}
