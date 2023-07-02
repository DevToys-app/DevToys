namespace DevToys.Blazor.Components.UIElements;

public partial class UIWrapPresenter : ComponentBase
{
    [Parameter]
    public IUIWrap UIWrap { get; set; } = default!;
}
