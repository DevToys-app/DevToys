namespace DevToys.Blazor.Components.UIElements;

public partial class UIGridPresenter : ComponentBase
{
    [Parameter]
    public IUIGrid UIGrid { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        UIGrid.CellsChanged += UIGrid_CellsChanged;
    }

    public void Dispose()
    {
        UIGrid.CellsChanged += UIGrid_CellsChanged;
    }

    private void UIGrid_CellsChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }
}
