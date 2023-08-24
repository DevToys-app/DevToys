namespace DevToys.Api;

/// <summary>
/// A base interface for all UI elements that can have a title / header on top of the element along with children elements.
/// </summary>
public interface IUITitledElementWithChildren : IUITitledElement, IUIElementWithChildren
{
}

internal abstract class UITitledElementWithChildren : UITitledElement, IUIElementWithChildren
{
    protected UITitledElementWithChildren(string? id)
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

        return UIElementWithChildren.GetChildElementById(id, GetChildren());
    }
}
