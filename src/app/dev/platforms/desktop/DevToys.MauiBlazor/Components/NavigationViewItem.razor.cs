using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Utilities;

namespace DevToys.MauiBlazor.Components;

public partial class NavigationViewItem : FluentComponentBase
{
    private bool _expanded = false;
    private FluentTreeItem? _treeItem;

    /// <summary>
    /// Gets or sets a unique identifier.
    /// </summary>
    [Parameter]
    public string Id { get; set; } = Identifier.NewId();

    /// <summary>
    /// Gets or sets a value that indicates whether a tree node is expanded. Ignored if there are no menu items.
    /// </summary>
    [Parameter]
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Gets the value that indicates whether or not descendant item is selected.
    /// </summary>
    public bool IsChildSelected { get; }

    /// <summary>
    /// Gets or sets the text of the item
    /// </summary>
    [Parameter]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the icon to display.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the item is selected
    /// </summary>
    [Parameter]
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private bool HasIcon => !string.IsNullOrWhiteSpace(Icon);

    [CascadingParameter(Name = "NavigationView")]
    public NavigationView NavigationView { get; set; } = default!;

    /// <summary>
    /// Callback function for when the item is clicked
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Callback function for when the item is expanded
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnExpandedChanged { get; set; }

    protected string? ClassValue => new CssBuilder(Class)
        .AddClass("navigationviewitem")
        .Build();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle(Style)
        .Build();

    protected override void OnParametersSet()
    {
        if (_expanded != IsExpanded)
        {
            _expanded = IsExpanded;
            if (OnExpandedChanged.HasDelegate)
                OnExpandedChanged.InvokeAsync(_expanded); //.SafeFireAndForget();
        }

        base.OnParametersSet();
    }

    protected async Task OnClickHandlerAsync(MouseEventArgs e)
    {
        IsSelected = true;

        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(e);
        }
    }
}
