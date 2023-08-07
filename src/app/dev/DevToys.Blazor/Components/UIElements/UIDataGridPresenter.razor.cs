namespace DevToys.Blazor.Components.UIElements;

public partial class UIDataGridPresenter : ComponentBase, IDisposable
{
    private int _selectedIndex = -1;

    [Parameter]
    public IUIDataGrid UIDataGrid { get; set; } = default!;

    [Parameter]
    public int SelectedIndex { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIDataGrid.SelectedRowChanged += UIDataGrid_SelectedRowChanged;
        UIDataGrid_SelectedRowChanged(this, EventArgs.Empty);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_selectedIndex != SelectedIndex)
        {
            if (UIDataGrid.CanSelectRow)
            {
                _selectedIndex = SelectedIndex;
                UIDataGrid.Select(SelectedIndex);
            }
            else
            {
                SelectedIndex = -1;
                UIDataGrid.Select(-1);
            }
        }
    }

    private void UIDataGrid_SelectedRowChanged(object? sender, EventArgs e)
    {
        if (UIDataGrid.Rows is not null && UIDataGrid.SelectedRow is not null && UIDataGrid.CanSelectRow)
        {
            SelectedIndex = UIDataGrid.Rows.IndexOf(UIDataGrid.SelectedRow);
        }
        else
        {
            SelectedIndex = -1;
            UIDataGrid.Select(-1);
        }
    }

    public void Dispose()
    {
        UIDataGrid.SelectedRowChanged -= UIDataGrid_SelectedRowChanged;
        GC.SuppressFinalize(this);
    }
}
