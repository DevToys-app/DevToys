using System.Collections.ObjectModel;

namespace DevToys.Api;

/// <summary>
/// A component that represents a row in a <see cref="IUIDataGrid"/>.
/// </summary>
public interface IUIDataGridRow : ICollection<IUIDataGridCell>, IDataGridRow<IUIElement>
{
    /// <summary>
    /// Gets the value associated to the row.
    /// </summary>
    object? Value { get; }
}

[DebuggerDisplay($"Value = {{{nameof(Value)}}}")]
internal sealed class UIDataGridRow : ObservableCollection<IUIDataGridCell>, IUIDataGridRow
{
    internal UIDataGridRow(object? value)
    {
        Value = value;
    }

    public object? Value { get; }

    public IUIElement? Details { get; internal set; }
}

public static partial class GUI
{
    /// <summary>
    /// Create component that represents an item in a list.
    /// </summary>
    /// <param name="value">The value associated to the item.</param>
    /// <param name="cells">The cells to display in the row.</param>
    public static IUIDataGridRow Row(object? value, params IUIDataGridCell[] cells)
    {
        var row = new UIDataGridRow(value);
        row.AddRange(cells);
        return row;
    }

    /// <summary>
    /// Create component that represents an item in a list.
    /// </summary>
    /// <param name="value">The value associated to the item.</param>
    /// <param name="cells">The cells to display in the row.</param>
    public static IUIDataGridRow Row(object? value, params string[] cells)
    {
        var row = new UIDataGridRow(value);
        if (cells is not null)
        {
            var gridCells = new IUIDataGridCell[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                gridCells[i] = Cell(cells[i]);
            }
            row.AddRange(gridCells);
        }

        return row;
    }

    /// <summary>
    /// Create component that represents an item in a list.
    /// </summary>
    /// <param name="value">The value associated to the item.</param>
    /// <param name="details">The element to display as detail information in the row.</param>
    /// <param name="cells">The cells to display in the row.</param>
    public static IUIDataGridRow Row(object? value, IUIElement? details, params IUIDataGridCell[] cells)
    {
        var row = (UIDataGridRow)Row(value, cells);
        row.Details = details;
        return row;
    }

    /// <summary>
    /// Create component that represents an item in a list.
    /// </summary>
    /// <param name="value">The value associated to the item.</param>
    /// <param name="details">The element to display as detail information in the row.</param>
    /// <param name="cells">The cells to display in the row.</param>
    public static IUIDataGridRow Row(object? value, IUIElement? details, params string[] cells)
    {
        var row = (UIDataGridRow)Row(value, cells);
        row.Details = details;
        return row;
    }
}

