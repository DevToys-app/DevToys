using DevToys.Api;
using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

// TODO: handle Up and Down arrow navigation in the list of context menu items.
// TODO: Close the context menu when the window lose focus or get resized.
public partial class ContextMenu : StyledComponentBase
{
    private bool _isOpen;
    private FocusTrapper? _focusTrapper;

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
    public IReadOnlyList<ContextMenuItem>? Items { get; set; }

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

        if (_isOpen && _focusTrapper is not null)
        {
            await _focusTrapper.FocusAsync();
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
        if (IsEnabled)
        {
            // This may update the item list.
            await OnContextMenuOpening.InvokeAsync();

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

                await OnContextMenuOpened.InvokeAsync();
            }
        }
    }

    private async Task CloseMenuAsync(MouseEventArgs? ev)
    {
        ContextMenuService.CloseContextMenuRequested -= ContextMenuService_CloseContextMenuRequested;
        ContextMenuService.CloseContextMenu();
        _isOpen = false;
        StateHasChanged();

        await OnContextMenuClosed.InvokeAsync();
    }

    private async Task OnEscapeKeyPressedAsync()
    {
        if (_isOpen)
        {
            await CloseMenuAsync(null);
        }
    }

    private async Task OnItemClickAsync(ContextMenuItem item)
    {
        if (item.IsEnabled)
        {
            CloseMenuAsync(null).Forget();
            await item.OnClick.InvokeAsync();
        }
    }

    private async Task OnItemKeyPressAsync(KeyboardEventArgs ev, ContextMenuItem item)
    {
        if (string.Equals(ev.Code, "Enter", StringComparison.OrdinalIgnoreCase)
            || string.Equals(ev.Code, "Space", StringComparison.OrdinalIgnoreCase))
        {
            await OnItemClickAsync(item);
        }
    }
}
