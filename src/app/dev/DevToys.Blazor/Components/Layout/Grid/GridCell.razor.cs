using System.ComponentModel.DataAnnotations;

namespace DevToys.Blazor.Components;

public partial class GridCell : StyledComponentBase
{
    protected string? GridCellStyleValue => new StyleBuilder()
        .AddStyle("grid-column", $"{Column + 1}", ColumnSpan <= 1)
        .AddStyle("grid-row", $"{Row + 1}", RowSpan <= 1)
        .AddStyle("grid-column", $"{Column + 1} / span {ColumnSpan}", ColumnSpan > 1)
        .AddStyle("grid-row", $"{Row + 1} / span {RowSpan}", RowSpan > 1)
        .Build();

    /// <summary>
    /// Gets or sets the column alignment of an element when child layout is processed by a parent <see cref="Grid"/> layout container.
    /// </summary>
    [Parameter]
    [Range(0, int.MaxValue, ErrorMessage = $"{nameof(Column)} must be positive and zero-based.")]
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the row alignment of an element when child layout is processed by a parent <see cref="Grid"/> layout container.
    /// </summary>
    [Parameter]
    [Range(0, int.MaxValue, ErrorMessage = $"{nameof(Row)} must be positive and zero-based.")]
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates the total number of columns that the element content spans within a parent <see cref="Grid"/>.
    /// </summary>
    [Parameter]
    [Range(1, int.MaxValue, ErrorMessage = $"{nameof(ColumnSpan)} must be greater or equal to 1.")]
    public int ColumnSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value that indicates the total number of rows that the element content spans within a parent <see cref="Grid"/>.
    /// </summary>
    [Parameter]
    [Range(1, int.MaxValue, ErrorMessage = $"{nameof(RowSpan)} must be greater or equal to 1.")]
    public int RowSpan { get; set; } = 1;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
