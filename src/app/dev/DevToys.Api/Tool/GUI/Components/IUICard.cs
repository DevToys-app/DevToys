namespace DevToys.Api;

/// <summary>
/// A component that represents a empty card and <see cref="IUIElement"/> for the option value.
/// </summary>
public interface IUICard : IUIElement
{
    /// <summary>
    /// Gets the <see cref="IUIElement"/> to display in the item.
    /// </summary>
    IUIElement UIElement { get; }
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}")]
internal class UICard : UIElement, IUICard
{
    internal UICard(string? id, IUIElement uiElement)
        : base(id)
    {
        Guard.IsNotNull(uiElement);
        UIElement = uiElement;
    }

    public IUIElement UIElement { get; }
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents an empty card and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="uiElement">The UI element to display.</param>
    public static IUICard Card(IUIElement uiElement)
    {
        return Card(null, uiElement);
    }

    /// <summary>
    /// A component that represents an empty card and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <param name="uiElement">The UI element to display.</param>
    public static IUICard Card(string? id, IUIElement uiElement)
    {
        return new UICard(id, uiElement);
    }
}
