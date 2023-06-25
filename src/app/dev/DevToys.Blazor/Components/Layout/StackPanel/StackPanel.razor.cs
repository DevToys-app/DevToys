﻿namespace DevToys.Blazor.Components;

public partial class StackPanel : StyledComponentBase
{
    protected string? ClassValue => new CssBuilder(FinalCssClasses)
        .AddClass("stack-horizontal", () => Orientation == Orientation.Horizontal)
        .AddClass("stack-vertical", () => Orientation == Orientation.Vertical)
        .Build();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("column-gap", HorizontalGap.ToPx(), () => HorizontalGap.HasValue)
        .AddStyle("row-gap", VerticalGap.ToPx(), () => VerticalGap.HasValue)
        .AddStyle("flex-wrap", "wrap", () => Wrap)
        .AddStyle(Style)
        .Build();

    /// <summary>
    /// Gets or set the orientation of the stacked components. 
    /// </summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// Gets or sets if the stack wraps.
    /// </summary>
    [Parameter]
    public bool Wrap { get; set; } = false;

    /// <summary>
    /// Gets or sets the gap between horizontally stacked components (in pixels).
    /// </summary>
    [Parameter]
    public int? HorizontalGap { get; set; } = 4;

    /// <summary>
    /// Gets or sets the gap between vertically stacked components (in pixels).
    /// </summary>
    [Parameter]
    public int? VerticalGap { get; set; } = 4;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
