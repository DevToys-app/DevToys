using System.Windows;
using System.Windows.Interop;
using DevToys.Blazor.Core.Services;
using DevToys.Windows.Controls;
using DevToys.Windows.Core.Helpers;

namespace DevToys.Windows.Core;

internal sealed class WindowService : IWindowService
{
    private MicaWindowWithOverlay? _window;
    private bool _isCompactOverlayMode;
    private WindowStateBackup? _nonCompactOverlayModeWindowState;
    private WindowStateBackup? _compactOverlayModeWindowState;

    public event EventHandler<EventArgs>? WindowActivated;
    public event EventHandler<EventArgs>? WindowDeactivated;
    public event EventHandler<EventArgs>? WindowLocationChanged;
    public event EventHandler<EventArgs>? WindowSizeChanged;
    public event EventHandler<EventArgs>? WindowClosing;
    public event EventHandler<EventArgs>? IsCompactOverlayModeChanged;

    public bool IsCompactOverlayModeSupportedByPlatform => true;

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

    internal void SetWindow(MicaWindowWithOverlay window)
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

    private void UpdateCompactOverlayState(bool shouldEnterCompactOverlayMode)
    {
        Guard.IsNotNull(_window);

        var dpiHelper = new DpiHelper(_window);
        double DPI_SCALE = dpiHelper.LogicalToDeviceUnitsScalingFactorX;
        var windowInteropHelper = new WindowInteropHelper(_window);
        var screen = Screen.FromHandle(windowInteropHelper.Handle);

        if (shouldEnterCompactOverlayMode)
        {
            // Save the state of the window before entering compact overlay mode.
            _nonCompactOverlayModeWindowState
                = new(
                    _window.Topmost,
                    _window.ActualHeight,
                    _window.ActualWidth,
                    _window.Top,
                    _window.Left,
                    _window.MaxHeight,
                    _window.MaxWidth,
                    _window.MinHeight,
                    _window.MinWidth,
                    _window.WindowState);

            // Enter compact overlay mode
            _window.ForbidMinimizeAndMaximize = true;

            _window.WindowState = WindowState.Normal;
            _window.Topmost = true;

            if (_compactOverlayModeWindowState is null)
            {
                _window.MinWidth = 340;
                _window.MinHeight = 400;
                _window.MaxWidth = 640;
                _window.MaxHeight = 800;
                _window.Width = 600;
                _window.Height = 400;
                _window.Top = (screen.WorkingArea.Top / DPI_SCALE) + 32;
                _window.Left = ((screen.WorkingArea.Left + screen.WorkingArea.Width) / DPI_SCALE) - _window.ActualWidth - 32;
            }
            else
            {
                _window.MinHeight = _compactOverlayModeWindowState.MinHeight;
                _window.MinWidth = _compactOverlayModeWindowState.MinWidth;
                _window.MaxHeight = _compactOverlayModeWindowState.MaxHeight;
                _window.MaxWidth = _compactOverlayModeWindowState.MaxWidth;
                _window.Height = _compactOverlayModeWindowState.Height;
                _window.Width = _compactOverlayModeWindowState.Width;
                _window.Top = _compactOverlayModeWindowState.Top;
                _window.Left = _compactOverlayModeWindowState.Left;
            }
        }
        else
        {
            // Save the state of the window while being in compact overlay mode.
            _compactOverlayModeWindowState
                = new(
                    _window.Topmost,
                    _window.ActualHeight,
                    _window.ActualWidth,
                    _window.Top,
                    _window.Left,
                    _window.MaxHeight,
                    _window.MaxWidth,
                    _window.MinHeight,
                    _window.MinWidth,
                    _window.WindowState);

            // Restore the state of the window
            _window.ForbidMinimizeAndMaximize = false;

            Guard.IsNotNull(_nonCompactOverlayModeWindowState);

            _window.MinHeight = _nonCompactOverlayModeWindowState.MinHeight;
            _window.MinWidth = _nonCompactOverlayModeWindowState.MinWidth;
            _window.MaxHeight = _nonCompactOverlayModeWindowState.MaxHeight;
            _window.MaxWidth = _nonCompactOverlayModeWindowState.MaxWidth;
            _window.Height = _nonCompactOverlayModeWindowState.Height;
            _window.Width = _nonCompactOverlayModeWindowState.Width;
            _window.Top = _nonCompactOverlayModeWindowState.Top;
            _window.Left = _nonCompactOverlayModeWindowState.Left;

            _window.WindowState = _nonCompactOverlayModeWindowState.State;
            _window.Topmost = _nonCompactOverlayModeWindowState.Topmost;
        }
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
