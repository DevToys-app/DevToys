using System.Globalization;

namespace DevToys.Api;

/// <summary>
/// A component provides a flexible grid area that consists of columns and rows.
/// Child elements of the Grid are measured and arranged according to their row/column assignments and other logic.
/// </summary>
public interface IUIGrid : IUIElementWithChildren
{
    /// <summary>
    /// Gets a value that indicates the space between rows.
    /// Default is <see cref="UISpacing.Small"/>.
    /// </summary>
    UISpacing RowSpacing { get; }

    /// <summary>
    /// Gets a value that indicates the space between columns.
    /// Default is <see cref="UISpacing.Small"/>.
    /// </summary>
    UISpacing ColumnSpacing { get; }

    /// <summary>
    /// Gets the definition of the rows in the grid.
    /// Default is a row that takes the whole height.
    /// </summary>
    UIGridLength[]? Rows { get; }

    /// <summary>
    /// Gets the definition of the columns in the grid.
    /// Default is a column that takes the whole width.
    /// </summary>
    UIGridLength[]? Columns { get; }

    /// <summary>
    /// Gets the cells in the grid.
    /// </summary>
    IUIGridCell[]? Cells { get; }

    /// <summary>
    /// Raised when <see cref="RowSpacing"/> is changed.
    /// </summary>
    event EventHandler? RowSpacingChanged;

    /// <summary>
    /// Raised when <see cref="ColumnSpacing"/> is changed.
    /// </summary>
    event EventHandler? ColumnSpacingChanged;

    /// <summary>
    /// Raised when <see cref="Rows"/> is changed.
    /// </summary>
    event EventHandler? RowsChanged;

    /// <summary>
    /// Raised when <see cref="Columns"/> is changed.
    /// </summary>
    event EventHandler? ColumnsChanged;

    /// <summary>
    /// Raised when <see cref="Cells"/> is changed.
    /// </summary>
    event EventHandler? CellsChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}")]
internal sealed class UIGrid : UIElementWithChildren, IUIGrid
{
    private UISpacing _rowSpacing = UISpacing.Small;
    private UISpacing _columnSpacing = UISpacing.Small;
    private UIGridLength[]? _rows;
    private UIGridLength[]? _columns;
    private IUIGridCell[]? _cells;

    internal UIGrid(string? id)
        : base(id)
    {
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (_cells is not null)
        {
            foreach (IUIGridCell cell in _cells)
            {
                if (cell.Child is not null)
                {
                    yield return cell.Child;
                }
            }
        }
    }

    public UISpacing RowSpacing
    {
        get => _rowSpacing;
        internal set => SetPropertyValue(ref _rowSpacing, value, RowSpacingChanged);
    }

    public UISpacing ColumnSpacing
    {
        get => _columnSpacing;
        internal set => SetPropertyValue(ref _columnSpacing, value, ColumnSpacingChanged);
    }

    public UIGridLength[]? Rows
    {
        get => _rows;
        internal set => SetPropertyValue(ref _rows, value, RowsChanged);
    }

    public UIGridLength[]? Columns
    {
        get => _columns;
        internal set => SetPropertyValue(ref _columns, value, ColumnsChanged);
    }

    public IUIGridCell[]? Cells
    {
        get => _cells;
        internal set => SetPropertyValue(ref _cells, value, CellsChanged);
    }

    public event EventHandler? RowSpacingChanged;
    public event EventHandler? ColumnSpacingChanged;
    public event EventHandler? RowsChanged;
    public event EventHandler? ColumnsChanged;
    public event EventHandler? CellsChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A grid length definition that automatically fits to its content.
    /// </summary>
    public static UIGridLength Auto => UIGridLength.Auto;

    /// <summary>
    /// A grid length definition that is sized as a weighted proportion of available space.
    /// </summary>
    public static UIGridLength Fraction(int value)
    {
        return new(value, UIGridUnitType.Fraction);
    }

    /// <summary>
    /// A component provides a flexible grid area that consists of columns and rows.
    /// Child elements of the Grid are measured and arranged according to their row/column assignments and other logic.
    /// </summary>
    public static IUIGrid Grid()
    {
        return Grid(null);
    }

    /// <summary>
    /// A component provides a flexible grid area that consists of columns and rows.
    /// Child elements of the Grid are measured and arranged according to their row/column assignments and other logic.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIGrid Grid(string? id)
    {
        return new UIGrid(id);
    }

