namespace DevToys.Api;

/// <summary>
/// Indicates that the <see cref="IGuiTool"/> does not support Compact Overlay mode.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NoCompactOverlaySupportAttribute : Attribute
{
    public bool NoCompactOverlaySupport { get; } = true;
}
