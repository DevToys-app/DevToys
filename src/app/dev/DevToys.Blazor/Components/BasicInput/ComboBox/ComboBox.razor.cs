using DevToys.Blazor.Core.Services;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Components;

public partial class ComboBox<TElement> : SelectBase<TElement> where TElement : class
{
    private bool _isOpen;
    private ElementReference? _listBox;
    private ScrollViewer? _scrollViewer;
    private Button? _button;

    protected string SelectedItemId => Id + "-selectedItem";

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

    private void OpenDropDown(MouseEventArgs _)
    {
        if (IsActuallyEnabled && ContextMenuService.TryOpenContextMenu())
        {
            ContextMenuService.CloseContextMenuRequested += ContextMenuService_CloseContextMenuRequested;
            _isOpen = true;
            StateHasChanged();

            Task.Delay(200)
                .ContinueWith(t =>
                {
                    InvokeAsync(async () =>
                    {
                        Guard.IsNotNull(_scrollViewer);
                        await _scrollViewer.ScrollToAsync(SelectedItemId);
                    });
                });
        }
    }

    private void CloseDropDown(MouseEventArgs? __)
    {
        ContextMenuService.CloseContextMenuRequested -= ContextMenuService_CloseContextMenuRequested;
        ContextMenuService.CloseContextMenu();
        _isOpen = false;
        StateHasChanged();

        if (_button is not null)
        {
            _ = JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _button.Element);
        }
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
