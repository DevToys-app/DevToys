#nullable enable

using DevToys.Api.Core;
using DevToys.Api.Core.Injection;
using DevToys.Api.Core.Navigation;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Messages;
using DevToys.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace DevToys.Views
{
    public sealed partial class MainPage : Page, IRecipient<NavigateToToolMessage>
    {
        private const string CompactOverlayStateName = "CompactOverlay";
        private const string NavigationViewExpandedStateName = "NavigationViewExpanded";
        private const string NavigationViewCompactStateName = "NavigationViewCompact";
        private const string NavigationViewMinimalStateName = "NavigationViewMinimal";

        private IMefProvider? _mefProvider;
        private NavigationParameter? _parameters;

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

            Loaded += MainPage_Loaded;
            SizeChanged += MainPage_SizeChanged;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Assumes.NotNull(_parameters, nameof(_parameters));
            Assumes.NotNull(_mefProvider, nameof(_mefProvider));

            SearchBox.Focus(FocusState.Keyboard);

            // Calling OnNavigatedToAsync in Loaded event instead of OnNavigatedTo because it needs access to CoreDispatcher,
            // which isn't available before the main window is created.
            ViewModel.OnNavigatedToAsync(_parameters!).Forget();

            NotificationControl.NotificationService = _mefProvider!.Import<INotificationService>();

            // Bug #54: Force to go to Expanded visual state on start fix an issue where starting the app
            //          with a size that made the app going to Compact state break the layout and Monaco Editor.
            VisualStateManager.GoToState(this, NavigationViewExpandedStateName, useTransitions: true);

            UpdateVisualState();
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _parameters = (NavigationParameter)e.Parameter;

            _mefProvider = _parameters.ExportProvider;

            // Set the view model
            ViewModel = _mefProvider!.Import<MainPageViewModel>();
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private void SearchBoxKeyboardAccelerator_Invoked(Windows.UI.Xaml.Input.KeyboardAccelerator sender, Windows.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            SearchBox.Focus(FocusState.Keyboard);
        }

        private void NavigationView_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
        {
            ViewModel.NavigationViewDisplayMode = NavigationView.DisplayMode;
            UpdateVisualState();
        }

        private void NavigationView_PaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
        {
            ViewModel.IsNavigationViewPaneOpened = false;
            UpdateVisualState();
        }

        private void NavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            ViewModel.IsNavigationViewPaneOpened = true;
            UpdateVisualState();
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.IsNavigationViewPaneOpened = NavigationView.IsPaneOpen;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            var view = ApplicationView.GetForCurrentView();
            bool isCompactOverlayMode = view.ViewMode == ApplicationViewMode.CompactOverlay;

            if (isCompactOverlayMode)
            {
                VisualStateManager.GoToState(this, CompactOverlayStateName, useTransitions: true);
            }
            else
            {
                switch ((NavigationViewDisplayMode)ViewModel.NavigationViewDisplayMode)
                {
                    case NavigationViewDisplayMode.Minimal:
                        VisualStateManager.GoToState(this, NavigationViewMinimalStateName, useTransitions: true);
                        break;

                    case NavigationViewDisplayMode.Compact:
                        VisualStateManager.GoToState(this, NavigationViewCompactStateName, useTransitions: true);
                        break;

                    case NavigationViewDisplayMode.Expanded:
                        VisualStateManager.GoToState(this, NavigationViewExpandedStateName, useTransitions: true);
                        break;
                }
            }
        }

        public void Receive(NavigateToToolMessage message)
        {
            Arguments.NotNull(message, nameof(message));
            Assumes.NotNull(_mefProvider, nameof(_mefProvider));

            ThreadHelper.ThrowIfNotOnUIThread();

            ContentFrame.Navigate(
                message.ViewModel.View,
                new NavigationParameter(
                    _mefProvider!,
                    message.ViewModel,
                    message.ClipboardContentData),
                new EntranceNavigationTransitionInfo());
        }
    }
}