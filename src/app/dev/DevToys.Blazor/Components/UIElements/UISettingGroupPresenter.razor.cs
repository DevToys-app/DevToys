namespace DevToys.Blazor.Components.UIElements;

public partial class UISettingGroupPresenter : ComponentBase
{
    [Parameter]
    public IUISettingGroup UISettingGroup { get; set; } = default!;
}
