namespace DevToys.Blazor.Core.Services;

public sealed class ContextMenuService
{
    private readonly object _lock = new();

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
}
