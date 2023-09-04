using DevToys.Blazor.Core.Services;

namespace DevToys.Linux.Core;

internal sealed class WindowService : IWindowService
{
    public bool IsCompactOverlayMode { get; set; }

    public bool IsCompactOverlayModeSupportedByPlatform => false;

    public event EventHandler<EventArgs>? WindowActivated;
    public event EventHandler<EventArgs>? WindowDeactivated;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;
    public event EventHandler<EventArgs>? IsCompactOverlayModeChanged;
}
