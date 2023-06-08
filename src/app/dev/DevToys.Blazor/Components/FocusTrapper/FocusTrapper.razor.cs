using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class FocusTrapper : JSStyledComponentBase
{
    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/FocusTrapper/FocusTrapper.razor.js";

    [Parameter]
    public EventCallback OnEscapeKeyPressed { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await (await JSModule).InvokeVoidAsync("initializeFocusTracking", Element);
        }
    }

    private void OnKeyDown(KeyboardEventArgs ev)
    {
        if (OnEscapeKeyPressed.HasDelegate && string.Equals(ev.Key, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            OnEscapeKeyPressed.InvokeAsync();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await (await JSModule).InvokeVoidAsync("dispose", Element);
        await base.DisposeAsync();
    }
}
