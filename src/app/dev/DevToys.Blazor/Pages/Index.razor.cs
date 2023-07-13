using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages;

public partial class Index : MefComponentBase
{
    private const int TitleBarMarginLeftWhenCompactOverlayMode = 0;
    private const int TitleBarMarginLeftWhenNavBarHidden = 90;
    private const int TitleBarMarginLeftWhenNavBarNotHidden = 47;

    private NavBar<INotifyPropertyChanged, GuiToolViewItem> _navBar = default!;

    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Import]
    internal GuiToolProvider GuiToolProvider { get; set; } = default!;

    [Import]
    internal TitleBarInfoProvider TitleBarInfoProvider { get; set; } = default!;

    [Import]
    internal IThemeListener ThemeListener { get; set; } = default!;

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    [Inject]
    internal IWindowService WindowService { get; set; } = default!;

    /// <summary>
    /// Indicates whether we're transitioning to another selected menu item.
    /// </summary>
    public bool IsTransitioning { get; set; }

    [Parameter]
    public bool WindowHasFocus { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.SelectedMenuItemChanged += ViewModel_SelectedMenuItemChanged;
        ViewModel.SelectedMenuItem ??= ViewModel.HeaderAndBodyToolViewItems[0];
        ContextMenuService.IsContextMenuOpenedChanged += ContextMenuService_IsContextMenuOpenedChanged;
        WindowService.WindowActivated += WindowService_WindowActivated;
        WindowService.WindowDeactivated += WindowService_WindowDeactivated;
        TitleBarInfoProvider.PropertyChanged += TitleBarMarginProvider_PropertyChanged;

        TitleBarInfoProvider.TitleBarMarginRight = 40;
        WindowHasFocus = true;
    }

    private void ViewModel_SelectedMenuItemChanged(object? sender, EventArgs e)
    {
        // This will force the page content to clear our, disposing the current tool group or tool component before creating a new one, instead
        // of re-using the one currently displayed.
        IsTransitioning = true;
        StateHasChanged();
    }

    private void ContextMenuService_IsContextMenuOpenedChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }

    private void TitleBarMarginProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void WindowService_WindowActivated(object? sender, EventArgs e)
    {
        WindowHasFocus = true;
        StateHasChanged();

        // Start Smart Detection
        ViewModel.RunSmartDetectionAsync(WindowService.IsCompactOverlayMode)
            .ContinueWith(async _ =>
            {
                await InvokeAsync(StateHasChanged);
            });
    }

    private void WindowService_WindowDeactivated(object? sender, EventArgs e)
    {
        WindowHasFocus = false;
        StateHasChanged();
    }

    private void OnBackButtonClicked()
    {
        ViewModel.GoBack();
    }

    private void OnToggleCompactOverlayModeButtonClick()
    {
        WindowService.IsCompactOverlayMode = !WindowService.IsCompactOverlayMode;

        OnHiddenStateChanged(_navBar.IsHiddenMode);
    }

    private void OnHiddenStateChanged(bool isHidden)
    {
        if (WindowService.IsCompactOverlayMode)
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenCompactOverlayMode;
        }
        else if (isHidden)
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarHidden;
        }
        else
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarNotHidden;
        }
    }

    private void OnSearchQueryChanged(string searchQuery)
    {
        ViewModel.SearchQuery = searchQuery;
        ViewModel.SearchBoxTextChangedCommand.Execute(null);
    }

    private void OnSearchQuerySubmitted(GuiToolViewItem? selectedItem)
    {
        ViewModel.SearchBoxQuerySubmittedCommand.Execute(selectedItem);
        // TODO: If succeeded, move the focus to the ToolPage.
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // Focus on the Search Box.
            _navBar.TryFocusSearchBoxAsync();

            // Start Smart Detection
            ViewModel.RunSmartDetectionAsync(WindowService.IsCompactOverlayMode).Forget();
        }

        if (IsTransitioning)
        {
            // This will force the page content to re-populate.
            IsTransitioning = false;
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }
}
