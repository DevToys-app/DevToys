using DevToys.Api;
using DevToys.Api.Core.Theme;
using DevToys.Business.Models;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.UI.Framework.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

#if !__WINDOWS__
using WindowActivationState = Windows.UI.Core.CoreWindowActivationState;
#endif

namespace DevToys.UI.Views;

/// <summary>
/// Main UI of the app
/// </summary>
public sealed partial class MainWindow : BackdropPage
{
    internal const string CompactOverlayStateName = "CompactOverlay";
    private const string NavigationViewExpandedStateName = "NavigationViewExpanded";
    private const string NavigationViewCompactStateName = "NavigationViewCompact";
    private const string NavigationViewMinimalStateName = "NavigationViewMinimal";

    private static MainWindow? MainWindowInstance;

    private readonly IMefProvider _mefProvider;

    private NavigationViewDisplayMode _navigationViewDisplayMode;
    private GuiToolInstance? _currentDisplayedTool;

    public MainWindow(BackdropWindow backdropWindow, IThemeListener themeListener, IMefProvider mefProvider)
        : base(backdropWindow, themeListener)
    {
        Guard.IsNotNull(mefProvider);
        _mefProvider = mefProvider;

        MainWindowInstance = this;

        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Resize(1250, 800);

        // Workaround for a bug where opening the window in compact display mode will misalign the content layout.
        MenuNavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;

        Loaded += MainWindow_Loaded;
        SizeChanged += MainWindow_SizeChanged;
        CompactOverlayModeChanged += MainWindow_CompactOverlayModeChanged;
        Activated += MainWindow_Activated;

        DataContext = mefProvider.Import<MainWindowViewModel>();
        ViewModel.SelectedMenuItemChanged += ViewModel_SelectedMenuItemChanged;
    }

    /// <summary>
    /// Gets the page's view model.
    /// </summary>
    internal MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

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
        MenuNavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;

        // Binding "IsExpanded" to IsExpandedByDefault in the XAML doesn't work on UI load, likely because the item has no children yet when the UI is composed.
        // So we manually expand the items that should be expanded on load.
        foreach (INotifyPropertyChanged? item in ViewModel.HeaderAndBodyToolViewItems)
        {
            if (item is GroupViewItem groupViewItem && groupViewItem.MenuItemShouldBeExpandedByDefault)
            {
                var menuItem = MenuNavigationView.ContainerFromMenuItem(item) as NavigationViewItem;
                if (menuItem is not null)
                {
                    menuItem.IsExpanded = true;
                }
            }
        }

        // Explicitly select the first item in the menu.
        Guard.IsNotEmpty((IReadOnlyList<INotifyPropertyChanged>)ViewModel.HeaderAndBodyToolViewItems);
        ViewModel.SelectedMenuItem = ViewModel.HeaderAndBodyToolViewItems[0];

