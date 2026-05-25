namespace DevToys.Blazor.Components;

public partial class StackPanel : StyledComponentBase
{
    protected string? ClassValue => new CssBuilder(FinalCssClasses)
        .AddClass("stack-horizontal", () => Orientation == UIOrientation.Horizontal)
        .AddClass("stack-vertical", () => Orientation == UIOrientation.Vertical)
        .Build();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("column-gap", Spacing.ToPx())
        .AddStyle("row-gap", Spacing.ToPx())
        .AddStyle("flex-wrap", "wrap", () => Wrap)
        .AddStyle("height", "inherit", () => UseMaxHeight)
        .AddStyle(Style)
        .Build();

    /// <summary>
    /// Gets or set the orientation of the stacked components. 
    /// </summary>
    [Parameter]
    public UIOrientation Orientation { get; set; } = UIOrientation.Horizontal;

    /// <summary>
    /// Gets or sets if the stack wraps.
    /// </summary>
    [Parameter]
    public bool Wrap { get; set; } = false;

    /// <summary>
    /// Gets or sets the space between stacked components (in pixels).
    /// </summary>
    [Parameter]
    public int Spacing { get; set; } = 4;

    /// <summary>
    /// Gets or sets if the stack need to use the full available height.
    /// </summary>
    [Parameter]
    public bool UseMaxHeight { get; set; } = false;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
