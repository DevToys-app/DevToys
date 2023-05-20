﻿using System.Collections.ObjectModel;

namespace DevToys.MauiBlazor.Components;

public partial class NavBar<TElement> : JSStyledComponentBase where TElement : class
{

    private readonly NavBarState _sidebarState = new();
    private DotNetObjectReference<NavBar<TElement>>? _objRef;
    private AutoSuggestBox _autoSuggestBox = default!;

    public string NavId { get; } = NewId();

    protected override string? JavaScriptFile => "./Components/NavBar/NavBar.razor.js";

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
            await (await JSModule).InvokeVoidAsync("registerResizeHandler", Id, NavId, _objRef);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public override ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
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

    private void OnCloseExpandedOverlaySidebarClick()
    {
        _sidebarState.CloseExpandedOverlay();
    }

    private void OnSearchButtonClick()
    {
        _sidebarState.ToggleSidebar();
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
}
