namespace DevToys.Api;

/// <summary>
/// A component that stacks child elements horizontally and stack them into multiple lines if there's not enough space to keep everything on a single line.
/// </summary>
public interface IUIWrap : IUIElementWithChildren
{
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
    /// Raised when <see cref="Spacing"/> is changed.
    /// </summary>
    event EventHandler? SpacingChanged;

    /// <summary>
    /// Raised when <see cref="Children"/> is changed.
    /// </summary>
    event EventHandler? ChildrenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}")]
internal sealed class UIWrap : UIElementWithChildren, IUIWrap
{
    private UISpacing _spacing = UISpacing.Small;
    private IUIElement[]? _children;

    internal UIWrap(string? id)
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

    public event EventHandler? SpacingChanged;

    public event EventHandler? ChildrenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// Create a component that stacks child elements horizontally and stack them into multiple lines if there's not enough space to keep everything on a single line.
    /// </summary>
    public static IUIWrap Wrap()
    {
        return Wrap(null);
    }

    /// <summary>
    /// Create a component that stacks child elements horizontally and stack them into multiple lines if there's not enough space to keep everything on a single line.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIWrap Wrap(string? id)
    {
        return new UIWrap(id);
    }

    /// <summary>
    /// Sets the children to be displayed in the stack.
    /// </summary>
    public static IUIWrap WithChildren(this IUIWrap element, params IUIElement[] children)
    {
        ((UIWrap)element).Children = children;
        return element;
    }

    /// <summary>
    /// Sets no spacing between children.
    /// </summary>
    public static IUIWrap NoSpacing(this IUIWrap element)
    {
        ((UIWrap)element).Spacing = UISpacing.None;
        return element;
    }

    /// <summary>
    /// Sets a small spacing between children.
    /// </summary>
    public static IUIWrap SmallSpacing(this IUIWrap element)
    {
        ((UIWrap)element).Spacing = UISpacing.Small;
        return element;
    }

    /// <summary>
    /// Sets a medium spacing between children.
    /// </summary>
    public static IUIWrap MediumSpacing(this IUIWrap element)
    {
        ((UIWrap)element).Spacing = UISpacing.Medium;
        return element;
    }

    /// <summary>
    /// Sets a large spacing between children.
    /// </summary>
    public static IUIWrap LargeSpacing(this IUIWrap element)
    {
        ((UIWrap)element).Spacing = UISpacing.Large;
        return element;
    }
}
