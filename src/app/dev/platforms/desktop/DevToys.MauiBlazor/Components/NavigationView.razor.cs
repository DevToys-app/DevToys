using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Utilities;

namespace DevToys.MauiBlazor.Components;

public partial class NavigationView : FluentComponentBase
{
    /// <summary>
    /// Gets or sets the width of the navigation view.
    /// </summary>
    [Parameter]
    public int Width { get; set; } = 320;

    /// <summary>
    /// Gets or sets an object source used to generate the content of the <see cref="NavigationView"/> menu.
    /// </summary>
    [Parameter]
    public RenderFragment? MenuItemsSource { get; set; }

    /// <summary>
    /// Gets or sets the object that represents the navigation items to be used in the footer menu.
    /// </summary>
    [Parameter]
    public RenderFragment? FooterMenuItemsSource { get; set; }

    /// <summary>
    /// Gets or sets the content of the navigation view.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    [Parameter]
    public FluentTreeItem? SelectedTreeItem { get; set; }

    /// <summary>
    /// Gets or sets a reasonably unique ID 
    /// </summary>
    [Parameter]
    public string Id { get; set; } = Identifier.NewId();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("width", $"{Width}px")
        .AddStyle(Style)
        .Build();

    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("navmenu")
        .Build();

    //private readonly List<NavMenuLink> _links = new();
    //private readonly List<NavMenuGroup> _groups = new();

    //private FluentTreeView? treeView;
    //private FluentTreeItem? currentSelected;

    //[Inject]
    //private NavigationManager NavigationManager { get; set; } = default!;

    //[Inject]
    //private IJSRuntime JS { get; set; } = default!;

    //private IJSObjectReference Module { get; set; } = default!;

    ///// <summary>
    ///// Gets or sets a reasonably unique ID 
    ///// </summary>
    //[Parameter]
    //public string Id { get; set; } = Identifier.NewId();

    ///// <summary>
    ///// Gets or sets the content to be rendered inside the component.
    ///// </summary>
    //[Parameter]
    //public RenderFragment? ToolsHierarchy { get; set; }

    ///// <summary>
    ///// Gets or sets the content to be rendered inside the component.
    ///// </summary>
    //[Parameter]
    //public RenderFragment? RecentTools { get; set; }

    ///// <summary>
    ///// Gets or sets the content to be rendered inside the component.
    ///// </summary>
    //[Parameter]
    //public RenderFragment? FooterItems { get; set; }

    ///// <summary>
    ///// Gets or sets the title of the navigation menu
    ///// Default to "Navigation menu"
    ///// </summary>
    //[Parameter]
    //public string? Title { get; set; } = "DevToys";

    ///// <summary>
    ///// Gets or sets the width of the menu (in pixels).
    ///// </summary>
    //[Parameter]
    //public int? Width { get; set; }

    ///// <summary>
    ///// Gets or sets whether the menu is collapsed.
    ///// </summary>
    //[Parameter]
    //[CascadingParameter]
    //public bool Expanded { get; set; } = true;

    ///// <summary>
    ///// Event callback for when the menu is collapsed status changed.
    ///// </summary>
    //[Parameter]
    //public EventCallback<bool> ExpandedChanged { get; set; }

    ///// <summary>
    ///// Event callback for when group/link is expanded
    ///// </summary>
    //[Parameter]
    //public EventCallback<bool> OnExpanded { get; set; }

    //internal bool HasSubMenu => _groups.Any();

    //internal bool HasIcons => _links.Any(i => !string.IsNullOrWhiteSpace(i.Icon));

    //internal async Task OnHamburgerButtonClickAsync(MouseEventArgs e)
    //{
    //    Expanded = !Expanded;
    //    await InvokeAsync(StateHasChanged);

    //    if (ExpandedChanged.HasDelegate)
    //    {
    //        await ExpandedChanged.InvokeAsync(Expanded);
    //    }

    //    if (OnExpanded.HasDelegate)
    //    {
    //        await OnExpanded.InvokeAsync(Expanded);
    //    }
    //}

    //internal int AddNavLink(NavMenuLink link)
    //{
    //    _links.Add(link);
    //    StateHasChanged();
    //    return _links.Count;
    //}

    //internal int AddNavGroup(NavMenuGroup group)
    //{
    //    _groups.Add(group);
    //    return _groups.Count;
    //}
}
