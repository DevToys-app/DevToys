using System.Globalization;

namespace DevToys.Api;

/// <summary>
/// A component hosted by a <see cref="IUIGrid"/> that represents a cell in a flexible grid.
/// </summary>
public interface IUIGridCell
{
    /// <summary>
    /// Gets the row alignment of the cell.
    /// </summary>
    int Row { get; }

    /// <summary>
    /// Gets the column alignment of the cell.
    /// </summary>
    int Column { get; }

    /// <summary>
    /// Gets a value that indicates the total number of rows that the cell content spans within the parent grid.
    /// Default is 1.
    /// </summary>
    int RowSpan { get; }

    /// <summary>
    /// Gets a value that indicates the total number of columns that the cell content spans within the parent grid.
    /// Default is 1.
    /// </summary>
    int ColumnSpan { get; }

    /// <summary>
    /// Gets the child element to display in the cell.
    /// </summary>
    IUIElement? Child { get; }

    /// <summary>
    /// Raised when <see cref="Row"/> is changed.
    /// </summary>
    event EventHandler? RowChanged;

    /// <summary>
    /// Raised when <see cref="Column"/> is changed.
    /// </summary>
    event EventHandler? ColumnChanged;

    /// <summary>
    /// Raised when <see cref="RowSpan"/> is changed.
    /// </summary>
    event EventHandler? RowSpanChanged;

    /// <summary>
    /// Raised when <see cref="ColumnSpan"/> is changed.
    /// </summary>
    event EventHandler? ColumnSpanChanged;

    /// <summary>
    /// Raised when <see cref="Child"/> is changed.
    /// </summary>
    event EventHandler? ChildChanged;
}

[DebuggerDisplay($"Row = {{{nameof(Row)}}}, Column = {{{nameof(Column)}}}, RowSpan = {{{nameof(RowSpan)}}}, ColumnSpan = {{{nameof(ColumnSpan)}}}")]
internal sealed class UIGridCell : ViewModelBase, IUIGridCell
{
    private int _row;
    private int _column;
    private int _rowSpan = 1;
    private int _columnSpan = 1;
    private IUIElement? _child;

    public int Row
    {
        get => _row;
        internal set => SetPropertyValue(ref _row, value, RowChanged);
    }

    public int Column
    {
        get => _column;
        internal set => SetPropertyValue(ref _column, value, ColumnChanged);
    }

    public int RowSpan
    {
        get => _rowSpan;
        internal set
        {
            Guard.IsGreaterThanOrEqualTo(value, 1);
            SetPropertyValue(ref _rowSpan, value, RowSpanChanged);
        }
    }

    public int ColumnSpan
    {
        get => _columnSpan;
        internal set
        {
            Guard.IsGreaterThanOrEqualTo(value, 1);
            SetPropertyValue(ref _columnSpan, value, ColumnSpanChanged);
        }
    }

    public IUIElement? Child
    {
        get => _child;
        internal set => SetPropertyValue(ref _child, value, ChildChanged);
    }

    public event EventHandler? RowChanged;
    public event EventHandler? ColumnChanged;
    public event EventHandler? RowSpanChanged;
    public event EventHandler? ColumnSpanChanged;
    public event EventHandler? ChildChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component hosted by a <see cref="IUIGrid"/> that represents a cell in a flexible grid.
    /// </summary>
    public static IUIGridCell Cell()
    {
        return new UIGridCell();
    }

    /// <summary>
    /// Create a component hosted by a <see cref="IUIGrid"/> that represents a cell in a flexible grid.
    /// </summary>
    /// <param name="row">The row alignment of the cell.</param>
    /// <param name="column">The column alignment of the cell.</param>
    /// <param name="rowSpan">A value that indicates the total number of rows that the cell content spans within the parent grid.</param>
    /// <param name="columnSpan">A value that indicates the total number of columns that the cell content spans within the parent grid.</param>
    /// <param name="child">The child element to display in the cell.</param>
    public static IUIGridCell Cell(int row, int column, int rowSpan, int columnSpan, IUIElement? child = null)
    {
        return new UIGridCell()
            .Row(row)
            .Column(column)
            .RowSpan(rowSpan)
            .ColumnSpan(columnSpan)
            .WithChild(child);
    }

    /// <summary>
    /// Create a component hosted by a <see cref="IUIGrid"/> that represents a cell in a flexible grid.
    /// </summary>
    /// <param name="row">The row where the cell should be placed.</param>
    /// <param name="column">The column where the cell should be placed.</param>
    /// <param name="child">The child element to display in the cell.</param>
    public static IUIGridCell Cell<TRow, TColumn>(TRow row, TColumn column, IUIElement? child = null)
        where TRow : Enum
        where TColumn : Enum
    {
        int rowIndex = row.ToInt();
        int columnIndex = column.ToInt();

        return new UIGridCell()
            .Row(rowIndex)
            .Column(columnIndex)
            .WithChild(child);
    }

    /// <summary>
    /// Create a component hosted by a <see cref="IUIGrid"/> that represents a cell in a flexible grid.
    /// </summary>
    /// <param name="firstRow">The first row where the cell should appear.</param>
    /// <param name="lastRow">The last row where the cell should appear.</param>
    /// <param name="firstColumn">The first column where the cell should appear.</param>
    /// <param name="lastColumn">The last column where the cell should appear.</param>
    /// <param name="child">The child element to display in the cell.</param>
    public static IUIGridCell Cell<TRow, TColumn>(
        TRow firstRow,
        TRow lastRow,
        TColumn firstColumn,
        TColumn? lastColumn = default,
        IUIElement? child = null)
        where TRow : Enum
        where TColumn : Enum
    {
        int rowIndex = firstRow.ToInt();
        int rowSpan = lastRow.ToInt() - rowIndex + 1;
        int columnIndex = firstColumn.ToInt();
        int columnSpan = lastColumn is not null ? lastColumn.ToInt() + 1 - columnIndex : 1;

        return new UIGridCell()
            .Row(rowIndex)
            .RowSpan(rowSpan)
            .Column(columnIndex)
            .ColumnSpan(columnSpan)
            .WithChild(child);
    }

    /// <summary>
    /// Sets the row alignment of the cell.
    /// </summary>
    public static IUIGridCell Row(this IUIGridCell element, int row)
    {
        ((UIGridCell)element).Row = row;
        return element;
    }

    /// <summary>
    /// Sets the column alignment of the cell.
    /// </summary>
    public static IUIGridCell Column(this IUIGridCell element, int column)
    {
        ((UIGridCell)element).Column = column;
        return element;
    }

    /// <summary>
    /// Sets a value that indicates the total number of rows that the cell content spans within the parent grid.
    /// </summary>
    public static IUIGridCell RowSpan(this IUIGridCell element, int rows)
    {
        ((UIGridCell)element).RowSpan = rows;
        return element;
    }

    /// <summary>
    /// Sets a value that indicates the total number of columns that the cell content spans within the parent grid.
    /// </summary>
    public static IUIGridCell ColumnSpan(this IUIGridCell element, int columns)
    {
        ((UIGridCell)element).ColumnSpan = columns;
        return element;
    }

    /// <summary>
    /// Sets the child element to display in the cell.
    /// </summary>
    public static IUIGridCell WithChild(this IUIGridCell element, IUIElement? child)
    {
        ((UIGridCell)element).Child = child;
        return element;
    }

    private static int ToInt(this Enum enumValue) => Convert.ToInt32(enumValue, CultureInfo.InvariantCulture);
}
