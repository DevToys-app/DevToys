#nullable enable

using DevTools.Core;
using DevTools.Core.Injection;
using DevTools.Core.Navigation;
using DevTools.Core.Threading;
using DevTools.Impl.Messages;
using DevTools.Impl.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DevTools.Impl.Views
{
    public sealed partial class MainPage : Page, IRecipient<NavigateToToolMessage>
    {
        private IMefProvider? _mefProvider;
        private IThread? _thread;

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

            // Register all recipient for messages
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = (NavigationParameter)e.Parameter;

            _mefProvider = parameters.ExportProvider;
            _thread = _mefProvider.Import<IThread>();

            // Set the view model
            ViewModel = parameters.ExportProvider.Import<MainPageViewModel>();
            DataContext = ViewModel;

            ViewModel.OnNavigatedToAsync(parameters.Parameter).Forget();

            base.OnNavigatedTo(e);
        }

        private void SearchBoxKeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            SearchBox.Focus(FocusState.Keyboard);
        }

        public void Receive(NavigateToToolMessage message)
        {
            Arguments.NotNull(message, nameof(message));
            Assumes.NotNull(_mefProvider, nameof(_mefProvider));
            Assumes.NotNull(_thread, nameof(_thread));

            _thread!.ThrowIfNotOnUIThread();

            contentFrame.Navigate(
                message.ViewModel.View,
                new NavigationParameter(
                    _mefProvider!,
                    message.ViewModel),
                new EntranceNavigationTransitionInfo());
        }
    }
}
