using DevToys.Api.Core.Theme;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
#if HAS_UNO
using Windows.UI.Core;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
#endif

namespace DevToys.UI.Framework.Controls;

public abstract partial class BackdropPage : Page
{
    private readonly IThemeListener _themeListener;

    protected BackdropPage(BackdropWindow window, IThemeListener themeListener)
    {
        Guard.IsNotNull(themeListener);
        Guard.IsNotNull(window);

        _themeListener = themeListener;
        _themeListener.ThemeChanged += ThemeListener_ThemeChanged;

        Window = window;
#if HAS_UNO
        Window.Window.Content = this;
        Window.Window.Activated += Window_Activated;
        Window.Window.VisibilityChanged += Window_VisibilityChanged;
        Window.Window.Closed += Window_Closed;
#else
        Window.Content = this;
        Window.Activated += Window_Activated;
        Window.VisibilityChanged += Window_VisibilityChanged;
        Window.Closed += Window_Closed;
#endif
        Window.Closing += Window_Closing;
        Window.Shown += Window_Shown;

        Window.CompactOverlayModeChanged += Window_CompactOverlayModeChanged;
    }

    internal BackdropWindow Window { get; }

    /// <summary>
    /// Gets whether the Compact Overlay (picture in picture) mode is supported by the operating system.
    /// </summary>
    public bool IsCompactOverlayModeSupported => Window.IsCompactOverlayModeSupported;

    /// <summary>
    /// Gets or sets the position of the window when first shown.
    /// </summary>
    public WindowStartupLocation WindowStartupLocation
    {
        get => Window.WindowStartupLocation;
        set => Window.WindowStartupLocation = value;
    }

    /// <summary>
    /// Raised when the <see cref="Window.Visible"/> property has changed.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? VisibilityChanged;

    /// <summary>
    /// Raised when the window opened.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Shown;

    /// <summary>
    /// Raised when the window is closing.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Closing;

    /// <summary>
    /// Raised when the window is closing.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Closed;

    /// <summary>
    /// Raised when the window got activated.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, WindowActivatedEventArgs>? Activated;

    /// <summary>
    /// Raised when the window enters or exits the Compact Overlay mode.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? CompactOverlayModeChanged;

    /// <summary>
    /// Changes the size of the window.
    /// </summary>
    public void Resize(int width, int height)
        => Window.Resize(width, height);

    /// <summary>
    /// Changes the window location on the screen.
    /// </summary>
    public void Move(int x, int y)
        => Window.Move(x, y);

    /// <summary>
    /// Opens a window and returns without waiting for the newly opened window to close.
    /// </summary>
    public void Show()
        => Window.Show();

    /// <summary>
    /// Attempts to activate the application window by bringing it to the foreground and setting the input focus to it.
    /// </summary>
    public void Activate()
    {
#if HAS_UNO
        Window.Window.Activate();
#else
        Window.Activate();
#endif
    }

    /// <summary>
    /// Gets whether the window is currently in compact overlay mode.
    /// </summary>
    public bool IsInCompactOverlayMode()
        => Window.IsInCompactOverlayMode();

    /// <summary>
    /// Tries to enter or exit compact overlay mode.
    /// </summary>
    public void TryToggleCompactOverlayMode()
        => Window.TryToggleCompactOverlayMode();

    /// <summary>
    /// Sets the title of the window that the operating system will display (in the task bar, for example).
    /// </summary>
    public void SetTitle(string title)
        => Window.SetTitle(title);

#if HAS_UNO
    private void Window_Closed(object sender, CoreWindowEventArgs args)
#else
    private void Window_Closed(object sender, WindowEventArgs args)
#endif
    {
#if HAS_UNO
        Window.Window.Activated -= Window_Activated;
        Window.Window.VisibilityChanged -= Window_VisibilityChanged;
        Window.Window.Closed -= Window_Closed;
#else
        Window.Activated -= Window_Activated;
        Window.VisibilityChanged -= Window_VisibilityChanged;
        Window.Closed -= Window_Closed;
#endif
        Window.Closing -= Window_Closing;
        Window.Shown -= Window_Shown;
        _themeListener.ThemeChanged -= ThemeListener_ThemeChanged;

        Closed?.Invoke(Window, EventArgs.Empty);
    }

#if HAS_UNO
    private void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs args)
#else
    private void Window_VisibilityChanged(object sender, WindowVisibilityChangedEventArgs args)
#endif
    {
        VisibilityChanged?.Invoke(Window, EventArgs.Empty);
    }

    private void Window_Shown(BackdropWindow sender, EventArgs args)
    {
        Shown?.Invoke(Window, args);
    }

    private void Window_Closing(BackdropWindow sender, EventArgs args)
    {
        Closing?.Invoke(Window, args);
    }

    private void Window_Activated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs args)
    {
        Activated?.Invoke(Window, args);
    }

    private void Window_CompactOverlayModeChanged(BackdropWindow sender, EventArgs args)
    {
        CompactOverlayModeChanged?.Invoke(Window, args);
    }

    private void ThemeListener_ThemeChanged(object? sender, EventArgs e)
    {
        this.RequestedTheme = _themeListener.ActualAppTheme == Api.Core.Theme.ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
    }
}
