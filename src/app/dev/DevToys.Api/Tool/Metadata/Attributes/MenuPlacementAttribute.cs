namespace DevToys.Api;

/// <summary>
/// Indicates where the <see cref="IGuiTool"/> should be displayed in the navigation view.
/// </summary>
[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class MenuPlacementAttribute : Attribute
{
    public MenuPlacement MenuPlacement { get; }

    public MenuPlacementAttribute(MenuPlacement menuPlacement)
    {
        MenuPlacement = menuPlacement;
    }
}
