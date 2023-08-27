namespace DevToys.Blazor.Components.UIElements;

public partial class UIElementPresenter : ComponentBase, IDisposable
{
    [Parameter]
    public IUIElement UIElement { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Guard.IsNotNull(UIElement);
        Guard.IsAssignableToType<INotifyPropertyChanged>(UIElement);
        ((INotifyPropertyChanged)UIElement).PropertyChanged += UIElementPresenter_PropertyChanged;
    }

    private void UIElementPresenter_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When a IUIElement such as a Button handles a click, the action may change properties of the Button in question, or other
        // element around (Stack, List, other Buttons...etc). When it does so, Blazor is smart enough to re-evaluate bindings
        // of the current Button being clicked, but due to optimization, it doesn't re-evaluate other elements that may have changed.
        // As a workaround, we here listen to UIElement.PropertyChanged to be notified when a property of the IUIElement has changed so
        // we can let Blazor know that we should re-render this element.
        InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    public void Dispose()
    {
        Guard.IsAssignableToType<INotifyPropertyChanged>(UIElement);
        ((INotifyPropertyChanged)UIElement).PropertyChanged -= UIElementPresenter_PropertyChanged;
        GC.SuppressFinalize(this);
    }
}
