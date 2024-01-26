using System.Collections.ObjectModel;

namespace DevToys.Api;

/// <summary>
/// A component that represents a grid that can display data with rows and columns.
/// </summary>
public interface IUIDataGrid : IUITitledElementWithChildren
{
    /// <summary>
    /// Gets the title of each column in the data grid.
    /// </summary>
    string[] Columns { get; }

    /// <summary>
    /// Gets the list of rows displayed in the data grid.
    /// </summary>
    ObservableCollection<IUIDataGridRow> Rows { get; }

    /// <summary>
    /// Gets whether rows are selectable in the data grid. Default is true.
    /// </summary>
    bool CanSelectRow { get; }

    /// <summary>
    /// Gets the selected row in the data grid.
    /// </summary>
    IUIDataGridRow? SelectedRow { get; }

    /// <summary>
    /// Gets the action to run when the user selects an row in the data grid.
    /// </summary>
    Func<IUIDataGridRow?, ValueTask>? OnRowSelectedAction { get; }

    /// <summary>
    /// Gets whether the element can be expanded to take the size of the whole tool boundaries. Default is false.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    bool IsExtendableToFullScreen { get; }

    /// <summary>
    /// Gets an extra interactive content to display in the command bar of the text input.
    /// </summary>
    IUIElement? CommandBarExtraContent { get; }

    /// <summary>
    /// Raised when <see cref="CanSelectRow"/> is changed.
    /// </summary>
    event EventHandler? CanSelectRowChanged;

    /// <summary>
    /// Raised when <see cref="SelectedRow"/> is changed.
    /// </summary>
    event EventHandler? SelectedRowChanged;

    /// <summary>
    /// Raised when <see cref="IsExtendableToFullScreen"/> is changed.
    /// </summary>
    event EventHandler? IsExtendableToFullScreenChanged;

    /// <summary>
    /// Raised when <see cref="CommandBarExtraContent"/> is changed.
    /// </summary>
    event EventHandler? CommandBarExtraContentChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, SelectedRow = {{{nameof(SelectedRow)}}}")]
internal sealed class UIDataGrid : UITitledElementWithChildren, IUIDataGrid, IDisposable
{
    private readonly ObservableCollection<IUIDataGridRow> _rows = new();
    private string[] _columns = Array.Empty<string>();
    private IUIDataGridRow? _selectedRow;
    private bool _canSelectRow = true;
    private bool _isExtendableToFullScreen;
    private IUIElement? _commandBarExtraContent;

    internal UIDataGrid(string? id)
        : base(id)
    {
        _rows.CollectionChanged += Rows_CollectionChanged;
    }

    public string[] Columns
    {
        get => _columns;
        internal set => SetPropertyValue(ref _columns, value, ColumnsChanged);
    }

    public ObservableCollection<IUIDataGridRow> Rows => _rows;

    public bool CanSelectRow
    {
        get => _canSelectRow;
        internal set
        {
            SetPropertyValue(ref _canSelectRow, value, CanSelectRowChanged);
            if (!value)
            {
                SelectedRow = null;
            }
        }
    }

