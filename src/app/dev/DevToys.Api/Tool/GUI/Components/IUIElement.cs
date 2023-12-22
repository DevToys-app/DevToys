namespace DevToys.Api;

/// <summary>
/// A base interface for all UI elements.
/// </summary>
public interface IUIElement
{
    /// <summary>
    /// An optional unique identifier for this UI element.
    /// This Id can be helpful to identify some behavior related to a component in the logs of the app.
    /// </summary>
    string? Id { get; }

    /// <summary>
    /// Gets whether this element should be visible or hidden in the UI.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets whether this element and its children should be enabled or disabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets how the element should align horizontally.
    /// </summary>
    UIHorizontalAlignment HorizontalAlignment { get; }

    /// <summary>
    /// Gets how the element should align vertically.
    /// </summary>
    UIVerticalAlignment VerticalAlignment { get; }

    /// <summary>
    /// Raised when <see cref="IsVisible"/> is changed.
    /// </summary>
    event EventHandler? IsVisibleChanged;

    /// <summary>
    /// Raised when <see cref="IsEnabled"/> is changed.
    /// </summary>
    event EventHandler? IsEnabledChanged;

    /// <summary>
    /// Raised when <see cref="HorizontalAlignment"/> is changed.
    /// </summary>
    event EventHandler? HorizontalAlignmentChanged;

    /// <summary>
    /// Raised when <see cref="VerticalAlignment"/> is changed.
    /// </summary>
    event EventHandler? VerticalAlignmentChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, IsVisible = {{{nameof(IsVisible)}}}, IsEnabled = {{{nameof(IsEnabled)}}}")]
internal abstract class UIElement : ViewModelBase, IUIElement
{
    private bool _isVisible = true;
    private bool _isEnabled = true;
    private UIHorizontalAlignment _horizontalAlignment = UIHorizontalAlignment.Stretch;
    private UIVerticalAlignment _verticalAlignment = UIVerticalAlignment.Stretch;

    protected UIElement(string? id)
    {
        Id = id;
    }

    public string? Id { get; }

    public bool IsVisible
    {
        get => _isVisible;
        internal set => SetPropertyValue(ref _isVisible, value, IsVisibleChanged);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        internal set => SetPropertyValue(ref _isEnabled, value, IsEnabledChanged);
    }

    public UIHorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        internal set => SetPropertyValue(ref _horizontalAlignment, value, HorizontalAlignmentChanged);
    }

    public UIVerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        internal set => SetPropertyValue(ref _verticalAlignment, value, VerticalAlignmentChanged);
    }

    public event EventHandler? IsVisibleChanged;

    public event EventHandler? IsEnabledChanged;

    public event EventHandler? HorizontalAlignmentChanged;

    public event EventHandler? VerticalAlignmentChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Hides the element.
    /// </summary>
    public static T Hide<T>(this T element) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.IsVisible = false;
        }
        return element;
    }

    /// <summary>
    /// Shows the element.
    /// </summary>
    public static T Show<T>(this T element) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.IsVisible = true;
        }
        return element;
    }

    /// <summary>
    /// Disable the element and its child elements.
    /// </summary>
    public static T Disable<T>(this T element) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.IsEnabled = false;
        }
        return element;
    }

    /// <summary>
    /// Enable the element and its child elements.
    /// </summary>
    public static T Enable<T>(this T element) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.IsEnabled = true;
        }
        return element;
    }

    /// <summary>
    /// Align the element horizontally.
    /// </summary>
    public static T AlignHorizontally<T>(this T element, UIHorizontalAlignment alignment) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.HorizontalAlignment = alignment;
        }
        return element;
    }

    /// <summary>
    /// Align the element vertically.
    /// </summary>
    public static T AlignVertically<T>(this T element, UIVerticalAlignment alignment) where T : IUIElement
    {
        if (element is UIElement strongElement)
        {
            strongElement.VerticalAlignment = alignment;
        }
        return element;
    }
}
