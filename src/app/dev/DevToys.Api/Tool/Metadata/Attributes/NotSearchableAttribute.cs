namespace DevToys.Api;

/// <summary>
/// Indicates that the <see cref="IGuiTool"/> can not be searched.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NotSearchableAttribute : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="IGuiTool"/> is not searchable.
    /// </summary>
    public bool NotSearchable { get; } = true;
}
