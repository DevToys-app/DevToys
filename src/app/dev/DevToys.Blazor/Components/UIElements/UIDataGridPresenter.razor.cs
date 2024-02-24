namespace DevToys.Blazor.Components.UIElements;

public partial class UIDataGridPresenter : StyledComponentBase, IDisposable
{
    private int _selectedIndex = -1;
    private bool _isInFullScreenMode;
    private Button? _toggleFullScreenButton;

    [Parameter]
    public IUIDataGrid UIDataGrid { get; set; } = default!;

    [Parameter]
    public int SelectedIndex { get; set; }

    protected string ExtendedId => UIDataGrid.Id + "-" + Id;

    [CascadingParameter]
    protected FullScreenContainer? FullScreenContainer { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIDataGrid.SelectedRowChanged += UIDataGrid_SelectedRowChanged;
        UIDataGrid.IsVisibleChanged += UIDataGrid_IsVisibleChanged;
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

    public void Dispose()
    {
        UIDataGrid.SelectedRowChanged -= UIDataGrid_SelectedRowChanged;
        UIDataGrid.IsVisibleChanged -= UIDataGrid_IsVisibleChanged;
        GC.SuppressFinalize(this);
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

    private void UIDataGrid_IsVisibleChanged(object? sender, EventArgs e)
    {
        if (_isInFullScreenMode && !UIDataGrid.IsVisible)
        {
            // If the element is not visible anymore, we need to exit the full screen mode.
            OnToggleFullScreenButtonClickAsync().Forget();
        }
    }

    private async Task OnToggleFullScreenButtonClickAsync()
    {
        Guard.IsNotNull(FullScreenContainer);
        Guard.IsNotNull(_toggleFullScreenButton);
        _isInFullScreenMode = await FullScreenContainer.ToggleFullScreenModeAsync(ExtendedId, _toggleFullScreenButton);
        StateHasChanged();
    }
}
