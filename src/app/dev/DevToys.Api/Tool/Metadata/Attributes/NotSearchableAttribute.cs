namespace DevToys.Api;

/// <summary>
/// Indicates that the <see cref="IGuiTool"/> can not be searched.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NotSearchableAttribute : Attribute
{
    public bool NotSearchable { get; } = true;
}
