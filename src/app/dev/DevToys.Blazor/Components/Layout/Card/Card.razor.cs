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

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    protected Expander? ParentExpander { get; set; }

    protected bool IsChildOfExpander => ParentExpander is not null;
}

