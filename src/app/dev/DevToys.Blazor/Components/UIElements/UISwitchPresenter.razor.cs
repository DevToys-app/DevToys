namespace DevToys.Blazor.Components.UIElements;

public partial class UISwitchPresenter : ComponentBase
{
    [Parameter]
    public IUISwitch UISwitch { get; set; } = default!;

    private void OnCheckedChanged(bool value)
    {
        if (value)
        {
            UISwitch.On();
        }
        else
        {
            UISwitch.Off();
        }
    }
}
