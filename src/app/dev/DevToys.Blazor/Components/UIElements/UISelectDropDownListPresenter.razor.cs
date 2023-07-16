namespace DevToys.Blazor.Components.UIElements;

public partial class UISelectDropDownListPresenter : ComponentBase, IDisposable
{
    private int _selectedIndex = -1;

    [Parameter]
    public IUISelectDropDownList UISelectDropDownList { get; set; } = default!;

    [Parameter]
    public int SelectedIndex { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UISelectDropDownList.SelectedItemChanged += UISelectDropDownList_SelectedItemChanged;
        UISelectDropDownList_SelectedItemChanged(this, EventArgs.Empty);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_selectedIndex != SelectedIndex)
        {
            _selectedIndex = SelectedIndex;
            UISelectDropDownList.Select(SelectedIndex);
        }
    }

    private void UISelectDropDownList_SelectedItemChanged(object? sender, EventArgs e)
    {
        if (UISelectDropDownList.Items is not null && UISelectDropDownList.SelectedItem is not null)
        {
            SelectedIndex = Array.IndexOf(UISelectDropDownList.Items, UISelectDropDownList.SelectedItem);
        }
        else
        {
            SelectedIndex = -1;
            UISelectDropDownList.Select(-1);
        }
    }

    public void Dispose()
    {
        UISelectDropDownList.SelectedItemChanged -= UISelectDropDownList_SelectedItemChanged;
        GC.SuppressFinalize(this);
    }
}
