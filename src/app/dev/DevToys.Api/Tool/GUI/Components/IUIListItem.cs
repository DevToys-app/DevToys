namespace DevToys.Api;

/// <summary>
/// A component that represents an item in a list.
/// </summary>
public interface IUIListItem
{
    /// <summary>
    /// Gets the <see cref="IUIElement"/> to display in the item.
    /// </summary>
    IUIElement UIElement { get; }

    /// <summary>
    /// Gets the value associated to the item.
    /// </summary>
    object? Value { get; }
}

[DebuggerDisplay($"Value = {{{nameof(Value)}}}")]
internal sealed class UIListItem : IUIListItem
{
    internal UIListItem(IUIElement uiElement, object? value)
    {
        Guard.IsNotNull(uiElement);
        UIElement = uiElement;
        Value = value;
    }

    public IUIElement UIElement { get; }

    public object? Value { get; }
}

public static partial class GUI
{
    /// <summary>
    /// Create component that represents an item in a list.
    /// </summary>
    /// <param name="uiElement">The element to display in the item.</param>
    /// <param name="value">The value associated to the item.</param>
    public static IUIListItem Item(IUIElement uiElement, object? value)
    {
        return new UIListItem(uiElement, value);
    }
}
