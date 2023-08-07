using System.Text.Json;
using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

internal sealed class DataGridColumnResizeService<TRow, TCell, TRowDetail> : IAsyncDisposable
    where TRow : class, ICollection<TCell>, IDataGridRow<TRowDetail>
    where TCell : class
    where TRowDetail : class
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const string EventMouseMove = "mousemove";
    private const string EventMouseUp = "mouseup";

    private readonly DocumentEventService _documentEventService;
    private readonly DataGrid<TRow, TCell, TRowDetail> _dataGrid;
    private bool _isContainerResizeMode;

    private double _currentX;
    private double _startWidth;
    private double _nextStartWidth;
    private DataGridColumnHeader<TRow, TCell, TRowDetail>? _startColumn;
    private DataGridColumnHeader<TRow, TCell, TRowDetail>? _nextColumn;
    private IAsyncDisposable? _mouseMoveSubscription;
    private IAsyncDisposable? _mouseUpSubscription;

    internal DataGridColumnResizeService(DataGrid<TRow, TCell, TRowDetail> dataGrid, DocumentEventService documentEventService)
    {
        _documentEventService = documentEventService;
        _dataGrid = dataGrid;
    }

    public async ValueTask DisposeAsync()
    {
        await UnsubscribeApplicationEvents();
    }

    internal async Task<bool> StartResizeColumn(
        DataGridColumnHeader<TRow, TCell, TRowDetail> headerCell,
        double clientX,
        IList<DataGridColumnHeader<TRow, TCell, TRowDetail>> columns,
        bool isContainerResizeMode)
    {
        if (_mouseMoveSubscription != default || _mouseUpSubscription != default)
        {
            return false;
        }

        _isContainerResizeMode = isContainerResizeMode;
        _currentX = clientX;

        _startWidth = await headerCell.GetCurrentCellWidthAsync();
        _startColumn = headerCell;

        if (!_isContainerResizeMode)
        {
            // In case resize mode is column, we have to find any column right of the current one that can also be resized and is not hidden.
            if (headerCell is not null)
            {
                DataGridColumnHeader<TRow, TCell, TRowDetail>? nextResizableColumn
                    = columns.Skip(columns.IndexOf(headerCell) + 1)
                        .FirstOrDefault(c => c.IsVisible);

                if (nextResizableColumn == null)
                {
                    return false;
                }

                _nextStartWidth = await nextResizableColumn.GetCurrentCellWidthAsync();
                _nextColumn = nextResizableColumn;
            }
        }

        _mouseMoveSubscription = await _documentEventService.SubscribeAsync(EventMouseMove, OnApplicationMouseMove);
        _mouseUpSubscription = await _documentEventService.SubscribeAsync(EventMouseUp, OnApplicationMouseUp);

        _dataGrid.IsResizing = true;
        _dataGrid.StateHasChanged();
        return true;
    }

    private async ValueTask OnApplicationMouseMove(string eventJson)
    {
        await ResizeColumn(JsonSerializer.Deserialize<MouseEventArgs>(eventJson, JsonOptions)!, false);
    }

    private async ValueTask OnApplicationMouseUp(string eventJson)
    {
        bool requiresUpdate = _mouseMoveSubscription != default || _mouseUpSubscription != default;

        _dataGrid.IsResizing = false;
        _dataGrid.StateHasChanged();
        await UnsubscribeApplicationEvents();

        if (requiresUpdate)
        {
            await ResizeColumn(JsonSerializer.Deserialize<MouseEventArgs>(eventJson, JsonOptions)!, true);
        }
    }

    private async Task UnsubscribeApplicationEvents()
    {
        if (_mouseMoveSubscription != null)
        {
            await _mouseMoveSubscription.DisposeAsync();
            _mouseMoveSubscription = null;
        }

        if (_mouseUpSubscription != null)
        {
            await _mouseUpSubscription.DisposeAsync();
            _mouseUpSubscription = null;
        }
    }

    private async Task ResizeColumn(MouseEventArgs mouseEventArgs, bool finish)
    {
        // Need to update height, because resizing of columns can lead to height changes in grid (due to line-breaks)
        double gridHeight = await _dataGrid.GetActualHeightAsync();

        double deltaX = mouseEventArgs.ClientX - _currentX;
        double targetWidth = _startWidth + deltaX;

        // Easy case: ResizeMode is container, we simply update the width of the resized column
        if (_isContainerResizeMode)
        {
            if (_startColumn is not null)
            {
                await _startColumn.UpdateColumnWidthAsync(targetWidth, gridHeight, finish);
            }

            return;
        }

        // In case of column resize mode, we have to find another column that can be resized to
        // enlarge/shrink this other column by the same amount, the current column shall be shrinked/enlarged.
        double nextTargetWidth = _nextStartWidth - deltaX;

        // In case we shrink the current column, make sure to not shrink further after min width has been reached:
        if (deltaX < 0)
        {
            if (_startColumn is not null && _nextColumn is not null)
            {
                await ResizeColumns(
                    _startColumn,
                    _nextColumn,
                    targetWidth,
                    nextTargetWidth,
                    gridHeight,
                    finish);
            }
        }
        // In case we enlarge, we first shrink the following column and ensure it is not shrinked beyond min width:
        else
        {
            if (_nextColumn is not null && _startColumn is not null)
            {
                await ResizeColumns(
                    _nextColumn,
                    _startColumn,
                    nextTargetWidth,
                    targetWidth,
                    gridHeight,
                    finish);
            }
        }
    }

    private static async Task ResizeColumns(
        DataGridColumnHeader<TRow, TCell, TRowDetail> columnToShrink,
        DataGridColumnHeader<TRow, TCell, TRowDetail> columnToEnlarge,
        double shrinkedWidth,
        double enlargedWidth,
        double gridHeight,
        bool finish)
    {
        double actualWidth = await columnToShrink.UpdateColumnWidthAsync(shrinkedWidth, gridHeight, finish);

        // Use actualWidth to see if the column could be made smaller or if it reached its min size.
        if (actualWidth >= shrinkedWidth)
        {
            enlargedWidth -= (actualWidth - shrinkedWidth);
        }

        await columnToEnlarge.UpdateColumnWidthAsync(enlargedWidth, gridHeight, finish);
    }
}
