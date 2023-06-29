using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class Expander : StyledComponentBase
{
    public bool IsExpanded { get; set; } = false;

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

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private void OnClick()
    {
        IsExpanded = !IsExpanded;
    }

    protected void OnKeyDown(KeyboardEventArgs ev)
    {
        if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
        {
            OnClick();
        }
    }
}

