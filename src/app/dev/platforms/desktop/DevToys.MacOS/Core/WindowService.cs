using DevToys.Blazor.Core.Services;
using Foundation;

namespace DevToys.MacOS.Core;

internal sealed class WindowService : IWindowService
{
    private bool _isCompactOverlayMode;

    public event EventHandler<EventArgs>? WindowActivated;
    public event EventHandler<EventArgs>? WindowDeactivated;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;
    public event EventHandler<EventArgs>? IsCompactOverlayModeChanged;

    public WindowService()
    {
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowDidBecomeMainNotification"), OnWindowActivated);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowDidResignMainNotification"), OnWindowDeactivated);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillMoveNotification"), OnWindowLocationChanged);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillStartLiveResizeNotification"), OnWindowSizeChanged);
        NSNotificationCenter.DefaultCenter.AddObserver(new NSString("NSWindowWillCloseNotification"), OnWindowClosing);
    }

    // TODO: support Picture-in-Picture mode or mock it.
    public bool IsCompactOverlayModeSupportedByPlatform => false;

    public bool IsCompactOverlayMode
    {
        get => _isCompactOverlayMode;
        set
        {
            _isCompactOverlayMode = value;
            UpdateCompactOverlayState(value);
            IsCompactOverlayModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateCompactOverlayState(bool shouldEnterCompactOverlayMode)
    {

    }

    public void OnWindowActivated(NSNotification notification)
    {
        WindowActivated?.Invoke(this, EventArgs.Empty);
    }

    public void OnWindowDeactivated(NSNotification notification)
    {
        WindowDeactivated?.Invoke(this, EventArgs.Empty);
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
