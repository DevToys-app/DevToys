namespace DevToys.Blazor.Components;

public partial class FullScreenContainer : JSStyledComponentBase
{
    protected string FullScreenId => Id + "-full-screen";

    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/Layout/FullScreenContainer/FullScreenContainer.razor.js";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<bool> IsInFullScreenModeChanged { get; set; }

    public bool IsInFullScreenMode { get; private set; }

    internal async Task<bool> ToggleFullScreenModeAsync(string elementId)
    {
        if (!IsInFullScreenMode)
        {
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
        return IsInFullScreenMode;
    }
}
