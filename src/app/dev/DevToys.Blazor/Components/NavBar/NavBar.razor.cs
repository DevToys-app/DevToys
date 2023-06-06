using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class NavBar<TElement, TSearchElement>
    : JSStyledComponentBase
    where TElement : class
    where TSearchElement : class
{
    private readonly NavBarState _sidebarState = new();
    private AutoSuggestBox<TSearchElement> _autoSuggestBox = default!;

    public string NavId { get; } = NewId();

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/NavBar/NavBar.razor.js";

    [Parameter]
    public RenderFragment<TElement> NavBarItemTitleTemplate { get; set; } = default!;

    [Parameter]
    public RenderFragment<TElement> NavBarItemIconTemplate { get; set; } = default!;

    /// <summary>
    /// Gets or sets the minimum width at which the sidebar should get hidden.
    /// </summary>
    [Parameter]
    public int NavBarWidthSidebarHiddenThreshold { get; set; } = 640;

    /// <summary>
    /// Gets or sets the minimum width at which the sidebar should collapse.
    /// </summary>
    [Parameter]
    public int NavBarWidthSidebarCollapseThreshold { get; set; } = 1008;

    [Parameter]
    public ReadOnlyObservableCollection<TElement>? MenuItemsSource { get; set; }

    [Parameter]
    public ReadOnlyObservableCollection<TElement>? FooterMenuItemsSource { get; set; }

    [Parameter]
    public TElement? SelectedItem { get; set; }

    [Parameter]
    public EventCallback<TElement> SelectedItemChanged { get; set; }

    [Parameter]
    public bool CanGoBack { get; set; }

    [Parameter]
    public EventCallback OnBackButtonClicked { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the header to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Header { get; set; }

    /// <summary>
    /// Raised when the NavBar goes and leave the hidden state.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnHiddenStateChanged { get; set; }

    /// <summary>
    /// Raised when the text in the search bar changed.
    /// </summary>
    [Parameter]
    public EventCallback<string> SearchQueryChanged { get; set; }

    /// <summary>
    /// Raised when the user press Enter in the search box or explicitly select an item in the search result list.
    /// </summary>
    [Parameter]
    public EventCallback<TSearchElement?> SearchQuerySubmitted { get; set; }

    /// <summary>
    /// Gets or sets the placeholder to display in the search bar.
    /// </summary>
    [Parameter]
    public string SearchBarPlaceholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of items to display in the search bar's drop down list.
    /// </summary>
    [Parameter]
    public ICollection<TSearchElement>? SearchResultItems { get; set; }

    /// <summary>
    /// Gets or sets the template of search bar result item to use.
    /// </summary>
    [Parameter]
    public RenderFragment<TSearchElement> SearchResultItemTemplate { get; set; } = default!;

    public NavBar()
    {
        _sidebarState.IsHiddenChanged += SidebarState_IsHiddenChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await (await JSModule).InvokeVoidAsync("registerResizeHandler", Id, NavId, Reference);
        }
    }

    public override ValueTask DisposeAsync()
    {
        _sidebarState.IsHiddenChanged -= SidebarState_IsHiddenChanged;
        return base.DisposeAsync();
    }

    [JSInvokable]
    public void OnComponentResize(int width)
    {
        bool stateChanged = _sidebarState.WidthUpdated(width, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        if (stateChanged)
        {
            StateHasChanged();
        }
    }

    private void OnToggleSidebarClick()
    {
        _sidebarState.ToggleSidebar();
    }

    private void OnCloseExpandedOverlaySidebarClick(MouseEventArgs ev)
    {
        _sidebarState.CloseExpandedOverlay();
    }

    private void OnSearchButtonClick()
    {
        _sidebarState.ForceExpand();
        Task.Delay(200)
            .ContinueWith(t =>
            {
                InvokeAsync(() =>
                {
                    _autoSuggestBox.FocusAsync();
                });
            });
    }

    private Task OnItemSelectedAsync(TElement item)
    {
        SelectedItem = item;
        return SelectedItemChanged.InvokeAsync(item);
    }

    private void SidebarState_IsHiddenChanged(object? sender, EventArgs e)
    {
        if (OnHiddenStateChanged.HasDelegate)
        {
            OnHiddenStateChanged.InvokeAsync(_sidebarState.IsHidden);
        }
    }
}
