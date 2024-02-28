using DevToys.Core;

namespace DevToys.Blazor.Components;

public partial class NavBarItem<TElement, TSearchElement>
    : StyledComponentBase
    where TElement : class
    where TSearchElement : class
{
    private bool _isExpanded;
    private bool _ignoreGroupShouldBeExpandedInUI;

    [Parameter]
    public NavBar<TElement, TSearchElement> OwnerNavBar { get; set; } = default!;

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

    [Parameter]
    public EventCallback<ListBoxItemBuildingContextMenuEventArgs> OnBuildingContextMenu { get; set; }

    public bool IsExpanded
    {
        get
        {
            if (Item is IGroup group)
            {
                if (!_ignoreGroupShouldBeExpandedInUI && group.GroupShouldBeExpandedInUI)
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

        if (Item is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += Item_PropertyChanged;
        }

        base.OnInitialized();
    }

    private Task OnItemSelectedAsync(object item)
    {
        Guard.IsAssignableToType<TElement>(item);
        var strongItem = (TElement)item;
        return OnSelected.InvokeAsync(strongItem);
    }

    private Task OnBuildingContextMenuAsync(ListBoxItemBuildingContextMenuEventArgs args)
    {
        if (args.ItemValue is not null)
        {
            Guard.IsAssignableToType<TElement>(args.ItemValue);
        }
        return OnBuildingContextMenu.InvokeAsync(args);
    }

    private void OnToggleExpandClick()
    {
        if (IsExpanded)
        {
            _ignoreGroupShouldBeExpandedInUI = true;
        }

        IsExpanded = !IsExpanded;
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGroup.GroupShouldBeExpandedInUI))
        {
            _ignoreGroupShouldBeExpandedInUI = false;
        }
    }
}
