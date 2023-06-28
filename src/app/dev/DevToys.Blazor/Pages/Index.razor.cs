using DevToys.Api;
using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages;

public partial class Index : MefComponentBase
{
    private const int TitleBarMarginLeftWhenNavBarHidden = 90;
    private const int TitleBarMarginLeftWhenNavBarNotHidden = 47;

    private NavBar<INotifyPropertyChanged, GuiToolViewItem> _navBar = default!;

    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Import]
    internal GuiToolProvider GuiToolProvider { get; set; } = default!;

    [Import]
    internal TitleBarMarginProvider TitleBarMarginProvider { get; set; } = default!;

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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.SelectedMenuItemChanged += ViewModel_SelectedMenuItemChanged;
        ViewModel.SelectedMenuItem = ViewModel.HeaderAndBodyToolViewItems[0];
        ContextMenuService.IsContextMenuOpenedChanged += ContextMenuService_IsContextMenuOpenedChanged;
        WindowService.WindowActivated += WindowService_WindowActivated;
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

    private void WindowService_WindowActivated(object? sender, EventArgs e)
    {
        // Start Smart Detection
        ViewModel.RunSmartDetectionAsync(WindowService.IsOverlayMode).Forget();
    }

    private void OnBackButtonClicked()
    {
        ViewModel.GoBack();
    }

    private void OnHiddenStateChanged(bool isHidden)
    {
        if (isHidden)
        {
            TitleBarMarginProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarHidden;
        }
        else
        {
            TitleBarMarginProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarNotHidden;
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
    }

    internal void OnSetFavorite()
    {
        // WARNING: This should be done in ToolPageViewModel. Doing it here just for demo.
        if (ViewModel.SelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            bool isFavorite = GuiToolProvider.GetToolIsFavorite(guiToolViewItem.ToolInstance);
            GuiToolProvider.SetToolIsFavorite(guiToolViewItem.ToolInstance, !isFavorite);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // Focus on the Search Box.
            _navBar.TryFocusSearchBoxAsync();

            // Start Smart Detection
            ViewModel.RunSmartDetectionAsync(WindowService.IsOverlayMode).Forget();
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
