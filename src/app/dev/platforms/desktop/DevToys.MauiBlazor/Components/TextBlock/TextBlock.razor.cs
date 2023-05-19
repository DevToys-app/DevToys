using Microsoft.AspNetCore.Components.Rendering;

namespace DevToys.MauiBlazor.Components;

public class TextBlock : StyledComponentBase
{
    [Parameter]
    public TextBlockAppearance Appearance { get; set; } = TextBlockAppearance.Body;

    [Parameter]
    public bool NoWrap { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, Appearance.Tag);
        builder.AddAttribute(1, "id", Id);
        builder.AddAttribute(2, "class", $"text-block type-{Appearance.Class} {(NoWrap ? "no-wrap" : string.Empty)} {FinalCssClasses}");
        builder.AddAttribute(3, "style", Style);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
