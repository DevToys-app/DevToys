using DevToys.Api;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIElementPresenter : ComponentBase
{
    [Parameter]
    public IUIElement? UIElement { get; set; }
}
