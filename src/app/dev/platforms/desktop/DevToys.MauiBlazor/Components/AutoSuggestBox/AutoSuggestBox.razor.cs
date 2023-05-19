namespace DevToys.MauiBlazor.Components;

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
    public bool IsEnabled { get; set; } = true;

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public EventCallback<string> OnQueryChanged { get; set; }

    internal ValueTask FocusAsync()
    {
        Guard.IsNotNull(_textBox);
        return _textBox.FocusAsync();
    }

    private Task OnTextBoxTextChangedAsync(string text)
    {
        return OnQueryChanged.InvokeAsync(text);
    }
}
