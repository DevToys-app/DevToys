using System.Windows;
using DevToys.Blazor.Core.Services;

namespace DevToys.Windows.Core;

internal sealed class WindowService : IWindowService
{
    private Window? _window;

    public event EventHandler<EventArgs>? WindowLostFocus;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;

    internal void SetWindow(Window window)
    {
        Guard.IsNull(_window);
        Guard.IsNotNull(window);
        _window = window;

        _window.LostFocus += Window_LostFocus;
        _window.LocationChanged += Window_LocationChanged;
        _window.SizeChanged += Window_SizeChanged;
        _window.Closing += Window_Closing;
    }

    private void Window_LostFocus(object sender, RoutedEventArgs e)
    {
        WindowLostFocus?.Invoke(this, EventArgs.Empty);
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
