namespace DevToys.Api;

/// <summary>
/// A component that represents a cell in a <see cref="IUIDataGridRow"/>.
/// </summary>
public interface IUIDataGridCell
{
    /// <summary>
    /// Gets the <see cref="IUIElement"/> to display in the cell.
    /// </summary>
    IUIElement? UIElement { get; }
}

[DebuggerDisplay($"UIElement = {{{nameof(UIElement)}}}")]
internal sealed class UIDataGridCell : IUIDataGridCell
{
    internal UIDataGridCell(IUIElement uiElement)
    {
        UIElement = uiElement;
    }

    public IUIElement? UIElement { get; }
}

public static partial class GUI
{
    /// <summary>
    /// Create component that represents a cell in a <see cref="IUIDataGridRow"/>.
    /// </summary>
    /// <param name="text">The text to display in the cell.</param>
    public static IUIDataGridCell Cell(string? text)
    {
        return
            new UIDataGridCell(
                Label()
                .NeverWrap()
                .Text(text));
    }

    /// <summary>
    /// Create component that represents a cell in a <see cref="IUIDataGridRow"/>.
    /// </summary>
    /// <param name="uiElement">The element to display in the cell.</param>
    public static IUIDataGridCell Cell(IUIElement uiElement)
    {
        return new UIDataGridCell(uiElement);
    }
}
