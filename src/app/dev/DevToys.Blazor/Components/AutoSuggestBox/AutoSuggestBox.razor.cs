namespace DevToys.Blazor.Components;

public partial class AutoSuggestBox : StyledComponentBase
{
    private TextBox? _textBox = default!;

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Query { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public EventCallback<string> QueryChanged { get; set; }

    internal ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_textBox);
        return _textBox.FocusAsync();
    }

    private Task OnTextBoxTextChangedAsync(string text)
    {
        return QueryChanged.InvokeAsync(text);
    }
}
