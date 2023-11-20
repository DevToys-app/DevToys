using System.Collections.Specialized;
using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class DataGrid<TRow, TCell, TRowDetail> : JSStyledComponentBase, IFocusable
    where TRow : class, ICollection<TCell>, IDataGridRow<TRowDetail>
    where TCell : class
    where TRowDetail : class
{
    private readonly Lazy<DataGridColumnResizeService<TRow, TCell, TRowDetail>> _dataGridColumnResizeService;
    private readonly List<DataGridColumnHeader<TRow, TCell, TRowDetail>> _columns = new();

    private int _oldSelectedIndex = int.MinValue;
    private TRow? _oldSelectedItem = default;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/Collections/DataGrid/DataGrid.razor.js";

    [Inject]
    internal DocumentEventService DocumentEventService { get; set; } = default!;

    [Parameter]
    public string[]? Columns { get; set; }

    [Parameter]
    public IList<TRow>? Rows { get; set; }

    [Parameter]
    public RenderFragment<TCell> CellTemplate { get; set; } = default!;

    [Parameter]
    public RenderFragment<TRowDetail> RowDetailTemplate { get; set; } = default!;

    [Parameter]
    public TRow? SelectedRow { get; set; }

    [Parameter]
    public int SelectedIndex { get; set; } = -1;

    [Parameter]
    public EventCallback<int> SelectedIndexChanged { get; set; }

    [Parameter]
    public EventCallback<TRow> SelectedRowChanged { get; set; }

    [Parameter]
    public bool CanSelectRow { get; set; } = true;

    [Parameter]
    public bool Virtualize { get; set; } = true;

    [Parameter]
    public virtual bool RaiseSelectionEventOnKeyboardNavigation { get; set; } = true;

    internal bool IsResizing { get; set; }

    public DataGrid()
    {
        _dataGridColumnResizeService = new(() =>
        {
            Guard.IsNotNull(DocumentEventService);
            return new DataGridColumnResizeService<TRow, TCell, TRowDetail>(this, DocumentEventService);
        });
    }

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    internal void AddColumn(DataGridColumnHeader<TRow, TCell, TRowDetail> dataGridColumnHeader)
    {
        _columns.Add(dataGridColumnHeader);
    }

    internal void RemoveColumn(DataGridColumnHeader<TRow, TCell, TRowDetail> dataGridColumnHeader)
    {
        _columns.Remove(dataGridColumnHeader);
    }

    internal async Task<double> GetActualHeightAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(Element);
            return await (await JSModule).InvokeAsync<double>("getActualHeight", Element);
        }
    }

    internal Task<bool> StartResizeColumn(DataGridColumnHeader<TRow, TCell, TRowDetail> dataGridColumnHeader, double clientX)
    {
        return _dataGridColumnResizeService.Value.StartResizeColumn(dataGridColumnHeader, clientX, _columns, isContainerResizeMode: false);
    }

    internal void SelectNextRow()
    {
        if (Rows is not null)
        {
            if (SelectedIndex == -1 || SelectedIndex == Rows.Count - 1)
            {
                SetSelectedIndex(0, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
            else
            {
                SetSelectedIndex(SelectedIndex + 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
        }
    }

    internal void SelectPreviousRow()
    {
        if (Rows is not null)
        {
            if (SelectedIndex == -1 || SelectedIndex == 0)
            {
                SetSelectedIndex(Rows.Count - 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
            else
            {
                SetSelectedIndex(SelectedIndex - 1, raiseEvent: RaiseSelectionEventOnKeyboardNavigation);
            }
        }
    }

    internal void SetSelectedIndex(int index)
    {
        SetSelectedIndex(index, raiseEvent: false);
    }

    internal void SetSelectedRow(TRow? row)
    {
        SetSelectedRow(row, raiseEvent: false);
    }

    public override ValueTask DisposeAsync()
    {
        if (Rows is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnItemsChanged;
        }
        return base.DisposeAsync();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_oldSelectedIndex != SelectedIndex)
        {
            SetSelectedIndex(SelectedIndex, raiseEvent: false);
        }
        else if (_oldSelectedItem != SelectedRow)
        {
            SetSelectedRow(SelectedRow, raiseEvent: false);
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        if (firstRender)
        {
            if (Rows is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += OnItemsChanged;
            }
        }
    }

    private void SetSelectedIndex(int index, bool raiseEvent)
    {
        if (!CanSelectRow)
        {
            return;
        }

        if (index >= Rows?.Count)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(index));
        }

        SelectedIndex = index;
        _oldSelectedIndex = index;
        if (index > -1)
        {
            Guard.IsNotNull(Rows);
            SelectedRow = Rows.ElementAt(index);
        }
        else
        {
            SelectedRow = default;
        }
        _oldSelectedItem = SelectedRow;

        if (raiseEvent)
        {
            RaiseOnSelectionChanged();
        }

        StateHasChanged();
    }

    private void SetSelectedRow(TRow? row, bool raiseEvent)
    {
        Guard.IsNotNull(Rows);

        if (!CanSelectRow)
        {
            StateHasChanged();
            return;
        }
        else if (row == SelectedRow && !raiseEvent)
        {
            StateHasChanged();
            return;
        }
        else if (row is null)
        {
            SetSelectedIndex(-1, raiseEvent);
        }
        else if (Rows is IList<TRow> rowsList)
        {
            int itemIndex = rowsList.IndexOf(row);
            SetSelectedIndex(itemIndex, raiseEvent);
        }
        else
        {
            int i = 0;
            foreach (TRow childRow in Rows)
            {
                if (object.ReferenceEquals(childRow, row))
                {
                    SetSelectedIndex(i, raiseEvent);
                    return;
                }
                i++;
            }

            SetSelectedIndex(-1, raiseEvent);
        }
    }

    private void RaiseOnSelectionChanged()
    {
        if (CanSelectRow)
        {
            InvokeAsync(() =>
            {
                OnRowSelected();

                if (SelectedIndexChanged.HasDelegate)
                {
                    SelectedIndexChanged.InvokeAsync(SelectedIndex);
                }

                if (SelectedRowChanged.HasDelegate)
                {
                    SelectedRowChanged.InvokeAsync(SelectedRow);
                }
            });
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetSelectedRow(SelectedRow, raiseEvent: false);
    }

    protected void OnRowClick(object? selectedRow)
    {
        SetSelectedRow(selectedRow as TRow, raiseEvent: true);
    }

    protected void OnKeyDown(KeyboardEventArgs ev)
    {
        if (Rows is not null && Rows.Count > 0)
        {
            if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
            {
                RaiseOnSelectionChanged();
            }
            else if (string.Equals(ev.Code, "ArrowDown", StringComparison.OrdinalIgnoreCase))
            {
                SelectNextRow();
            }
            else if (string.Equals(ev.Code, "ArrowUp", StringComparison.OrdinalIgnoreCase))
            {
                SelectPreviousRow();
            }
            else if (string.Equals(ev.Code, "Home", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Math.Min(0, Rows.Count - 1));
            }
            else if (string.Equals(ev.Code, "End", StringComparison.OrdinalIgnoreCase))
            {
                SetSelectedIndex(Rows.Count - 1);
            }
        }
    }

    protected virtual void OnRowSelected()
    {
    }
}
