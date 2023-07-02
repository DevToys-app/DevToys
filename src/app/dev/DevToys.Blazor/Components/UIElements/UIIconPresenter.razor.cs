namespace DevToys.Blazor.Components.UIElements;

public partial class UIIconPresenter : ComponentBase
{
    [Parameter]
    public IUIIcon UIIcon { get; set; } = default!;
}
