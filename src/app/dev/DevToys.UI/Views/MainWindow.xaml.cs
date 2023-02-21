using DevToys.Api;
using DevToys.Api.Core.Theme;
using DevToys.UI.Framework.Controls;
using DevToys.UI.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;

namespace DevToys.UI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : BackdropPage
{
    private const string CompactOverlayStateName = "CompactOverlay";
    private const string NavigationViewExpandedStateName = "NavigationViewExpanded";
    private const string NavigationViewCompactStateName = "NavigationViewCompact";
    private const string NavigationViewMinimalStateName = "NavigationViewMinimal";

    private NavigationViewDisplayMode _navigationViewDisplayMode;

    public MainWindow(BackdropWindow backdropWindow, IThemeListener themeListener, IMefProvider mefProvider)
        : base(backdropWindow, themeListener)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Resize(1200, 800);

        // Workaround for a bug where opening the window in compact display mode will misalign the content layout.
        NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;

        Loaded += MainWindow_Loaded;
        SizeChanged += MainWindow_SizeChanged;
        CompactOverlayModeChanged += MainWindow_CompactOverlayModeChanged;

        ViewModel = mefProvider.Import<MainWindowViewModel>();
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainWindowViewModel),
            typeof(MainWindow),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets the page's view model.
    /// </summary>
    internal MainWindowViewModel ViewModel
    {
        get => (MainWindowViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty IsInCompactOverlayProperty =
        DependencyProperty.Register(
            nameof(IsInCompactOverlay),
            typeof(bool),
            typeof(MainWindow),
            new PropertyMetadata(false));

    internal bool IsInCompactOverlay
    {
        get => (bool)GetValue(IsInCompactOverlayProperty);
        private set => SetValue(IsInCompactOverlayProperty, value);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        IsInCompactOverlay = IsInCompactOverlayMode();
        SearchBox.Focus(FocusState.Keyboard);

        // Bug #54: Force to go to Expanded visual state on start fix an issue where starting the app
        //          with a size that made the app going to Compact state break the layout and Monaco Editor.
        VisualStateManager.GoToState(this, NavigationViewExpandedStateName, useTransitions: true);

        UpdateVisualState();

        // Workaround for a bug where opening the window in compact display mode will misalign the content layout.
        NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private void MainWindow_CompactOverlayModeChanged(BackdropWindow sender, EventArgs args)
    {
        IsInCompactOverlay = IsInCompactOverlayMode();
    }

    private void SearchBoxKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Keyboard);
    }

    private void NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        _navigationViewDisplayMode = NavigationView.DisplayMode;
        UpdateVisualState();
    }

    private void NavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        _navigationViewDisplayMode = NavigationViewDisplayMode.Compact;
        UpdateVisualState();
    }

    private void NavigationView_PaneOpening(NavigationView sender, object args)
    {
        _navigationViewDisplayMode = NavigationViewDisplayMode.Expanded;
        UpdateVisualState();
    }

    private void NavigationView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateVisualState();
    }

    private void CompactOverlayModeButton_Click(object sender, RoutedEventArgs e)
    {
        TryToggleCompactOverlayMode();
        if (IsInCompactOverlayMode())
        {
            Resize(400, 450);
        }
    }

    private void UpdateVisualState()
    {
#if !HAS_UNO
        if (IsInCompactOverlayMode())
        {
            VisualStateManager.GoToState(this, CompactOverlayStateName, useTransitions: true);
        }
        else
#endif
        {
            switch (_navigationViewDisplayMode)
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
}
