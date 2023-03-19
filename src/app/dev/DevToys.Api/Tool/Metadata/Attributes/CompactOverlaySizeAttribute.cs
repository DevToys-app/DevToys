namespace DevToys.Api;

/// <summary>
/// Indicates the size that the window should take in Compact Overlay mode when this <see cref="IGuiTool"/> is active.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class CompactOverlaySizeAttribute : Attribute
{
    public int? CompactOverlayHeight { get; set; }

    public int? CompactOverlayWidth { get; set; }

    public CompactOverlaySizeAttribute(int height, int width)
    {
        CompactOverlayHeight = height;
        CompactOverlayWidth = width;
    }
}
