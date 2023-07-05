namespace DevToys.Api;

/// <summary>
/// A base interface for all UI elements that can have a title / header on top of the element.
/// </summary>
public interface IUITitledElement : IUIElement
{
    /// <summary>
    /// Gets a title to display for this element.
    /// </summary>
    string? Title { get; }

    /// <summary>
    /// Raised when <see cref="Title"/> is changed.
    /// </summary>
    event EventHandler? TitleChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Title = {{{nameof(Title)}}}")]
internal abstract class UITitledElement : UIElement, IUITitledElement
{
    private string? _title;

    protected UITitledElement(string? id)
        : base(id)
    {
    }

    public string? Title
    {
        get => _title;
        internal set => SetPropertyValue(ref _title, value, TitleChanged);
    }

    public event EventHandler? TitleChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Sets the title of the element.
    /// </summary>
    public static T Title<T>(this T element, string? title) where T : IUITitledElement
    {
        if (element is UITitledElement strongElement)
        {
            strongElement.Title = title;
        }
        return element;
    }
}
