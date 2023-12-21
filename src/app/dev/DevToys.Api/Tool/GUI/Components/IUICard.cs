namespace DevToys.Api;

/// <summary>
/// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
/// </summary>
public interface IUICard : IUIElementWithChildren
{
    /// <summary>
    /// Gets the list of child elements.
    /// </summary>
    IUIElement[]? Children { get; }

    /// <summary>
    /// Raised when <see cref="Children"/> is changed.
    /// </summary>
    event EventHandler? ChildrenChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}")]
internal class UICard : UIElementWithChildren, IUICard
{
    private IUIElement[]? _children;

    internal UICard(string? id)
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

    public IUIElement[]? Children
    {
        get => _children;
        internal set => SetPropertyValue(ref _children, value, ChildrenChanged);
    }

    public event EventHandler? ChildrenChanged;
}

public static partial class GUI
{
    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    public static IUICard Card()
    {
        return Card(null);
    }

    /// <summary>
    /// A component that represents a setting, with a title, description, icon and <see cref="IUIElement"/> for the option value.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUICard Card(string? id)
    {
        return new UICard(id);
    }

    /// <summary>
    /// Sets the <see cref="IUISetting.InteractiveElement"/> of the setting.
    /// </summary>
    public static IUICard InteractiveElement(this IUICard element, IUIElement? uiElement)
    {
        ((UISetting)element).InteractiveElement = uiElement;
        return element;
    }

    /// <summary>
    /// Sets the children to be displayed in the stack.
    /// </summary>
    public static IUICard WithChildren(this IUICard element, params IUIElement[] children)
    {
        ((UICard)element).Children = children;
        return element;
    }

}
