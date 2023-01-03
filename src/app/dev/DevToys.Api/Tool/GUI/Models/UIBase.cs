namespace DevToys.Api;

/// <summary>
/// The base for all UI components.
/// </summary>
public abstract class UIBase
{
    /// <summary>
    /// Creates a new instance of <see cref="AbstractUIBase"/>.
    /// </summary>
    /// <param name="id"><inheritdoc cref="Id"/></param>
    protected UIBase(string id)
    {
        Id = id;
    }

    /// <summary>
    /// An identifier for this component.
    /// </summary>
    public string Id { get; }
}
