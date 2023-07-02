namespace DevToys.Blazor.Components;

public partial class CheckBox : JSStyledComponentBase, IFocusable
{
    /// <summary>
    /// Gets or sets the text to display in the check box.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets whether the check box is checked.
    /// </summary>
    [Parameter]
    public bool IsChecked { get; set; }

    /// <summary>
    /// Raised when the check box is toggled.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsCheckedChanged { get; set; }

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    private void OnChange(ChangeEventArgs ev)
    {
        bool value = (ev.Value as bool?) ?? false;
        if (value != IsChecked)
        {
            IsChecked = value;
            IsCheckedChanged.InvokeAsync(value);
        }
    }
}
