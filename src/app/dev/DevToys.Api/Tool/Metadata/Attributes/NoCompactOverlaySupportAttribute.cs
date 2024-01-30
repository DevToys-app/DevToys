namespace DevToys.Api;

/// <summary>
/// Indicates that the <see cref="IGuiTool"/> does not support Compact Overlay mode.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class NoCompactOverlaySupportAttribute : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the <see cref="IGuiTool"/> supports Compact Overlay mode.
    /// </summary>
    public bool NoCompactOverlaySupport { get; } = true;
}