        // Start Smart Detection
        ViewModel.RunSmartDetectionAsync(IsInCompactOverlay).Forget();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateVisualState();
    }

    private void MainWindow_CompactOverlayModeChanged(BackdropWindow sender, EventArgs args)
    {
        IsInCompactOverlay = IsInCompactOverlayMode();
    }

    private void MainWindow_Activated(BackdropWindow sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.CodeActivated && IsLoaded)
        {
            ViewModel.RunSmartDetectionAsync(IsInCompactOverlay).Forget();
        }
    }

    private void MenuNavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        _navigationViewDisplayMode = MenuNavigationView.DisplayMode;
        UpdateVisualState();
    }

    private void MenuNavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
    {
        _navigationViewDisplayMode = NavigationViewDisplayMode.Compact;
        UpdateVisualState();
    }

    private void MenuNavigationView_PaneOpening(NavigationView sender, object args)
    {
        _navigationViewDisplayMode = NavigationViewDisplayMode.Expanded;
        UpdateVisualState();
    }

    private void MenuNavigationView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateVisualState();
    }

    private void MenuNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        ViewModel.GoBack();
    }

    private void SearchBoxText_Changed(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        ViewModel.SearchBoxTextChangedCommand.Execute((SearchBoxTextChangedReason)args.Reason);
    }

    private void SearchBoxText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchBoxQuerySubmittedCommand.Execute(args.ChosenSuggestion);
    }

    private void SearchBoxKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        SearchBox.Focus(FocusState.Keyboard);
    }

    private void ViewModel_SelectedMenuItemChanged(object? sender, EventArgs e)
    {
        if (ViewModel.SelectedMenuItem is not null)
        {
            if (ViewModel.SelectedMenuItem is GroupViewItem groupViewItem)
            {
                _currentDisplayedTool = null;
                ContentFrame.Navigate(
                    typeof(ToolGroupPage),
                    new NavigationParameters<GroupViewItem>(_mefProvider, groupViewItem),
                    new EntranceNavigationTransitionInfo());
                SetTitle(string.Format(DevToys.Localization.Strings.MainWindow.MainWindow.WindowTitleInSystem, groupViewItem.DisplayTitle));
            }
            else if (ViewModel.SelectedMenuItem is GuiToolViewItem guiToolViewItem)
            {
                // Don't navigate if we already navigated to this tool.
                if (guiToolViewItem.ToolInstance != _currentDisplayedTool)
                {
                    _currentDisplayedTool = guiToolViewItem.ToolInstance;
                    ContentFrame.Navigate(
                        typeof(ToolPage),
                        new NavigationParameters<GuiToolViewItem>(_mefProvider, guiToolViewItem),
                        new EntranceNavigationTransitionInfo());
                    SetTitle(string.Format(DevToys.Localization.Strings.MainWindow.MainWindow.WindowTitleInSystem, guiToolViewItem.ToolInstance.LongOrShortDisplayTitle));
                }
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }

            UpdateVisualState();
        }
        else
        {
            _currentDisplayedTool = null;
        }

        ViewModel.UpdateWindowTitle(IsInCompactOverlayMode());
    }

    private void CompactOverlayModeButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Switch the UI to compact mode (smaller controls) automatically when in compact overlay mode, and restore it to whatever it was
        // when leaving the mode.

        TryToggleCompactOverlayMode();

        bool isInCompactOverlayMode = IsInCompactOverlayMode();
        if (isInCompactOverlayMode)
        {
            Resize(400, 450);
        }

        ViewModel.UpdateWindowTitle(isInCompactOverlayMode);
    }

    private void UpdateVisualState()
    {
#if !HAS_UNO
        if (IsInCompactOverlayMode())
        {
            VisualStateManager.GoToState(this, CompactOverlayStateName, useTransitions: true);
            SetContentFrameVisualState(CompactOverlayStateName);
        }
        else
#endif
        {
            switch (_navigationViewDisplayMode)
            {
                case NavigationViewDisplayMode.Minimal:
                    VisualStateManager.GoToState(this, NavigationViewMinimalStateName, useTransitions: true);
                    SetContentFrameVisualState(NavigationViewMinimalStateName);
                    break;

                case NavigationViewDisplayMode.Compact:
                    VisualStateManager.GoToState(this, NavigationViewCompactStateName, useTransitions: true);
                    SetContentFrameVisualState(NavigationViewCompactStateName);
                    break;

                case NavigationViewDisplayMode.Expanded:
                    VisualStateManager.GoToState(this, NavigationViewExpandedStateName, useTransitions: true);
                    SetContentFrameVisualState(NavigationViewExpandedStateName);
                    break;
            }
        }
    }

    private void SetContentFrameVisualState(string visualStateName)
    {
        Guard.IsNotNullOrWhiteSpace(visualStateName);
        if (ContentFrame.Content is IVisualStateListener visualStateListener)
        {
            visualStateListener.SetVisualState(visualStateName);
        }
    }

    private static bool UpdateMenuItemShouldBeExpanded(bool menuItemShouldBeExpanded)
    {
        Guard.IsNotNull(MainWindowInstance);
        return menuItemShouldBeExpanded && MainWindowInstance._navigationViewDisplayMode == NavigationViewDisplayMode.Expanded;
    }
}
