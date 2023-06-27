using System.Windows;
using DevToys.Blazor.Core.Services;

namespace DevToys.Windows.Core;

internal sealed class WindowService : IWindowService
{
    private Window? _window;

    public event EventHandler<EventArgs>? WindowActivated;
    public event EventHandler<EventArgs>? WindowDeactivated;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;

    public bool IsOverlayMode { get; set; }

    internal void SetWindow(Window window)
    {
        Guard.IsNull(_window);
        Guard.IsNotNull(window);
        _window = window;

        _window.Activated += Window_Activated;
        _window.Deactivated += Window_Deactivated;
        _window.LocationChanged += Window_LocationChanged;
        _window.SizeChanged += Window_SizeChanged;
        _window.Closing += Window_Closing;
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        WindowActivated?.Invoke(this, EventArgs.Empty);
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        WindowDeactivated?.Invoke(this, EventArgs.Empty);
    }

    private void Window_LocationChanged(object? sender, EventArgs e)
    {
        WindowLocationChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        WindowSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        WindowClosing?.Invoke(this, EventArgs.Empty);
    }
}
