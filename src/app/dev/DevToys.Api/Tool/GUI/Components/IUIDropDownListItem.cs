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
internal sealed class UIDropDownListItem : IUIDropDownListItem, IEquatable<UIDropDownListItem>
{
    internal UIDropDownListItem(string? text, object? value)
    {
        Text = text;
        Value = value;
    }

    public string? Text { get; }

    public object? Value { get; }

    public static bool operator ==(UIDropDownListItem item1, UIDropDownListItem item2)
        => Equals(item1, item2);

    public static bool operator !=(UIDropDownListItem item1, UIDropDownListItem item2)
        => !Equals(item1, item2);

    public override bool Equals(object? other)
        => Equals(other as UIDropDownListItem);

    public bool Equals(UIDropDownListItem? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Value == other.Value && Text == other.Text)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Text!.GetHashCode(), Value!.GetHashCode());
    }
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
