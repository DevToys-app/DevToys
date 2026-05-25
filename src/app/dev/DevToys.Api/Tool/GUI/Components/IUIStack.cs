namespace DevToys.Api;

/// <summary>
/// A component that stacks child elements into a single line that can be oriented horizontally or vertically.
/// </summary>
public interface IUIStack : IUIElementWithChildren
{
    /// <summary>
    /// Gets whether this element should use the full height available.
    /// </summary>
    bool UseMaxHeight { get; }

    /// <summary>
    /// Gets a value that indicates the dimension by which child elements are stacked.
    /// Default is <see cref="UIOrientation.Horizontal"/>.
    /// </summary>
    UIOrientation Orientation { get; }

    /// <summary>
    /// Gets a value that indicates the space between stacked elements.
    /// Default is <see cref="UISpacing.Small"/>.
    /// </summary>
    UISpacing Spacing { get; }

    /// <summary>
    /// Gets the list of child elements.
    /// </summary>
    IUIElement[]? Children { get; }

    /// <summary>
    /// Raised when <see cref="Orientation"/> is changed.
    /// </summary>
    event EventHandler? OrientationChanged;

    /// <summary>
    /// Raised when <see cref="Spacing"/> is changed.
    /// </summary>
    event EventHandler? SpacingChanged;

    /// <summary>
    /// Raised when <see cref="Children"/> is changed.
    /// </summary>
    event EventHandler? ChildrenChanged;

    /// <summary>
    /// Raised when <see cref="UseMaxHeight"/> is changed.
    /// </summary>
    event EventHandler? HeightChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, Orientation = {{{nameof(Orientation)}}}")]
internal sealed class UIStack : UIElementWithChildren, IUIStack
{
    private bool _useMaxHeight = false;
    private UIOrientation _orientation = UIOrientation.Horizontal;
    private UISpacing _spacing = UISpacing.Small;
    private IUIElement[]? _children;

    internal UIStack(string? id)
        : base(id)
    {
    }

    protected override IEnumerable<IUIElement> GetChildren()
    {
        if (_children is not null)
        {
            foreach (IUIElement child in _children)
            {
                if (child is not null)
                {
                    yield return child;
                }
            }
        }
    }

    public UIOrientation Orientation
    {
        get => _orientation;
        internal set => SetPropertyValue(ref _orientation, value, OrientationChanged);
    }

    public UISpacing Spacing
    {
        get => _spacing;
        internal set => SetPropertyValue(ref _spacing, value, SpacingChanged);
    }

    public IUIElement[]? Children
    {
        get => _children;
        internal set => SetPropertyValue(ref _children, value, ChildrenChanged);
    }

    public bool UseMaxHeight
    {
        get => _useMaxHeight;
        internal set => SetPropertyValue(ref _useMaxHeight, value, HeightChanged);

    }

    public event EventHandler? OrientationChanged;

    public event EventHandler? SpacingChanged;

    public event EventHandler? ChildrenChanged;

    public event EventHandler? HeightChanged;
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
    /// Sets the children to be displayed in the stack.
    /// </summary>
    public static IUIStack WithChildren(this IUIStack element, params IUIElement[] children)
    {
        ((UIStack)element).Children = children;
        return element;
    }

    /// <summary>
    /// Sets no spacing between children.
    /// </summary>
    public static IUIStack NoSpacing(this IUIStack element)
    {
        ((UIStack)element).Spacing = UISpacing.None;
        return element;
    }

    /// <summary>
    /// Sets a small spacing between children.
    /// </summary>
    public static IUIStack SmallSpacing(this IUIStack element)
    {
        ((UIStack)element).Spacing = UISpacing.Small;
        return element;
    }

    /// <summary>
    /// Sets a medium spacing between children.
    /// </summary>
    public static IUIStack MediumSpacing(this IUIStack element)
    {
        ((UIStack)element).Spacing = UISpacing.Medium;
        return element;
    }

    /// <summary>
    /// Sets a large spacing between children.
    /// </summary>
    public static IUIStack LargeSpacing(this IUIStack element)
    {
        ((UIStack)element).Spacing = UISpacing.Large;
        return element;
    }

    /// <summary>
    /// Sets a large spacing between children.
    /// </summary>
    public static IUIStack UseMaxHeight(this IUIStack element)
    {
        ((UIStack)element).UseMaxHeight = true;
        return element;
    }
}
