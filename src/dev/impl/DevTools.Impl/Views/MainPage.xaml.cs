#nullable enable

using DevTools.Core.Navigation;
using DevTools.Impl.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Impl.Views
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Gets the page's view model.
        /// </summary>
        internal MainPageViewModel ViewModel => (MainPageViewModel)DataContext;

        public MainPage()
        {
            InitializeComponent();

            // Set custom title bar dragging area
            Window.Current.SetTitleBar(AppTitleBar);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;
            DataContext = parameters.ExportProvider.Import<MainPageViewModel>();

            base.OnNavigatedTo(e);
        }

        private void SearchBoxKeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            SearchBox.Focus(FocusState.Keyboard);
        }
    }
}
