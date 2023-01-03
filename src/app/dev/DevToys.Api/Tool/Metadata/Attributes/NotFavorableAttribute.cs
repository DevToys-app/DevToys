namespace DevToys.Api;

/// <summary>
/// Indicates that the <see cref="IGuiTool"/> can not be added to the favorites.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NotFavorableAttribute : Attribute
{
    public bool NotFavorable { get; } = true;
}
