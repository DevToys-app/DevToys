namespace DevToys.Blazor.Components.UIElements;

public partial class UISettingPresenter : ComponentBase
{
    [Parameter]
    public IUISetting UISetting { get; set; } = default!;
}
