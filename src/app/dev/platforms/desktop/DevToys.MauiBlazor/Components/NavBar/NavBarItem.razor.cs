namespace DevToys.MauiBlazor.Components;

public partial class NavBarItem<TElement> : StyledComponentBase where TElement : class
{
    [Parameter]
    public NavBar<TElement> OwnerNavBar { get; set; } = default!;

    [Parameter]
    public TElement Item { get; set; } = default!;

    [Parameter]
    public IEnumerable<object>? Children { get; set; }

    [Parameter]
    public bool IsExpanded { get; set; }

    [Parameter]
    public RenderFragment<TElement> TitleTemplate { get; set; } = default!;

    [Parameter]
    public RenderFragment<TElement> IconTemplate { get; set; } = default!;

    [Parameter]
    public EventCallback<TElement> OnSelected { get; set; }

    private Task OnItemSelectedAsync(object item)
    {
        Guard.IsAssignableToType<TElement>(item);
        var strongItem = (TElement)item;
        return OnSelected.InvokeAsync(strongItem);
    }

    private void OnToggleExpandClick()
    {
        IsExpanded = !IsExpanded;
    }
}
