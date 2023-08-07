using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class DataGridColumnHeader<TRow, TCell, TRowDetail> : JSStyledComponentBase
    where TRow : class, ICollection<TCell>, IDataGridRow<TRowDetail>
    where TCell : class
    where TRowDetail : class
{
    private double? _resizerHeight;
    private bool _isResizing;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/Collections/DataGrid/DataGridColumnHeader.razor.js";

    protected string? DataGridColumnHeaderStyle => new StyleBuilder()
        .AddStyle("width", Width!.ToPx(), when: Width.HasValue)
        .AddStyle(Style)
        .Build();

    protected string? ResizerStyle => new StyleBuilder()
        .AddStyle("height", _resizerHeight?.ToPx() ?? "100%")
        .Build();

    [Parameter]
    public string? Title { get; set; }

    [CascadingParameter]
    protected DataGrid<TRow, TCell, TRowDetail>? DataGrid { get; set; }

    protected override void OnInitialized()
    {
        Guard.IsNotNull(DataGrid);
        DataGrid.AddColumn(this);

        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        Guard.IsNotNull(DataGrid);
        DataGrid.RemoveColumn(this);
        return base.DisposeAsync();
    }

    internal async Task<double> UpdateColumnWidthAsync(double targetWidth, double gridHeight, bool finishResize)
    {
        if (targetWidth > 0)
        {
            _resizerHeight = gridHeight;
            Width = (int)targetWidth;
            await InvokeAsync(StateHasChanged);
        }

        if (finishResize)
        {
            _isResizing = false;
            await InvokeAsync(StateHasChanged);
        }

        return await GetCurrentCellWidthAsync();
    }

    internal async Task<double> GetCurrentCellWidthAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(Element);
            return await (await JSModule).InvokeAsync<double>("getActualWidth", Element);
        }
    }

    private async Task OnResizerMouseDown(MouseEventArgs args)
    {
        if (args.Detail > 1) // Double click clears the width, hence setting it to minimum size.
        {
            Width = null;
            return;
        }

        Guard.IsNotNull(DataGrid);
        _isResizing = await DataGrid.StartResizeColumn(this, args.ClientX);
    }

    private async Task OnResizerMouseOver()
    {
        if (!_isResizing)
        {
            Guard.IsNotNull(DataGrid);
            _resizerHeight = await DataGrid.GetActualHeightAsync();
        }
    }

    private void OnResizerMouseLeave()
    {
        if (!_isResizing)
        {
            _resizerHeight = null;
        }
    }
}
