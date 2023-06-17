using DevToys.Api;
using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class DropDownList : StyledComponentBase
{
    private bool _isOpen;
    private ListBox<DropDownListItem>? _listBox;

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the content to render in the button.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Header { get; set; }

    /// <summary>
    /// Gets or sets the menu items to display in the drop down list.
    /// </summary>
    [Parameter]
    public ICollection<DropDownListItem>? Items { get; set; }

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

    private void OpenDropDown(MouseEventArgs ev)
    {
        if (IsEnabled && ContextMenuService.TryOpenContextMenu())
        {
            ContextMenuService.CloseContextMenuRequested += ContextMenuService_CloseContextMenuRequested;
            _isOpen = true;
            StateHasChanged();
        }
    }

    private void CloseDropDown(MouseEventArgs? ev)
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

    private void OnDropDowlListItemSelected(int itemIndex)
    {
        if (_listBox is not null && _listBox.SelectedItem is not null && _listBox.SelectedItem.IsEnabled)
        {
            CloseDropDown(null);
            _listBox.SelectedItem.OnClick.InvokeAsync().Forget();
        }
    }
}
