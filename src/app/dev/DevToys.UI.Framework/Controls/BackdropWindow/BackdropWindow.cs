using Windows.Foundation;

namespace DevToys.UI.Framework.Controls;

public sealed partial class BackdropWindow
{
    /// <summary>
    /// Gets or sets the position of the window when first shown.
    /// </summary>
    public WindowStartupLocation WindowStartupLocation { get; set; }

    /// <summary>
    /// Raised when the window opened.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Shown;

    /// <summary>
    /// Raised when the window is closing.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Closing;

    /// <summary>
    /// Changes the size of the window.
    /// </summary>
    public partial void Resize(int width, int height);

    /// <summary>
    /// Changes the window location on the screen.
    /// </summary>
    public partial void Move(int x, int y);

    /// <summary>
    /// Opens a window and returns without waiting for the newly opened window to close.
    /// </summary>
    public partial void Show();
}
