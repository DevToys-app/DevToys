namespace DevToys.Api;

/// <summary>
/// Defines the different spacing to apply between element in the layout.
/// </summary>
public enum UISpacing
{
    /// <summary>
    /// No space between element. Use this space between element or text that should be closed from each other.
    /// </summary>
    None = 0,

    /// <summary>
    /// A small space between element. Use this space between elements of a same group.
    /// </summary>
    Small = 1,

    /// <summary>
    /// A medium space between element. Use this space to separate groups of elements.
    /// </summary>
    Medium = 2,

    /// <summary>
    /// A large space between element. Use this space to separate sections.
    /// </summary>
    Large = 3
}
