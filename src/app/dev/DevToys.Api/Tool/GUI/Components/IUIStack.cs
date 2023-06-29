namespace DevToys.Api;

/// <summary>
/// A component that stacks child elements into a single line that can be oriented horizontally or vertically.
/// </summary>
public interface IUIStack : IUIElement
{
    /// <summary>
    /// Gets a value that indicates the dimension by which child elements are stacked.
    /// Default is horizontal.
    /// </summary>
    UIOrientation Orientation { get; }

    /// <summary>
    /// Gets the list of child elements.
    /// </summary>
    IUIElement[]? Children { get; }

    /// <summary>
    /// Raised when <see cref="Orientation"/> is changed.
    /// </summary>
    public event EventHandler? OrientationChanged;

    /// <summary>
    /// Raised when <see cref="Children"/> is changed.
    /// </summary>
    public event EventHandler? ChildrenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Orientation = {{{nameof(Orientation)}}}")]
internal sealed class UIStack : UIElement, IUIStack
{
    private UIOrientation _orientation;
    private IUIElement[]? _children;

    internal UIStack(string? id)
        : base(id)
    {
    }

    public UIOrientation Orientation
    {
        get => _orientation;
        internal set => SetPropertyValue(ref _orientation, value, OrientationChanged);
    }

    public IUIElement[]? Children
    {
        get => _children;
        internal set => SetPropertyValue(ref _children, value, ChildrenChanged);
    }

    public event EventHandler? OrientationChanged;

    public event EventHandler? ChildrenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that stacks child elements into a single line that can be oriented horizontally or vertically.
    /// </summary>
    public static IUIStack Stack()
    {
        return Stack(null);
    }

    /// <summary>
    /// Create a component that stacks child elements into a single line that can be oriented horizontally or vertically.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIStack Stack(string? id)
    {
        return new UIStack(id);
    }

    /// <summary>
    /// Organize the <see cref="IUIStack.Children"/> vertically.
    /// </summary>
    public static IUIStack Vertical(this IUIStack element)
    {
        ((UIStack)element).Orientation = UIOrientation.Vertical;
        return element;
    }

    /// <summary>
    /// Organize the <see cref="IUIStack.Children"/> horizontally.
    /// </summary>
    public static IUIStack Horizontal(this IUIStack element)
    {
        ((UIStack)element).Orientation = UIOrientation.Horizontal;
        return element;
    }

    /// <summary>
    /// Set the children to be displayed in the stack.
    /// </summary>
    public static IUIStack WithChildren(this IUIStack element, params IUIElement[] children)
    {
        ((UIStack)element).Children = children;
        return element;
    }
}
