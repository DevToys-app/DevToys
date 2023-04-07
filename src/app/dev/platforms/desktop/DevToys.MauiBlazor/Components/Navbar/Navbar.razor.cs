using DevToys.MauiBlazor.Core.Helpers;

namespace DevToys.MauiBlazor.Components;

public partial class Navbar : MefLayoutComponentBase
{
    private readonly Dictionary<string, NavbarItem> items = new();

    private DotNetObjectReference<Navbar> _objectReference = default!;

    private bool IsResizeTrackingEnabled { get; set; } = false;

    private bool HasManualCollapsed { get; set; } = false;

    [Inject]
    private NavigationManager _navigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime _jSRuntime { get; set; } = default!;

    [Parameter]
    public bool IsCollapsible { get; set; } = false;

    [Parameter]
    public bool IsVertical { get; set; } = false;

    [Parameter]
    public bool HasSearch { get; set; } = false;

    [Parameter]
    public bool Collapsed { get; set; } = false;

    public string CollapsedName => Collapsed.ToString().ToLowerInvariant();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<NavbarItem> CurrentSelectedChanged { get; set; }

    [Parameter]
    public EventCallback<NavbarItem> OnSelectedChange { get; set; }

    [Parameter]
    public EventCallback<bool> OnExpandedChange { get; set; }

    [Parameter]
    public EventCallback<bool> OnExpanded { get; set; }

    [JSInvokable]
    public bool ToggleResizeTracking()
    {
        IsResizeTrackingEnabled = !IsResizeTrackingEnabled;
        return IsResizeTrackingEnabled;
    }

    [JSInvokable]
    public void OnWindowResize(string current)
    {
        if (!IsResizeTrackingEnabled)
        {
            return;
        }
        var currentBreakpoint = Breakpoint.FindByCode(current);
        if (currentBreakpoint.Value < Breakpoint.Wide.Value)
        {
            Collapsed = true;
            StateHasChanged();
        }
    }

    internal async Task OnClickAsync()
    {
        Collapsed = !Collapsed;
        HasManualCollapsed = !HasManualCollapsed;
        await InvokeAsync(StateHasChanged);
    }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("navbar");
        if (IsVertical)
        {
            helper.Append("navbar-vertical");
        }
        base.AppendClasses(helper);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _objectReference = DotNetObjectReference.Create(this);
            await _jSRuntime.InvokeVoidAsync("blazorExtensions.toggleResizeTracking", _objectReference);
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
