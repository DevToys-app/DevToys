namespace DevToys.Blazor.Components;

public partial class RadioButton : JSStyledComponentBase, IFocusable
{
    /// <summary>
    /// Gets or sets the text to display in the radio button.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the text to display in the check box.
    /// </summary>
    [Parameter]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets whether the radio button is checked.
    /// </summary>
    [Parameter]
    public bool IsChecked { get; set; }

    /// <summary>
    /// Raised when the radio button is toggled.
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
