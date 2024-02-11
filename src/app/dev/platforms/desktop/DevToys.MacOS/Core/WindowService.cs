using DevToys.Blazor.Core.Services;
using DevToys.Core;
using DevToys.Core.Tools;
using DevToys.MacOS.Views;

namespace DevToys.MacOS.Core;

internal sealed class WindowService : IWindowService
{
    private bool _isCompactOverlayMode;
    private WindowStateBackup? _nonCompactOverlayModeWindowState;
    private WindowStateBackup? _compactOverlayModeWindowState;

    internal WindowService()
    {
        MainWindow.Instance.WillMove += OnWindowLocationChanged;
        MainWindow.Instance.WillStartLiveResize += OnWindowSizeChanged;
        MainWindow.Instance.DidResize += OnWindowSizeChanged;
        MainWindow.Instance.DidEndLiveResize += OnWindowSizeChanged;
        MainWindow.Instance.WillClose += OnWindowClosing;
        MainWindow.Instance.DidBecomeMain += OnWindowActivated;
        MainWindow.Instance.DidResignMain += OnWindowDeactivated;
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

    public event EventHandler<EventArgs>? WindowActivated;

    public event EventHandler<EventArgs>? WindowDeactivated;

    public event EventHandler<EventArgs>? WindowLocationChanged;

    public event EventHandler<EventArgs>? WindowSizeChanged;

    public event EventHandler<EventArgs>? WindowClosing;

    public event EventHandler<EventArgs>? IsCompactOverlayModeChanged;

    private void OnWindowLocationChanged(object? sender, EventArgs e)
    {
        WindowLocationChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        WindowSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        WindowActivated?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        WindowDeactivated?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowClosing(object? sender, EventArgs e)
    {
        WindowClosing?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateCompactOverlayState(bool shouldEnterCompactOverlayMode)
    {
        NSScreen screen = MainWindow.Instance.Screen;
        int top = (int)Math.Max(screen.Frame.Top, screen.Frame.Bottom);

        if (shouldEnterCompactOverlayMode)
        {
            bool isZoomed = MainWindow.Instance.IsZoomed;
            if (isZoomed)
            {
                MainWindow.Instance.ToggleFullScreen(MainWindow.Instance);
            }

            // Save the state of the window before entering compact overlay mode.
            _nonCompactOverlayModeWindowState
                = new(
                    isZoomed,
                    MainWindow.Instance.Frame.Height,
                    MainWindow.Instance.Frame.Width,
                    MainWindow.Instance.Frame.Top,
                    MainWindow.Instance.Frame.Left,
                    MainWindow.Instance.MaxSize.Height,
                    MainWindow.Instance.MaxSize.Width,
                    MainWindow.Instance.MinSize.Height,
                    MainWindow.Instance.MinSize.Width);

            // Enter compact overlay mode
            MainWindow.Instance.StyleMask = NSWindowStyle.Closable | NSWindowStyle.Titled | NSWindowStyle.FullSizeContentView;
            MainWindow.Instance.Level = NSWindowLevel.Floating;

            if (_compactOverlayModeWindowState is null)
            {
                MainWindow.Instance.MinSize = new CGSize(340, 400);
                MainWindow.Instance.MaxSize = new CGSize(640, 800);
                CGRect newFrame = MainWindow.Instance.Frame;
                newFrame.Width = 600;
                newFrame.Height = 400;
                newFrame.X = ((screen.Frame.Left + screen.Frame.Width) / screen.BackingScaleFactor) - 600 - 54;
                newFrame.Y = top - (screen.Frame.Top / screen.BackingScaleFactor) - 54; // On mac, the screen origin starts at the bottom.
                MainWindow.Instance.SetFrame(newFrame, display: true);
            }
            else
            {
                MainWindow.Instance.MinSize = new CGSize(_compactOverlayModeWindowState.MinWidth, _compactOverlayModeWindowState.MinHeight);
                MainWindow.Instance.MaxSize = new CGSize(_compactOverlayModeWindowState.MaxWidth, _compactOverlayModeWindowState.MaxHeight);
                CGRect newFrame = MainWindow.Instance.Frame;
                newFrame.Width = _compactOverlayModeWindowState.Width;
                newFrame.Height = _compactOverlayModeWindowState.Height;
                newFrame.X = _compactOverlayModeWindowState.X;
                newFrame.Y = _compactOverlayModeWindowState.Y;
                MainWindow.Instance.SetFrame(newFrame, display: true);
            }
        }
        else
        {
            // Save the state of the window while being in compact overlay mode.
            _compactOverlayModeWindowState
                = new(
                    MainWindow.Instance.IsZoomed,
                    MainWindow.Instance.Frame.Height,
                    MainWindow.Instance.Frame.Width,
                    MainWindow.Instance.Frame.Top,
                    MainWindow.Instance.Frame.Left,
                    MainWindow.Instance.MaxSize.Height,
                    MainWindow.Instance.MaxSize.Width,
                    MainWindow.Instance.MinSize.Height,
                    MainWindow.Instance.MinSize.Width);

            // Restore the state of the window
            MainWindow.Instance.StyleMask = NSWindowStyle.Closable | NSWindowStyle.Miniaturizable |
                                            NSWindowStyle.Resizable | NSWindowStyle.Titled |
                                            NSWindowStyle.FullSizeContentView;
            MainWindow.Instance.Level = NSWindowLevel.Normal;

            Guard.IsNotNull(_nonCompactOverlayModeWindowState);

            MainWindow.Instance.MinSize = new CGSize(_nonCompactOverlayModeWindowState.MinWidth, _nonCompactOverlayModeWindowState.MinHeight);
            MainWindow.Instance.MaxSize = new CGSize(_nonCompactOverlayModeWindowState.MaxWidth, _nonCompactOverlayModeWindowState.MaxHeight);
            CGRect newFrame = MainWindow.Instance.Frame;
            newFrame.Width = _nonCompactOverlayModeWindowState.Width;
            newFrame.Height = _nonCompactOverlayModeWindowState.Height;
            newFrame.X = _nonCompactOverlayModeWindowState.X;
            newFrame.Y = _nonCompactOverlayModeWindowState.Y;
            MainWindow.Instance.SetFrame(newFrame, display: true);

            if (_nonCompactOverlayModeWindowState.IsZoomed)
            {
                MainWindow.Instance.ToggleFullScreen(MainWindow.Instance);
            }
        }
    }
}
