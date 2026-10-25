namespace DevToys.Blazor.Components.UIElements;

public partial class UICheckBoxPresenter : ComponentBase
{
    [Parameter]
    public IUICheckBox UICheckBox { get; set; } = default!;

    private void OnCheckedChanged(bool value)
    {
        if (value)
        {
            UICheckBox.Check();
        }
        else
        {
            UICheckBox.Uncheck();
        }
    }
}