    /// <summary>
    /// Sets the rows.
    /// </summary>
    public static IUIGrid Rows(this IUIGrid element, params UIGridLength[] heights)
    {
        ((UIGrid)element).Rows = heights;
        return element;
    }

    /// <summary>
    /// Sets the rows.
    /// </summary>
    public static IUIGrid Rows<TEnum>(this IUIGrid element, params (TEnum name, UIGridLength height)[] rows) where TEnum : Enum
    {
        var heights = new UIGridLength[rows.Length];

        for (int i = 0; i < rows.Length; i++)
        {
            if (i != Convert.ToInt32(rows[i].name, CultureInfo.InvariantCulture))
            {
                throw new ArgumentException(
                    $"Value of row name {rows[i].name} is not {i}. " +
                    "Rows must be defined with enum names whose values form the sequence 0,1,2,..."
                );
            }

            heights[i] = rows[i].height;
        }
        return Rows(element, heights);
    }

    /// <summary>
    /// Sets the columns.
    /// </summary>
    public static IUIGrid Columns(this IUIGrid element, params UIGridLength[] widths)
    {
        ((UIGrid)element).Columns = widths;
        return element;
    }

    /// <summary>
    /// Sets the columns.
    /// </summary>
    public static IUIGrid Columns<TEnum>(this IUIGrid element, params (TEnum name, UIGridLength width)[] columns) where TEnum : Enum
    {
        var widths = new UIGridLength[columns.Length];

        for (int i = 0; i < columns.Length; i++)
        {
            if (i != Convert.ToInt32(columns[i].name, CultureInfo.InvariantCulture))
            {
                throw new ArgumentException(
                    $"Value of column name {columns[i].name} is not {i}. " +
                    "Columns must be defined with enum names whose values form the sequence 0,1,2,..."
                );
            }

            widths[i] = columns[i].width;
        }
        return Columns(element, widths);
    }

    /// <summary>
    /// Sets the cells in the grid.
    /// </summary>
    public static IUIGrid Cells(this IUIGrid element, params IUIGridCell[] cells)
    {
        ((UIGrid)element).Cells = cells;
        return element;
    }

    /// <summary>
    /// Sets no spacing between rows.
    /// </summary>
    public static IUIGrid RowNoSpacing(this IUIGrid element)
    {
        ((UIGrid)element).RowSpacing = UISpacing.None;
        return element;
    }

    /// <summary>
    /// Sets a small spacing between rows.
    /// </summary>
    public static IUIGrid RowSmallSpacing(this IUIGrid element)
    {
        ((UIGrid)element).RowSpacing = UISpacing.Small;
        return element;
    }

    /// <summary>
    /// Sets a medium spacing between rows.
    /// </summary>
    public static IUIGrid RowMediumSpacing(this IUIGrid element)
    {
        ((UIGrid)element).RowSpacing = UISpacing.Medium;
        return element;
    }

    /// <summary>
    /// Sets a large spacing between rows.
    /// </summary>
    public static IUIGrid RowLargeSpacing(this IUIGrid element)
    {
        ((UIGrid)element).RowSpacing = UISpacing.Large;
        return element;
    }

    /// <summary>
    /// Sets no spacing between columns.
    /// </summary>
    public static IUIGrid ColumnNoSpacing(this IUIGrid element)
    {
        ((UIGrid)element).ColumnSpacing = UISpacing.None;
        return element;
    }

    /// <summary>
    /// Sets a small spacing between columns.
    /// </summary>
    public static IUIGrid ColumnSmallSpacing(this IUIGrid element)
    {
        ((UIGrid)element).ColumnSpacing = UISpacing.Small;
        return element;
    }

    /// <summary>
    /// Sets a medium spacing between columns.
    /// </summary>
    public static IUIGrid ColumnMediumSpacing(this IUIGrid element)
    {
        ((UIGrid)element).ColumnSpacing = UISpacing.Medium;
        return element;
    }

    /// <summary>
    /// Sets a large spacing between columns.
    /// </summary>
    public static IUIGrid ColumnLargeSpacing(this IUIGrid element)
    {
        ((UIGrid)element).ColumnSpacing = UISpacing.Large;
        return element;
    }
}
