using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class ContextMenu : StyledComponentBase
{
    private bool _isOpen;
    private ListBox<ContextMenuItem>? _listBox;

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    /// <summary>
    /// Gets or sets a Button, TextBox or any other component on which the context menu should be displayed on right click.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the menu items to display in the context menu.
    /// </summary>
    [Parameter]
    public ICollection<ContextMenuItem>? Items { get; set; }

    [Parameter]
    public EventCallback OnContextMenuOpening { get; set; }

    [Parameter]
    public EventCallback OnContextMenuOpened { get; set; }

    [Parameter]
    public EventCallback OnContextMenuClosed { get; set; }

    public string? PopoverStyle { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isOpen && _listBox is not null)
        {
            await _listBox.FocusAsync();
        }
    }

    private void ContextMenuService_CloseContextMenuRequested(object? sender, EventArgs e)
    {
        CloseMenuAsync(null).Forget();
    }

    private Task ToggleMenuAsync(MouseEventArgs ev)
    {
        if (_isOpen)
        {
            return CloseMenuAsync(ev);
        }
        else
        {
            return OpenMenuAsync(ev);
        }
    }

    private async Task OpenMenuAsync(MouseEventArgs ev)
    {
        if (IsActuallyEnabled)
        {
            // This may update the item list.
            if (OnContextMenuOpening.HasDelegate)
            {
                await OnContextMenuOpening.InvokeAsync();
            }

            if (Items?.Count == 0)
            {
                // Cancelling. Nothing to display.
                return;
            }

            if (ContextMenuService.TryOpenContextMenu())
            {
                // Place the popup at the mouse position.
                PopoverStyle = $"margin-top: {ev?.ClientY.ToPx()}; margin-left: {ev?.ClientX.ToPx()};";

                ContextMenuService.CloseContextMenuRequested += ContextMenuService_CloseContextMenuRequested;
                _isOpen = true;
                StateHasChanged();

                if (OnContextMenuOpened.HasDelegate)
                {
                    await OnContextMenuOpened.InvokeAsync();
                }
            }
        }
    }

    private async Task CloseMenuAsync(MouseEventArgs? _)
    {
        ContextMenuService.CloseContextMenuRequested -= ContextMenuService_CloseContextMenuRequested;
        ContextMenuService.CloseContextMenu();
        _isOpen = false;
        StateHasChanged();

        if (OnContextMenuClosed.HasDelegate)
        {
            await OnContextMenuClosed.InvokeAsync();
        }
    }

    private async Task OnEscapeKeyPressedAsync()
    {
        if (_isOpen)
        {
            await CloseMenuAsync(null);
        }
    }

    private void OnListBoxKeyDown(KeyboardEventArgs ev)
    {
        if (_listBox is not null && string.Equals(ev.Code, "Tab", StringComparison.OrdinalIgnoreCase))
        {
            _listBox.SelectNextItem();
        }
    }

    private void OnContextMenuItemSelected(int itemIndex)
    {
        if (_listBox is not null && _listBox.SelectedItem is not null && _listBox.SelectedItem.IsEnabled)
        {
            CloseMenuAsync(null).Forget();
            if (_listBox.SelectedItem.OnClick.HasDelegate)
            {
                _listBox.SelectedItem.OnClick.InvokeAsync(_listBox.SelectedItem).Forget();
            }
        }
    }
}
