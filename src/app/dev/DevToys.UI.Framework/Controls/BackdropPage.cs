using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
#if HAS_UNO
using Windows.UI.Core;
using WindowActivatedEventArgs = Microsoft.UI.Xaml.WindowActivatedEventArgs;
#endif

namespace DevToys.UI.Framework.Controls;

public abstract class BackdropPage : Page
{
    internal BackdropWindow Window { get; }

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
    /// Raised when the window got activated.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, WindowActivatedEventArgs>? Activated;

    protected BackdropPage(BackdropWindow window)
    {
        Guard.IsNotNull(window);
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
    }

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
}
