namespace DevToys.Blazor.Core.Services;

public sealed class ContextMenuService
{
    private readonly object _lock = new();
    private readonly IWindowService _windowService;

    public ContextMenuService(IWindowService windowService)
    {
        Guard.IsNotNull(windowService);
        _windowService = windowService;

        _windowService.WindowDeactivated += WindowService_MajorWindowChange;
        _windowService.WindowClosing += WindowService_MajorWindowChange;
        _windowService.WindowLocationChanged += WindowService_MajorWindowChange;
        _windowService.WindowSizeChanged += WindowService_MajorWindowChange;
    }

    internal bool IsContextMenuOpened { get; private set; }

    internal event EventHandler? IsContextMenuOpenedChanged;

    internal event EventHandler? CloseContextMenuRequested;

    internal bool TryOpenContextMenu()
    {
        lock (_lock)
        {
            if (IsContextMenuOpened)
            {
                // Only one context menu should open at the same time.
                return false;
            }

            IsContextMenuOpened = true;
            IsContextMenuOpenedChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }

    internal void CloseContextMenu()
    {
        lock (_lock)
        {
            IsContextMenuOpened = false;
            IsContextMenuOpenedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void OnCloseContextMenuRequested()
    {
        CloseContextMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    private void WindowService_MajorWindowChange(object? sender, EventArgs e)
    {
        if (!Debugger.IsAttached)
        {
            OnCloseContextMenuRequested();
        }
    }
}
