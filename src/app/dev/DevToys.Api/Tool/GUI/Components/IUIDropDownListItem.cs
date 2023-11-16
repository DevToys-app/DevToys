namespace DevToys.Api;

/// <summary>
/// A component that represents an item in a drop down list.
/// </summary>
public interface IUIDropDownListItem
{
    /// <summary>
    /// Gets the text to display in the item.
    /// </summary>
    string? Text { get; }

    /// <summary>
    /// Gets the value associated to the item.
    /// </summary>
    object? Value { get; }
}

[DebuggerDisplay($"Text = {{{nameof(Text)}}}")]
internal sealed class UIDropDownListItem : IUIDropDownListItem
{
    internal UIDropDownListItem(string? text, object? value)
    {
        Text = text;
        Value = value;
    }

    public string? Text { get; }

    public object? Value { get; }
}

public static partial class GUI
{
    /// <summary>
    /// Create component that represents an item in a drop down list.
    /// </summary>
    /// <param name="value">The value associated to the item. It will be used to generate the text to display in the item</param>
    public static IUIDropDownListItem Item(object? value)
    {
        return Item(text: value?.ToString() ?? string.Empty, value);
    }

    /// <summary>
    /// Create component that represents an item in a drop down list.
    /// </summary>
    /// <param name="text">The text to display in the item.</param>
    /// <param name="value">The value associated to the item.</param>
    public static IUIDropDownListItem Item(string? text, object? value)
    {
        return new UIDropDownListItem(text, value);
    }
}
