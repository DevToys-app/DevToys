namespace DevToys.Blazor.Components;

public partial class Dialog : JSStyledComponentBase, IFocusable
{
    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public bool Dismissible { get; set; }

    /// <summary>
    /// Raised when the dialog got dismissed.
    /// </summary>
    [Parameter]
    public EventCallback OnDismissed { get; set; }

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (IsOpen)
            {
                FocusAsync();
            }
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    private void OnDismiss()
    {
        if (Dismissible)
        {
            IsOpen = false;
            if (OnDismissed.HasDelegate)
            {
                OnDismissed.InvokeAsync();
            }
        }
    }
}
