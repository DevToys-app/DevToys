namespace DevToys.Api;

/// <summary>
/// A base interface for all UI elements that can have a title / header on top of the element.
/// </summary>
public interface IUITitledElement : IUIElement
{
    /// <summary>
    /// Gets a title to display for this element.
    /// </summary>
    string? DisplayTitle { get; }

    /// <summary>
    /// Raised when <see cref="DisplayTitle"/> is changed.
    /// </summary>
    event EventHandler? DisplayTitleChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(DisplayTitle)}}}")]
internal abstract class UITitledElement : UIElement, IUITitledElement
{
    private string? _title;

    protected UITitledElement(string? id)
        : base(id)
    {
    }

    public string? DisplayTitle
    {
        get => _title;
        internal set
        {
            _title = value;
            DisplayTitleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DisplayTitleChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Set the title of the element.
    /// </summary>
    public static T Title<T>(this T element, string? title) where T : IUITitledElement
    {
        if (element is UITitledElement strongElement)
        {
            strongElement.DisplayTitle = title;
        }
        return element;
    }
}
