namespace DevToys.Blazor.Components;

public partial class SplitGrid : JSStyledComponentBase
{
    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/Layout/SplitGrid/SplitGrid.razor.js";

    private ElementReference _gutterElement;

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("display", "grid")
        .AddStyle("grid-template-columns", $"{GetCellLength(LeftOrTopCellSize)} 16px {GetCellLength(RightOrBottomCellSize)}", GutterOrientation == UIOrientation.Vertical)
        .AddStyle("grid-template-rows", $"{GetCellLength(LeftOrTopCellSize)} 16px {GetCellLength(RightOrBottomCellSize)}", GutterOrientation == UIOrientation.Horizontal)
        .AddStyle(Style)
        .Build();

    protected string? GutterStyle => new StyleBuilder()
        .AddStyle("cursor", "col-resize", GutterOrientation == UIOrientation.Vertical)
        .AddStyle("cursor", "row-resize", GutterOrientation == UIOrientation.Horizontal)
        .AddStyle("grid-column", "2", GutterOrientation == UIOrientation.Vertical)
        .AddStyle("grid-column", "1/-1", GutterOrientation == UIOrientation.Horizontal)
        .AddStyle("grid-row", "1/-1", GutterOrientation == UIOrientation.Vertical)
        .AddStyle("grid-row", "2", GutterOrientation == UIOrientation.Horizontal)
        .Build();

    protected char GripperGlyph => GutterOrientation == UIOrientation.Vertical ? '\uE9F9' : '\uE9F6';

    /// <summary>
    /// Gets or sets the length of the left or top cell.
    /// </summary>
    [Parameter]
    public UIGridLength LeftOrTopCellSize { get; set; } = new UIGridLength(1, UIGridUnitType.Fraction);

    /// <summary>
    /// Gets or sets the length of the right or bottom cell.
    /// </summary>
    [Parameter]
    public UIGridLength RightOrBottomCellSize { get; set; } = new UIGridLength(1, UIGridUnitType.Fraction);

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
    public UIOrientation GutterOrientation { get; set; } = UIOrientation.Vertical;

    /// <summary>
    /// Gets or set the minimum size of the cells.
    /// </summary>
    [Parameter]
    public int MinimumCellSize { get; set; }

    public override async ValueTask DisposeAsync()
    {
        await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", Element);
        await base.DisposeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                await (await JSModule).InvokeVoidWithErrorHandlingAsync(
                     "initializeSplitGrid",
                     Element,
                     _gutterElement,
                     GutterOrientation == UIOrientation.Vertical,
                     MinimumCellSize);
            }
        }
    }

    private static string GetCellLength(UIGridLength length)
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
            ThrowHelper.ThrowNotSupportedException($"{nameof(UIGridLength)}.{nameof(UIGridLength.Auto)} is not supported in {nameof(SplitGrid)}.");
            return string.Empty;
        }
    }
}
