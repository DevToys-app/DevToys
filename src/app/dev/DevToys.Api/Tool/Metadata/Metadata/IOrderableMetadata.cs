namespace DevToys.Api;

/// <summary>
/// Represents an orderable component.
/// </summary>
public interface IOrderableMetadata
{
    /// <summary>
    /// Gets the list of items that should come before this component.
    /// </summary>
    IReadOnlyList<string> Before { get; }

    /// <summary>
    /// Gets the list of items that should come after this component.
    /// </summary>
    IReadOnlyList<string> After { get; }

    /// <summary>
    /// Gets the internal component name.
    /// </summary>
    string InternalComponentName { get; }
}
