namespace DevToys.Blazor.Components.UIElements;

public partial class UICardPresenter : ComponentBase
{
    [Parameter]
    public IUICard UICard { get; set; } = default!;
}
