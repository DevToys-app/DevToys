using DevToys.Blazor.Core.Services;
using Foundation;

namespace DevToys.MacOS.Core;

internal sealed class WindowService : IWindowService
{
    public event EventHandler<EventArgs>? WindowLostFocus;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;

    public WindowService()
    {
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowDidResignMainNotification"), OnWindowLostFocus);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillMoveNotification"), OnWindowLocationChanged);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillStartLiveResizeNotification"), OnWindowSizeChanged);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillCloseNotification"), OnWindowClosing);
    }

    public void OnWindowLostFocus(NSNotification notification)
    {
        WindowLostFocus?.Invoke(this, EventArgs.Empty);
    }

    public void OnWindowLocationChanged(NSNotification notification)
    {
        WindowLocationChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OnWindowSizeChanged(NSNotification notification)
    {
        WindowSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OnWindowClosing(NSNotification notification)
    {
        WindowClosing?.Invoke(this, EventArgs.Empty);
    }
}
