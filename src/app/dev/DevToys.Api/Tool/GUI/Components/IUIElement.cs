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
    /// Raised when <see cref="IsVisible"/> is changed.
    /// </summary>
    event EventHandler? IsVisibleChanged;

    /// <summary>
    /// Raised when <see cref="IsEnabled"/> is changed.
    /// </summary>
    event EventHandler? IsEnabledChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, IsVisible = {{{nameof(IsVisible)}}}, IsEnabled = {{{nameof(IsEnabled)}}}")]
internal abstract class UIElement : IUIElement
{
    private bool _isVisible = true;
    private bool _isEnabled = true;

    protected UIElement(string? id)
    {
        Id = id;
    }

    public string? Id { get; }

    public bool IsVisible
    {
        get => _isVisible;
        internal set
        {
            _isVisible = value;
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        internal set
        {
            _isEnabled = value;
            IsEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? IsVisibleChanged;

    public event EventHandler? IsEnabledChanged;
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
}
