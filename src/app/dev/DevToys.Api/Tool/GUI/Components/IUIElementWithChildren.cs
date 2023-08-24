namespace DevToys.Api;

/// <summary>
/// A base interface for all UI elements that can have children elements.
/// </summary>
public interface IUIElementWithChildren : IUIElement
{
    /// <summary>
    /// Gets the first child element with the specified id.
    /// </summary>
    /// <param name="id">The id of a child element.</param>
    /// <returns>Returns null if the element could not be found. If many elements have the name id, this method returns the first it finds</returns>
    IUIElement? GetChildElementById(string id);
}

internal abstract class UIElementWithChildren : UIElement, IUIElementWithChildren
{
    protected UIElementWithChildren(string? id)
        : base(id)
    {
    }

    protected abstract IEnumerable<IUIElement> GetChildren();

    public IUIElement? GetChildElementById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return GetChildElementById(id, GetChildren());
    }

    internal static IUIElement? GetChildElementById(string id, IEnumerable<IUIElement> children)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (children is not null)
        {
            foreach (IUIElement? element in children)
            {
                if (element is not null && string.Equals(element.Id, id, StringComparison.Ordinal))
                {
                    return element;
                }

                if (element is IUIElementWithChildren uIElementWithChildren)
                {
                    IUIElement? matchingChild = uIElementWithChildren.GetChildElementById(id);
                    if (matchingChild is not null)
                    {
                        return matchingChild;
                    }
                }
            }
        }

        return null;
    }
}
