using System.Collections.ObjectModel;

namespace DevToys.Blazor.Components;

public partial class ListBoxItem : StyledComponentBase
{
    private readonly ICollection<ContextMenuItem> _contextMenuItems = new ObservableCollection<ContextMenuItem>();

    [Parameter]
    public object Item { get; set; } = default!;

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback<object> OnSelected { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<ListBoxItemBuildingContextMenuEventArgs> OnBuildingContextMenu { get; set; }

    protected override void OnParametersSet()
    {
        if (IsSelected)
        {
            CSS.Add("selected");
        }
        else
        {
            CSS.Remove("selected");
        }

        if (IsActuallyEnabled)
        {
            CSS.Remove("disabled");
        }
        else
        {
            CSS.Add("disabled");
        }

        base.OnParametersSet();
    }

    private Task OnClickAsync()
    {
        return OnSelected.InvokeAsync(Item);
    }

    private Task OnContextMenuOpeningAsync()
    {
        return OnBuildingContextMenu.InvokeAsync(new ListBoxItemBuildingContextMenuEventArgs(Item, _contextMenuItems));
    }
}
