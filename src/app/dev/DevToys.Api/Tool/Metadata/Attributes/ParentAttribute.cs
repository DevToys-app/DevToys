namespace DevToys.Api;

/// <summary>
/// Indicates the parent tool of the current one.
/// The name should corresponds to an existing <see cref="NameAttribute.InternalComponentName"/> value, or null if no parent.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ParentAttribute : Attribute
{
    public string Parent { get; set; }

    public ParentAttribute(string? name)
    {
        Parent = name ?? string.Empty;
    }
}
