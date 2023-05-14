namespace DevToys.MauiBlazor.Components;

public partial class NavBar : StyledComponentBase, IAsyncDisposable
{
    private const string JAVASCRIPT_FILE = "./Components/NavBar/NavBar.razor.js";

    private DotNetObjectReference<NavBar>? _objRef;
    private int _currentWidth;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference JSModule { get; set; } = default!;

    public string NavId { get; } = NewId();

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

    private NavBarState SidebarState { get; set; } = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objRef = DotNetObjectReference.Create(this);
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await JSModule.InvokeVoidAsync("registerResizeHandler", Id, _objRef);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public void OnComponentResize(int width)
    {
        bool stateChanged = SidebarState.WidthUpdated(width, NavBarWidthSidebarHiddenThreshold, NavBarWidthSidebarCollapseThreshold);
        if (stateChanged)
        {
            StateHasChanged();
        }
    }

    private void OnToggleSidebarClick()
    {
        SidebarState.ToggleSidebar();
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
        if (JSModule is not null)
        {
            await JSModule.DisposeAsync();
        }
    }
}
