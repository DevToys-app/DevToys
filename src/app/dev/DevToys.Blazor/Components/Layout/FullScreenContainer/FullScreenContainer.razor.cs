namespace DevToys.Blazor.Components;

public partial class FullScreenContainer : JSStyledComponentBase
{
    protected string FullScreenId => Id + "-full-screen";

    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/Layout/FullScreenContainer/FullScreenContainer.razor.js";

    private string _fullScreenElementId = string.Empty;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<bool> IsInFullScreenModeChanged { get; set; }

    public bool IsInFullScreenMode { get; private set; }

    internal async Task<bool> ToggleFullScreenModeAsync(string elementId, IFocusable? elementToGiveFocusBack)
    {
        if (!IsInFullScreenMode)
        {
            _fullScreenElementId = elementId;
            await (await JSModule).InvokeVoidWithErrorHandlingAsync("setToFullScreen", FullScreenId, elementId);
            IsInFullScreenMode = true;
        }
        else
        {
            await (await JSModule).InvokeVoidWithErrorHandlingAsync("restoreFromFullScreen", elementId);
            IsInFullScreenMode = false;
        }

        if (IsInFullScreenModeChanged.HasDelegate)
        {
            await IsInFullScreenModeChanged.InvokeAsync(IsInFullScreenMode);
        }
        StateHasChanged();

        if (elementToGiveFocusBack is not null)
        {
            // We just changed the DOM. Let's make sure that the calling element
            // gets back the focus.
            await elementToGiveFocusBack.FocusAsync();
        }

        return IsInFullScreenMode;
    }

    internal async Task ForceQuitFullScreenModeAsync()
    {
        if (IsInFullScreenMode)
        {
            await ToggleFullScreenModeAsync(_fullScreenElementId, null);
        }
    }
}
