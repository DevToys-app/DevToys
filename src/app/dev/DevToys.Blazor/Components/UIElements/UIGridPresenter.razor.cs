namespace DevToys.Blazor.Components.UIElements;

public partial class UIGridPresenter : ComponentBase
{
    [Parameter]
    public IUIGrid UIGrid { get; set; } = default!;
}
