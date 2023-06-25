namespace DevToys.Blazor.Components;

public partial class Card : StyledComponentBase
{
    [Parameter]
    public char IconGlyph { get; set; }

    [Parameter]
    public string IconFontFamily { get; set; } = "FluentSystemIcons";

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public RenderFragment? Control { get; set; }
}

