using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Utilities;

// Remember to replace the namespace below with your own project's namespace..
namespace DevToys.MauiBlazor.Components;

/// <summary>
/// Determines the horizontal alignment of the content within the <see cref="StackPanel"/>.
/// </summary>
public enum StackHorizontalAlignment
{
    /// <summary>
    /// The content is aligned to the left.
    /// </summary>
    Left,

    /// <summary>
    /// The content is center aligned.
    /// </summary>
    Center,

    /// <summary>
    /// The content is aligned to the right.
    /// </summary>
    Right,
}

/// <summary>
/// Determines the vertical alignment of the content within the <see cref="StackPanel"/>.
/// </summary>
public enum StackVerticalAlignment
{
    /// <summary>
    /// The content is aligned to the top.
    /// </summary>
    Top,

    /// <summary>
    /// The content is center aligned.
    /// </summary>
    Center,

    /// <summary>
    /// The content is aligned to the bottom
    /// </summary>
    Bottom,
}

public partial class StackPanel : FluentComponentBase
{
    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("stack-horizontal", () => Orientation == Orientation.Horizontal)
        .AddClass("stack-vertical", () => Orientation == Orientation.Vertical)
        .Build();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("align-items", GetHorizontalAlignment(), () => Orientation == Orientation.Vertical)
        .AddStyle("justify-content", GetVerticalAlignment(), () => Orientation == Orientation.Vertical)

        .AddStyle("justify-content", GetHorizontalAlignment(), () => Orientation == Orientation.Horizontal)
        .AddStyle("align-items", GetVerticalAlignment(), () => Orientation == Orientation.Horizontal)

        .AddStyle("column-gap", $"{HorizontalGap}px", () => HorizontalGap.HasValue)
        .AddStyle("row-gap", $"{VerticalGap}px", () => VerticalGap.HasValue)
        .AddStyle("width", Width, () => !string.IsNullOrEmpty(Width))
        .AddStyle("flex-wrap", "wrap", () => Wrap)

        .AddStyle(Style)
        .Build();

    /// <summary>
    /// The horizontal alignment of the components in the stack. 
    /// </summary>
    [Parameter]
    public StackHorizontalAlignment HorizontalAlignment { get; set; } = StackHorizontalAlignment.Left;

    /// <summary>
    /// The vertical alignment of the components in the stack.
    /// </summary>
    [Parameter]
    public StackVerticalAlignment VerticalAlignment { get; set; } = StackVerticalAlignment.Top;

    /// <summary>
    /// Gets or set the orientation of the stacked components. 
    /// </summary>
    [Parameter]
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// The width of the stack as a percentage string (default = 100%).
    /// </summary>
    [Parameter]
    public string? Width { get; set; } = "100%";

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

    private string GetHorizontalAlignment()
    {
        return HorizontalAlignment switch
        {
            StackHorizontalAlignment.Left => "start",
            StackHorizontalAlignment.Center => "center",
            StackHorizontalAlignment.Right => "end",
            _ => "start",
        };
    }

    private string GetVerticalAlignment()
    {
        return VerticalAlignment switch
        {
            StackVerticalAlignment.Top => "start",
            StackVerticalAlignment.Center => "center",
            StackVerticalAlignment.Bottom => "end",
            _ => "start",
        };
    }
}
