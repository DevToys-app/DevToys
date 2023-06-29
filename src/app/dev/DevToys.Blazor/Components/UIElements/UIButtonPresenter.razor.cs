namespace DevToys.Blazor.Components.UIElements;

public partial class UIButtonPresenter : ComponentBase
{
    [Parameter]
    public IUIButton UIButton { get; set; } = default!;

    internal async Task OnButtonClickAsync()
    {
        Guard.IsNotNull(UIButton);
        if (UIButton.OnClickAction is not null)
        {
            await UIButton.OnClickAction.Invoke(); // TODO: Try Catch and log?
        }
    }
}
