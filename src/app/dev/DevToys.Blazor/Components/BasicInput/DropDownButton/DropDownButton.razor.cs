using System.Collections.Specialized;
using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class DropDownButton<TItem> : StyledComponentBase, IDisposable where TItem : DropDownListItem
{
    private bool _isOpen;
    private ListBox<TItem>? _listBox;

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the content to render in the button.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public string? ToolTip { get; set; }

    [Parameter]
    public bool DisplayArrow { get; set; } = true;

    /// <summary>
    /// Set the anchor point on the element of the popover.
    /// The anchor point will determinate where the popover will be placed.
    /// </summary>
    [Parameter]
    public Origin AnchorOrigin { get; set; } = Origin.BottomCenter;

    /// <summary>
    /// Sets the intersection point if the anchor element. At this point the popover will lay above the popover.
    /// This property in conjunction with <see cref="AnchorOrigin"/> determinate where the popover will be placed.
    /// </summary>
    [Parameter]
    public Origin TransformOrigin { get; set; } = Origin.TopCenter;

    /// <summary>
    /// Gets or sets the menu items to display in the drop down list.
    /// </summary>
    [Parameter]
    public ICollection<TItem>? Items { get; set; }

    public void Dispose()
    {
        if (Items is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged -= OnItemsChanged;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            if (Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += OnItemsChanged;
            }
        }

        if (_isOpen && _listBox is not null)
        {
            await _listBox.FocusAsync();
        }
    }

    private void ContextMenuService_CloseContextMenuRequested(object? sender, EventArgs e)
    {
        CloseDropDown(null);
    }

    private void ToggleDropDown(MouseEventArgs ev)
    {
        if (_isOpen)
        {
            CloseDropDown(ev);
        }
        else
        {
            OpenDropDown(ev);
        }
    }

    private void OpenDropDown(MouseEventArgs _)
    {
        if (IsActuallyEnabled && ContextMenuService.TryOpenContextMenu())
        {
            ContextMenuService.CloseContextMenuRequested += ContextMenuService_CloseContextMenuRequested;
            _isOpen = true;
            StateHasChanged();
        }
    }

    private void CloseDropDown(MouseEventArgs? _)
    {
        ContextMenuService.CloseContextMenuRequested -= ContextMenuService_CloseContextMenuRequested;
        ContextMenuService.CloseContextMenu();
        _isOpen = false;
        StateHasChanged();
    }

    private void OnEscapeKeyPressed()
    {
        if (_isOpen)
        {
            CloseDropDown(null);
        }
    }

    private void OnDropDowlListItemSelected(int _)
    {
        if (_listBox is not null && _listBox.SelectedItem is not null && _listBox.SelectedItem.IsEnabled)
        {
            CloseDropDown(null);
            _listBox.SelectedItem.OnClick.InvokeAsync(_listBox.SelectedItem).Forget();
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        StateHasChanged();
    }
}
