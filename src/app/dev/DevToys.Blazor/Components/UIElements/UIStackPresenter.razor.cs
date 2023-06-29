namespace DevToys.Blazor.Components.UIElements;

public partial class UIStackPresenter : ComponentBase
{
    [Parameter]
    public IUIStack UIStack { get; set; } = default!;
}
