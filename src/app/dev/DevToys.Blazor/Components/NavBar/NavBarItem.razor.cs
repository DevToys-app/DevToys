using DevToys.Core;

namespace DevToys.Blazor.Components;

public partial class NavBarItem<TElement> : StyledComponentBase where TElement : class
{
    private bool _isExpanded;

    [Parameter]
    public NavBar<TElement> OwnerNavBar { get; set; } = default!;

    [Parameter]
    public TElement Item { get; set; } = default!;

    [Parameter]
    public IEnumerable<object>? Children { get; set; }

    [Parameter]
    public RenderFragment<TElement> TitleTemplate { get; set; } = default!;

    [Parameter]
    public RenderFragment<TElement> IconTemplate { get; set; } = default!;

    [Parameter]
    public EventCallback<TElement> OnSelected { get; set; }

    public bool IsExpanded
    {
        get
        {
            if (Item is IGroup group)
            {
                if (group.GroupShouldBeExpandedInUI)
                {
                    group.GroupIsExpandedInUI = true;
                    return true;
                }

                return group.GroupIsExpandedInUI;
            }
            else
            {
                return _isExpanded;
            }
        }
        set
        {
            if (Item is IGroup group)
            {
                group.GroupIsExpandedInUI = value;
            }
            else
            {
                _isExpanded = value;
            }
        }
    }

    protected override void OnInitialized()
    {
        if (Item is IGroup group && group.GroupShouldBeExpandedByDefaultInUI)
        {
            IsExpanded = true;
        }

        base.OnInitialized();
    }

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
