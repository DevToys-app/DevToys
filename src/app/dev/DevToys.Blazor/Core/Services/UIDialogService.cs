namespace DevToys.Blazor.Core.Services;

public sealed class UIDialogService
{
    private readonly object _lock = new();
    private readonly GlobalDialogService _globalDialogService;

    public UIDialogService(GlobalDialogService globalDialogService)
    {
        _globalDialogService = globalDialogService;
    }

    internal bool IsDialogOpened { get; private set; }

    internal bool IsDismissible { get; private set; }

    internal RenderFragment? DialogContent { get; set; }

    internal RenderFragment? FooterContent { get; set; }

    internal event EventHandler? IsDialogOpenedChanged;

    internal event EventHandler? CloseDialogRequested;

    internal bool TryOpenDialog(bool isDismissible)
    {
        lock (_lock)
        {
            if (_globalDialogService.IsDialogOpened)
            {
                // Only one dialog should open at the same time.
                return false;
            }

            IsDismissible = isDismissible;
            IsDialogOpened = true;
            IsDialogOpenedChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }

    internal void CloseDialog()
    {
        lock (_lock)
        {
            IsDialogOpened = false;
            IsDialogOpenedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void OnCloseDialogRequested()
    {
        CloseDialogRequested?.Invoke(this, EventArgs.Empty);
    }
}
