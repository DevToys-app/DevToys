namespace DevToys.MauiBlazor.Components;

public partial class TextBox : StyledComponentBase
{
    private ElementReference _input;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

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

    internal ValueTask FocusAsync()
    {
        return JSRuntime.InvokeVoidAsync("devtoys.DOM.setFocus", _input);
    }

    private void OnClearClick()
    {
        Text = string.Empty;
        FocusAsync();
    }
}
