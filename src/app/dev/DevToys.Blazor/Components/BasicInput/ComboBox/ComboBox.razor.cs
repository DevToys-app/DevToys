using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class ComboBox<TElement> : SelectBase<TElement> where TElement : class
{
    private bool _isOpen;
    private ElementReference? _listBox;
    private Button? _button;

    public ComboBox()
    {
        RaiseSelectionEventOnKeyboardNavigation = false;
    }

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    [Parameter]
    public string? Header { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (_isOpen && _listBox is not null)
        {
            await JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _listBox);
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

        _ = JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _button);
    }

    private void OnEscapeKeyPressed()
    {
        if (_isOpen)
        {
            CloseDropDown(null);
        }
    }

    protected override void OnItemSelected()
    {
        base.OnItemSelected();
        CloseDropDown(null);
    }
}
