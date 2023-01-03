namespace DevToys.Api;

/// <summary>
/// A base class for all UI elements that can have a title / header on top of the element.
/// </summary>
public abstract class UITitledElement : UIElement
{
    private string? _title;

    /// <summary>
    /// Creates a new instance of a <see cref="UITitledElement"/>.
    /// </summary>
    /// <param name="id"><inheritdoc cref="Id"/></param>
    protected UITitledElement(string id)
        : base(id)
    {
    }

    /// <summary>
    /// Gets or sets a title to display for this element.
    /// </summary>
    public string? Title
    {
        get => _title;
        set
        {
            _title = value;
            TitleChanged?.Invoke(this, _title);
        }
    }

    /// <summary>
    /// Raised when <see cref="Title"/> is changed.
    /// </summary>
    public event EventHandler<string?>? TitleChanged;
}
