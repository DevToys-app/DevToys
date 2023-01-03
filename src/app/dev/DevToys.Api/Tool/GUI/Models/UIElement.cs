namespace DevToys.Api;

/// <summary>
/// A base class for all UI elements.
/// </summary>
public abstract class UIElement : UIBase
{
    private bool _isVisible;
    private bool _isEnabled;

    /// <summary>
    /// Creates a new instance of a <see cref="UIElement"/>.
    /// </summary>
    /// <param name="id"><inheritdoc cref="Id"/></param>
    protected UIElement(string id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets or sets whether this element should be visible or hidden in the UI.
    /// </summary>
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            IsVisibleChanged?.Invoke(this, _isVisible);
        }
    }

    /// <summary>
    /// Gets or sets whether this element and its children should be enabled or disabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            IsEnabledChanged?.Invoke(this, _isEnabled);
        }
    }

    /// <summary>
    /// Raised when <see cref="IsVisible"/> is changed.
    /// </summary>
    public event EventHandler<bool>? IsVisibleChanged;

    /// <summary>
    /// Raised when <see cref="IsEnabled"/> is changed.
    /// </summary>
    public event EventHandler<bool>? IsEnabledChanged;
}
