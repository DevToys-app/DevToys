using DevToys.Blazor.Core.Services;
using Object = GObject.Object;
using static GObject.Object;

namespace DevToys.Linux.Core;

internal sealed class WindowService : IWindowService
{
    private Gtk.Window? _window;

    // Not supported because GTK4 doesn't have APIs to move a window.
    public bool IsCompactOverlayMode
    {
        get => false;
        set => throw new NotSupportedException();
    }

    public bool IsCompactOverlayModeSupportedByPlatform => false;

    public event EventHandler<EventArgs>? WindowActivated;

    public event EventHandler<EventArgs>? WindowDeactivated;

    public event EventHandler<EventArgs>? WindowLocationChanged;

    public event EventHandler<EventArgs>? WindowSizeChanged;

    public event EventHandler<EventArgs>? WindowClosing;

    public event EventHandler<EventArgs>? IsCompactOverlayModeChanged;

    internal void SetMainWindow(Gtk.Window window)
    {
        Guard.IsNull(_window);
        Guard.IsNotNull(window);
        _window = window;
        _window.OnNotify += Window_OnNotify;
        _window.OnCloseRequest += Window_OnCloseRequest;
    }

    private void Window_OnNotify(Object? sender, NotifySignalArgs e)
    {
        Guard.IsNotNull(_window);

        if (e.Pspec is not null)
        {
            string name = e.Pspec.GetName();
            if (name == "is-active")
            {
                if (_window.IsActive)
                {
                    WindowActivated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    WindowDeactivated?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (name == "position") // Does not work.
            {
                // In GTK4, it is not possible to get the current location of a window.
                // This is because not all window managers/compositors provide this information, for privacy and/or technical reasons.
                // For example, Wayland does not provide window coordinates, because not every Wayland compositor is even a 2D rectangle1.
                // Because of this, the functionality was removed from GTK4.
                WindowLocationChanged?.Invoke(this, EventArgs.Empty);
            }
            else if (name == "size" || name == "default-height" || name == "default-width")
            {
                WindowSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool Window_OnCloseRequest(Gtk.Window sender, EventArgs e)
    {
        WindowClosing?.Invoke(this, EventArgs.Empty);
        return false; // do not stop closing.
    }
}
