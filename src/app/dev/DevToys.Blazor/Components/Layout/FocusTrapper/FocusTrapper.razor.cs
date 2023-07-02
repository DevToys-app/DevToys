using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class FocusTrapper : JSStyledComponentBase, IFocusable
{
    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/Layout/FocusTrapper/FocusTrapper.razor.js";

    [Parameter]
    public EventCallback OnEscapeKeyPressed { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    public override async ValueTask DisposeAsync()
    {
        await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", Element);
        await base.DisposeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeFocusTracking", Element);
            }
        }
    }

    private void OnKeyDown(KeyboardEventArgs ev)
    {
        if (OnEscapeKeyPressed.HasDelegate && string.Equals(ev.Code, "Escape", StringComparison.OrdinalIgnoreCase))
        {
            OnEscapeKeyPressed.InvokeAsync();
        }
    }
}
