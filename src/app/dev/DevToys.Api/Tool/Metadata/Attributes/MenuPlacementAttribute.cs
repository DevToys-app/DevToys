namespace DevToys.Api;

/// <summary>
/// Indicates where the <see cref="IGuiTool"/> should be displayed in the navigation view.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class MenuPlacementAttribute : Attribute
{
    /// <summary>
    /// Gets the menu placement for the <see cref="IGuiTool"/>.
    /// </summary>
    public MenuPlacement MenuPlacement { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuPlacementAttribute"/> class with the specified menu placement.
    /// </summary>
    /// <param name="menuPlacement">The menu placement for the <see cref="IGuiTool"/>.</param>
    public MenuPlacementAttribute(MenuPlacement menuPlacement)
    {
        MenuPlacement = menuPlacement;
    }
}
