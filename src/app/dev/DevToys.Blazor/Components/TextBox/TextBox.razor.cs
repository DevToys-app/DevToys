namespace DevToys.Blazor.Components;

public partial class TextBox : JSStyledComponentBase
{
    private readonly string _contextMenuId = NewId();
    private ElementReference _input;

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public bool IsEnabled { get; set; } = true;

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public TextBoxTypes Type { get; set; } = TextBoxTypes.Text;

    [Parameter]
    public RenderFragment? Buttons { get; set; }

    [Parameter]
    public EventCallback<string> OnTextChanged { get; set; }

    internal ValueTask FocusAsync()
    {
        return JSRuntime.InvokeVoidAsync("devtoys.DOM.setFocus", _input);
    }

    private void OnClearClick()
    {
        Text = string.Empty;
        FocusAsync();
    }

    private Task InputTextChangedAsync(ChangeEventArgs e)
    {
        return OnTextChanged.InvokeAsync(e.Value as string);
    }
}
