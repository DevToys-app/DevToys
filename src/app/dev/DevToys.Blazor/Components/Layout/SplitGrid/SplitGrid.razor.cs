namespace DevToys.Blazor.Components;

public partial class SplitGrid : JSStyledComponentBase
{
    private ElementReference _gutterElement;

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("display", "grid")
        .AddStyle("grid-template-columns", $"{GetCellLength(LeftOrTopCellSize)} 10px {GetCellLength(RightOrBottomCellSize)}", GutterOrientation == Orientation.Vertical)
        .AddStyle("grid-template-rows", $"{GetCellLength(LeftOrTopCellSize)} 10px {GetCellLength(RightOrBottomCellSize)}", GutterOrientation == Orientation.Horizontal)
        .AddStyle(Style)
        .Build();

    protected string? GutterStyle => new StyleBuilder()
        .AddStyle("cursor", "col-resize", GutterOrientation == Orientation.Vertical)
        .AddStyle("cursor", "row-resize", GutterOrientation == Orientation.Horizontal)
        .AddStyle("grid-column", "2", GutterOrientation == Orientation.Vertical)
        .AddStyle("grid-column", "1/-1", GutterOrientation == Orientation.Horizontal)
        .AddStyle("grid-row", "1/-1", GutterOrientation == Orientation.Vertical)
        .AddStyle("grid-row", "2", GutterOrientation == Orientation.Horizontal)
        .Build();

    /// <summary>
    /// Gets or sets the length of the left or top cell.
    /// </summary>
    [Parameter]
    public GridLength LeftOrTopCellSize { get; set; } = new GridLength(1, GridUnitType.Fraction);

    /// <summary>
    /// Gets or sets the length of the right or bottom cell.
    /// </summary>
    [Parameter]
    public GridLength RightOrBottomCellSize { get; set; } = new GridLength(1, GridUnitType.Fraction);

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? LeftOrTopCellContent { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? RightOrBottomCellContent { get; set; }

    /// <summary>
    /// Gets or set the orientation of the grid gutter.
    /// </summary>
    [Parameter]
    public Orientation GutterOrientation { get; set; } = Orientation.Vertical;

    /// <summary>
    /// Gets or set the minimum size of the cells.
    /// </summary>
    [Parameter]
    public int MinimumCellSize { get; set; }

    public override async ValueTask DisposeAsync()
    {
        await JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.SplitGrid.dispose", Element);
        await base.DisposeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await JSRuntime.InvokeVoidWithErrorHandlingAsync(
                "devtoys.SplitGrid.initializeSplitGrid",
                Element,
                _gutterElement,
                GutterOrientation == Orientation.Vertical,
                MinimumCellSize);
        }
    }

    private static string GetCellLength(GridLength length)
    {
        if (length.IsFraction)
        {
            return $"{length.Value}fr";
        }
        else if (length.IsAbsolute)
        {
            return length.Value.ToPx();
        }
        else
        {
            Guard.IsTrue(length.IsAuto);
            ThrowHelper.ThrowNotSupportedException($"{nameof(GridLength)}.{nameof(GridLength.Auto)} is not supported in {nameof(SplitGrid)}.");
            return string.Empty;
        }
    }
}
