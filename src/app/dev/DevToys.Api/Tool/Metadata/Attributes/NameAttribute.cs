namespace DevToys.Api;

/// <summary>
/// Defines the internal name of this component. This name can be used to explicitly request this component to be invoked.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NameAttribute : Attribute
{
    /// <summary>
    /// Gets the internal name of this component.
    /// </summary>
    public string InternalComponentName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NameAttribute"/> class with the specified internal component name.
    /// </summary>
    /// <param name="internalComponentName">The internal name of the component.</param>
    public NameAttribute(string internalComponentName)
    {
        Guard.IsNotEmpty(internalComponentName);
        InternalComponentName = internalComponentName;
    }
}
