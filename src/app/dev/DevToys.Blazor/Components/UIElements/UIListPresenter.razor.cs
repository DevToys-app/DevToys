namespace DevToys.Blazor.Components.UIElements;

public partial class UIListPresenter : ComponentBase, IDisposable
{
    private int _selectedIndex = -1;

    [Parameter]
    public IUIList UIList { get; set; } = default!;

    [Parameter]
    public int SelectedIndex { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIList.SelectedItemChanged += UIList_SelectedItemChanged;
        UIList_SelectedItemChanged(this, EventArgs.Empty);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_selectedIndex != SelectedIndex)
        {
            if (UIList.CanSelectItem)
            {
                _selectedIndex = SelectedIndex;
                UIList.Select(SelectedIndex);
            }
            else
            {
                SelectedIndex = -1;
                UIList.Select(-1);
            }
        }
    }

    private void UIList_SelectedItemChanged(object? sender, EventArgs e)
    {
        if (UIList.Items is not null && UIList.SelectedItem is not null && UIList.CanSelectItem)
        {
            SelectedIndex = UIList.Items.IndexOf(UIList.SelectedItem);
        }
        else
        {
            SelectedIndex = -1;
            UIList.Select(-1);
        }
    }

    public void Dispose()
    {
        UIList.SelectedItemChanged -= UIList_SelectedItemChanged;
        GC.SuppressFinalize(this);
    }
}