    public IUIDataGridRow? SelectedRow
    {
        get => _selectedRow;
        internal set
        {
            if (_selectedRow != value)
            {
                _selectedRow = value;
                OnRowSelectedAction?.Invoke(_selectedRow);
                SelectedRowChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }
    }

    public bool IsExtendableToFullScreen
    {
        get => _isExtendableToFullScreen;
        internal set => SetPropertyValue(ref _isExtendableToFullScreen, value, IsExtendableToFullScreenChanged);
    }

    public IUIElement? CommandBarExtraContent
    {
        get => _commandBarExtraContent;
        internal set => SetPropertyValue(ref _commandBarExtraContent, value, CommandBarExtraContentChanged);
    }

    public Func<IUIDataGridRow?, ValueTask>? OnRowSelectedAction { get; internal set; }

    public event EventHandler? CanSelectRowChanged;
    public event EventHandler? SelectedRowChanged;
    public event EventHandler? ColumnsChanged;
    public event EventHandler? IsExtendableToFullScreenChanged;
    public event EventHandler? CommandBarExtraContentChanged;

    public void Dispose()
    {
        _rows.CollectionChanged -= Rows_CollectionChanged;
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        IUIDataGridRow[] rows = _rows.ToArray();
        for (int i = 0; i < rows.Length; i++)
        {
            IUIDataGridRow row = rows[i];
            IUIDataGridCell[] cells = row.ToArray();
            for (int j = 0; j < cells.Length; j++)
            {
                IUIDataGridCell cell = cells[j];
                if (cell.UIElement is not null)
                {
                    yield return cell.UIElement;
                }
            }

            if (row.Details is not null)
            {
                yield return row.Details;
            }
        }
    }

    private void Rows_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Try to preserve the selection.
        this.Select(SelectedRow);
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a grid that can display data in with rows and columns.
    /// </summary>
    public static IUIDataGrid DataGrid()
    {
        return DataGrid(null);
    }

    /// <summary>
    /// A component that represents a grid that can display data in with rows and columns.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIDataGrid DataGrid(string? id)
    {
        return new UIDataGrid(id);
    }

    /// <summary>
    /// Sets the title of each column in the data grid.
    /// </summary>
    public static IUIDataGrid WithColumns(this IUIDataGrid element, params string[] columns)
    {
        var dataGrid = (UIDataGrid)element;
        dataGrid.Columns = columns ?? Array.Empty<string>();

        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDataGrid.Rows"/> of the data grid.
    /// </summary>
    public static IUIDataGrid WithRows(this IUIDataGrid element, params IUIDataGridRow[] rows)
    {
        var dataGrid = (UIDataGrid)element;
        dataGrid.Rows.Clear();
        dataGrid.Rows.AddRange(rows);

        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the data grid.
    /// </summary>
    public static IUIDataGrid OnRowSelected(this IUIDataGrid element, Func<IUIDataGridRow?, ValueTask>? onRowSelectedAction)
    {
        ((UIDataGrid)element).OnRowSelectedAction = onRowSelectedAction;
        return element;
    }

    /// <summary>
    /// Sets the action to run when selecting an item in the data grid.
    /// </summary>
    public static IUIDataGrid OnRowSelected(this IUIDataGrid element, Action<IUIDataGridRow?>? onRowSelectedAction)
    {
        ((UIDataGrid)element).OnRowSelectedAction
            = (value) =>
            {
                onRowSelectedAction?.Invoke(value);
                return ValueTask.CompletedTask;
            };
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDataGridRow"/> that should be selected in the data grid.
    /// If <paramref name="row"/> is null or does not exist in the data grid, no row will be selected.
    /// </summary>
    public static IUIDataGrid Select(this IUIDataGrid element, IUIDataGridRow? row)
    {
        var dataGrid = (UIDataGrid)element;
        if ((row is not null
            && dataGrid.Rows is not null
            && !dataGrid.Rows.Contains(row))
            || !dataGrid.CanSelectRow)
        {
            dataGrid.SelectedRow = null;
        }
        else
        {
            dataGrid.SelectedRow = row;
        }

        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIDataGridRow"/> that should be selected in the data grid, using its index in the data grid.
    /// If <paramref name="index"/> smaller or greater than the amount of rows in the data grid, no row will be selected.
    /// </summary>
    public static IUIDataGrid Select(this IUIDataGrid element, int index)
    {
        var dataGrid = (UIDataGrid)element;

        if (dataGrid.Rows is null
            || index < 0
            || index > dataGrid.Rows.Count - 1
            || !dataGrid.CanSelectRow)
        {
            dataGrid.SelectedRow = null;
        }
        else
        {
            dataGrid.SelectedRow = dataGrid.Rows[index];
        }

        return element;
    }

    /// <summary>
    /// Allows the user to select a row in the data grid.
    /// </summary>
    public static IUIDataGrid AllowSelectItem(this IUIDataGrid element)
    {
        ((UIDataGrid)element).CanSelectRow = true;
        return element;
    }

    /// <summary>
    /// Prevents the user from selecting a row in the data grid.
    /// </summary>
    public static IUIDataGrid ForbidSelectItem(this IUIDataGrid element)
    {
        ((UIDataGrid)element).CanSelectRow = false;
        return element;
    }

    /// <summary>
    /// Indicates that the control can be extended to take the size of the whole tool boundaries.
    /// </summary>
    /// <remarks>
    /// When <see cref="IUIElement.IsVisible"/> is false and that the element is in full screen mode, the element goes back to normal mode.
    /// </remarks>
    public static IUIDataGrid Extendable(this IUIDataGrid element)
    {
        ((UIDataGrid)element).IsExtendableToFullScreen = true;
        return element;
    }

    /// <summary>
    /// Indicates that the control can not be extended to take the size of the whole tool boundaries.
    /// </summary>
    public static IUIDataGrid NotExtendable(this IUIDataGrid element)
    {
        ((UIDataGrid)element).IsExtendableToFullScreen = false;
        return element;
    }

    /// <summary>
    /// Defines an additional element to display in the command bar.
    /// </summary>
    public static IUIDataGrid CommandBarExtraContent(this IUIDataGrid element, IUIElement? extraElement)
    {
        ((UIDataGrid)element).CommandBarExtraContent = extraElement;
        return element;
    }
}
