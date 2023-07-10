namespace DevToys.Blazor.Core.Services;

public interface IWindowService
{
    event EventHandler<EventArgs>? WindowActivated;

    event EventHandler<EventArgs>? WindowDeactivated;

    event EventHandler<EventArgs>? WindowLocationChanged;

    event EventHandler<EventArgs>? WindowSizeChanged;

    event EventHandler<EventArgs>? WindowClosing;

    event EventHandler<EventArgs>? IsCompactOverlayModeChanged;

    bool IsCompactOverlayMode { get; set; }

    bool IsCompactOverlayModeSupportedByPlatform { get; }
}
