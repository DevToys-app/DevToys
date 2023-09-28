using System.Runtime.InteropServices;
using DevToys.Blazor.Core.Services;
using DevToys.Core;
using DevToys.Core.Tools;
using Foundation;
using Microsoft.Maui.Controls;
using ObjCRuntime;
using UIKit;

namespace DevToys.MacOS.Core;

internal sealed class WindowService : IWindowService
{
    private Window? _window;
    private TitleBarInfoProvider? _titleBarInfoProvider;
    private bool _isCompactOverlayMode;
    private WindowStateBackup? _nonCompactOverlayModeWindowState;
    private WindowStateBackup? _compactOverlayModeWindowState;

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

    internal void SetWindow(Window window, TitleBarInfoProvider titleBarInfoProvider)
    {
        Guard.IsNull(_window);
        Guard.IsNotNull(window);
        Guard.IsNotNull(titleBarInfoProvider);

        _window = window;
        _titleBarInfoProvider = titleBarInfoProvider;
        _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        UpdateWindowTitle();
    }

    private void UpdateCompactOverlayState(bool shouldEnterCompactOverlayMode)
    {
        Guard.IsNotNull(_window);

        if (_window.Handler is null || _window.Handler.PlatformView is not UIWindow uiWindow)
        {
            return;
        }

        int top = (int)Math.Max(uiWindow.Screen.Bounds.Top, uiWindow.Screen.Bounds.Bottom);
        var uiWindowPosition = uiWindow.GetWindowPosition();
        if (shouldEnterCompactOverlayMode)
        {
            // Save the state of the window before entering compact overlay mode.
            _nonCompactOverlayModeWindowState
                = new(
                    uiWindow.IsZoomed(),
                    uiWindow.CoordinateSpace.Bounds.Height,
                    uiWindow.CoordinateSpace.Bounds.Width,
                    uiWindowPosition.Y,
                    uiWindowPosition.X,
                    _window.MaximumHeight,
                    _window.MaximumWidth,
                    _window.MinimumHeight,
                    _window.MinimumWidth);

            // Enter compact overlay mode
            uiWindow.ToggleWindowAlwaysOnTop(alwaysOnTop: true);
            uiWindow.ToggleTitleBarButtons(hideButtons: true);
            uiWindow.ForceUnzoom();

            if (_compactOverlayModeWindowState is null)
            {
                // MacCatalyst doesn't support resizing programmatically. However a workaround to enable resizing is to set Minimum and Maximize size to the desired one.
                // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows#mac-catalyst
                _window.MinimumWidth = 600;
                _window.MaximumWidth = 600;
                _window.MinimumHeight = 400;
                _window.MaximumHeight = 400;

                // Give the Window time to resize
                _window.Dispatcher.Dispatch(() =>
                {
                    // Set the actual minimum and maximum size we want to allow.
                    _window.MinimumWidth = 340;
                    _window.MinimumHeight = 400;
                    _window.MaximumWidth = 640;
                    _window.MaximumHeight = 800;
                });

                uiWindow.MoveWindow(
                    x: ((uiWindow.Screen.Bounds.Left + uiWindow.Screen.Bounds.Width) / uiWindow.Screen.Scale) - 600 - 54,
                    y: top - (uiWindow.Screen.Bounds.Top / uiWindow.Screen.Scale) - 54); // On mac, the screen origin starts at the bottom.
            }
            else
            {
                // MacCatalyst doesn't support resizing programmatically. However a workaround to enable resizing is to set Minimum and Maximize size to the desired one.
                // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows#mac-catalyst
                _window.MinimumWidth = _compactOverlayModeWindowState.Width;
                _window.MaximumWidth = _compactOverlayModeWindowState.Width;
                _window.MinimumHeight = _compactOverlayModeWindowState.Height;
                _window.MaximumHeight = _compactOverlayModeWindowState.Height;

                // Give the Window time to resize
                _window.Dispatcher.Dispatch(() =>
                {
                    // Set the actual minimum and maximum size we want to allow.
                    _window.MinimumWidth = _compactOverlayModeWindowState.MinWidth;
                    _window.MinimumHeight = _compactOverlayModeWindowState.MinHeight;
                    _window.MaximumWidth = _compactOverlayModeWindowState.MaxWidth;
                    _window.MaximumHeight = _compactOverlayModeWindowState.MaxHeight;

                    uiWindow.MoveWindow(
                        x: _compactOverlayModeWindowState.Left,
                        y: _compactOverlayModeWindowState.Top + _compactOverlayModeWindowState.Height); // On mac, the screen origin starts at the bottom.
                });
            }
        }
        else
        {
            // Save the state of the window while being in compact overlay mode.
            _compactOverlayModeWindowState
                = new(
                    uiWindow.IsZoomed(),
                    uiWindow.CoordinateSpace.Bounds.Height,
                    uiWindow.CoordinateSpace.Bounds.Width,
                    uiWindowPosition.Y,
                    uiWindowPosition.X,
                    _window.MaximumHeight,
                    _window.MaximumWidth,
                    _window.MinimumHeight,
                    _window.MinimumWidth);

            // Restore the state of the window
            uiWindow.ToggleWindowAlwaysOnTop(alwaysOnTop: false);
            uiWindow.ToggleTitleBarButtons(hideButtons: false);
            if (_compactOverlayModeWindowState.IsZoomed)
            {
                uiWindow.ForceZoom();
            }

            Guard.IsNotNull(_nonCompactOverlayModeWindowState);

            // MacCatalyst doesn't support resizing programmatically. However a workaround to enable resizing is to set Minimum and Maximize size to the desired one.
            // https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/windows#mac-catalyst
            _window.MinimumWidth = _nonCompactOverlayModeWindowState.Width;
            _window.MaximumWidth = _nonCompactOverlayModeWindowState.Width;
            _window.MinimumHeight = _nonCompactOverlayModeWindowState.Height;
            _window.MaximumHeight = _nonCompactOverlayModeWindowState.Height;

            // Give the Window time to resize
            _window.Dispatcher.Dispatch(() =>
            {
                // Set the actual minimum and maximum size we want to allow.
                _window.MinimumWidth = _nonCompactOverlayModeWindowState.MinWidth;
                _window.MinimumHeight = _nonCompactOverlayModeWindowState.MinHeight;
                _window.MaximumWidth = _nonCompactOverlayModeWindowState.MaxWidth;
                _window.MaximumHeight = _nonCompactOverlayModeWindowState.MaxHeight;

                uiWindow.MoveWindow(
                    x: _nonCompactOverlayModeWindowState.Left,
                    y: _nonCompactOverlayModeWindowState.Top + _nonCompactOverlayModeWindowState.Height); // On mac, the screen origin starts at the bottom.
            });
        }
    }

    private void UpdateWindowTitle()
    {
        Guard.IsNotNull(_window);
        Guard.IsNotNull(_titleBarInfoProvider);
        if (_window.Handler is not null && _window.Handler.PlatformView is UIWindow uiWindow)
        {
            Guard.IsNotNull(uiWindow.WindowScene);
            uiWindow.WindowScene.Title = _titleBarInfoProvider.Title ?? string.Empty;
        }
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.Title))
        {
            UpdateWindowTitle();
        }
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

        Guard.IsNotNull(MauiProgram.MefComposer);

        // Dispose every disposable tool instance.
        MauiProgram.MefComposer.Provider.Import<GuiToolProvider>().DisposeTools();

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);
    }
}
