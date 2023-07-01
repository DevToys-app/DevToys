namespace DevToys.Blazor.Components;

public partial class ToggleSwitch : JSStyledComponentBase, IFocusable
{
    /// <summary>
    /// Gets or sets the text to display in the toggle switch.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets whether the toggle switch is on.
    /// </summary>
    [Parameter]
    public bool IsOn { get; set; }

    /// <summary>
    /// Raised when the toggle switch is toggled.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnCheckedChanged { get; set; }

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    private void OnChange(ChangeEventArgs ev)
    {
        bool value = (ev.Value as bool?) ?? false;
        if (value != IsOn)
        {
            IsOn = value;
            OnCheckedChanged.InvokeAsync(value);
        }
    }
}
