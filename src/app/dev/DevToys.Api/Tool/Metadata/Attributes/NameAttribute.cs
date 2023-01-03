namespace DevToys.Api;

/// <summary>
/// Defines the internal name of this component. This name can be used to explicitly request this component to be invoked.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NameAttribute : Attribute
{
    public string InternalComponentName { get; }

    public NameAttribute(string internalComponentName)
    {
        Guard.IsNotEmpty(internalComponentName);
        InternalComponentName = internalComponentName;
    }
}
