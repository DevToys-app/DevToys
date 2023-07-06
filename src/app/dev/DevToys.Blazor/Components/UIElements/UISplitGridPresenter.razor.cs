namespace DevToys.Blazor.Components.UIElements;

public partial class UISplitGridPresenter : ComponentBase
{
    [Parameter]
    public IUISplitGrid UISplitGrid { get; set; } = default!;
}
