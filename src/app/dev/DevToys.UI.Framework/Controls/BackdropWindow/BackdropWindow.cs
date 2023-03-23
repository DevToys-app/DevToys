using DevToys.UI.Framework.Helpers;
using Microsoft.UI.Xaml;
using Windows.Foundation;

namespace DevToys.UI.Framework.Controls;

public sealed partial class BackdropWindow
{
    private readonly List<FrameworkElement> _draggableAreas = new();

    #region IsWindowDraggableArea

    /// <summary>
    /// Attached property that can be used to set one or multiple controls as area that the user can user to move the window using the mouse.
    /// </summary>
    public static readonly DependencyProperty IsWindowDraggableAreaProperty
        = DependencyProperty.RegisterAttached(
            "IsWindowDraggableArea",
            typeof(bool),
            typeof(BackdropWindow),
            new PropertyMetadata(false, OnAttachingIsWindowDraggableArea));

    public static bool GetIsWindowDraggableArea(DependencyObject d)
    {
        return (bool)d.GetValue(IsWindowDraggableAreaProperty);
    }

    public static void SetIsWindowDraggableArea(DependencyObject d, bool value)
    {
        d.SetValue(IsWindowDraggableAreaProperty, value);
    }

    private static void OnAttachingIsWindowDraggableArea(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is FrameworkElement frameworkElement)
        {
            if (frameworkElement.IsLoaded)
            {
                BackdropPage? parentPage = VisualTreeHelperExtend.FindParent<BackdropPage>(frameworkElement);
                parentPage?.Window._draggableAreas.Add(frameworkElement);
            }
            else
            {
                frameworkElement.Loaded += (_, _) =>
                {
                    BackdropPage? parentPage = VisualTreeHelperExtend.FindParent<BackdropPage>(frameworkElement);
                    parentPage?.Window._draggableAreas.Add(frameworkElement);
                };
            }
        }
    }

    #endregion

    /// <summary>
    /// Gets or sets the position of the window when first shown.
    /// </summary>
    public WindowStartupLocation WindowStartupLocation { get; set; }

    /// <summary>
    /// Gets whether the Compact Overlay (picture in picture) mode is supported by the operating system.
    /// </summary>
    public bool IsCompactOverlayModeSupported { get; }

    /// <summary>
    /// Raised when the window opened.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Shown;

    /// <summary>
    /// Raised when the window is closing.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? Closing;

    /// <summary>
    /// Raised when the window enters or exits the Compact Overlay mode.
    /// </summary>
    public event TypedEventHandler<BackdropWindow, EventArgs>? CompactOverlayModeChanged;

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

    /// <summary>
    /// Gets whether the window is currently in compact overlay mode.
    /// </summary>
    public partial bool IsInCompactOverlayMode();

    /// <summary>
    /// Tries to enter or exit compact overlay mode.
    /// </summary>
    public partial void TryToggleCompactOverlayMode();

    /// <summary>
    /// Sets the title of the window that the operating system will display (in the task bar, for example).
    /// </summary>
    public partial void SetTitle(string title);
}
