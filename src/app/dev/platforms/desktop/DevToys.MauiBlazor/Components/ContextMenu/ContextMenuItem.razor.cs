namespace DevToys.MauiBlazor.Components;

public partial class ContextMenuItem : JSStyledComponentBase
{
    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public char IconGlyph { get; set; }

    [Parameter]
    public string IconFontFamily { get; set; } = "FluentSystemIcons";

    [Parameter]
    public string? KeyboardShortcut { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }

    private Task OnItemClickAsync()
    {
        return OnClick.InvokeAsync();
    }
}
