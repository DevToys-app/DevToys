using DevToys.Api;
using DevToys.Api.Tool.Metadata.Attributes;

namespace DevToys.Core.Tools;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class GuiToolMetadata
{
    public string InternalComponentName { get; }

    public string Author { get; }

    public string IconFontName { get; }

    public string IconGlyph { get; }

    public string ResourceManagerBaseName { get; }

    public string MenuDisplayTitleResourceName { get; }

    public string SearchDisplayTitleResourceName { get; }

    public string DescriptionResourceName { get; }

    public string AccessibleNameResourceName { get; }

    public string SearchKeywordsResourceName { get; }

    public string ParentTool { get; }

    public string Before { get; }

    public string After { get; }

    public bool NotSearchable { get; }

    public bool NotFavorable { get; }

    public bool NoCompactOverlaySupport { get; }

    public MenuPlacement? MenuPlacement { get; }

    public int? CompactOverlayHeight { get; }

    public int? CompactOverlayWidth { get; }

    public IReadOnlyList<Platform> TargetPlatforms { get; }

    public GuiToolMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Author = metadata.GetValueOrDefault(nameof(AuthorAttribute.Author)) as string ?? string.Empty;
        IconFontName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.IconFontName)) as string ?? string.Empty;
        IconGlyph = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.IconGlyph)) as string ?? string.Empty;
        ResourceManagerBaseName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.ResourceManagerBaseName)) as string ?? string.Empty;
        MenuDisplayTitleResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.MenuDisplayTitleResourceName)) as string ?? string.Empty;
        SearchDisplayTitleResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.SearchDisplayTitleResourceName)) as string ?? string.Empty;
        DescriptionResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.DescriptionResourceName)) as string ?? string.Empty;
        AccessibleNameResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.AccessibleNameResourceName)) as string ?? string.Empty;
        SearchKeywordsResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.SearchKeywordsResourceName)) as string ?? string.Empty;
        ParentTool = metadata.GetValueOrDefault(nameof(ParentAttribute.Parent)) as string ?? string.Empty;
        Before = metadata.GetValueOrDefault(nameof(OrderAttribute.Before)) as string ?? string.Empty;
        After = metadata.GetValueOrDefault(nameof(OrderAttribute.After)) as string ?? string.Empty;
        NotSearchable = metadata.GetValueOrDefault(nameof(NotSearchableAttribute.NotSearchable)) as bool? ?? false;
        NotFavorable = metadata.GetValueOrDefault(nameof(NotFavorableAttribute.NotFavorable)) as bool? ?? false;
        NoCompactOverlaySupport = metadata.GetValueOrDefault(nameof(NoCompactOverlaySupportAttribute.NoCompactOverlaySupport)) as bool? ?? false;
        MenuPlacement = metadata.GetValueOrDefault(nameof(MenuPlacementAttribute.MenuPlacement)) as MenuPlacement?;
        CompactOverlayHeight = metadata.GetValueOrDefault(nameof(CompactOverlaySizeAttribute.CompactOverlayHeight)) as int?;
        CompactOverlayWidth = metadata.GetValueOrDefault(nameof(CompactOverlaySizeAttribute.CompactOverlayWidth)) as int?;
        TargetPlatforms = metadata.GetValueOrDefault(nameof(TargetPlatformAttribute.TargetPlatform)) as IReadOnlyList<Platform> ?? Array.Empty<Platform>();
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);
        Guard.IsNotNullOrWhiteSpace(Author);
        Guard.IsNotNullOrWhiteSpace(IconFontName);
        Guard.IsNotNullOrWhiteSpace(IconGlyph);
        Guard.IsNotNullOrWhiteSpace(ResourceManagerBaseName);
        Guard.IsNotNullOrWhiteSpace(MenuDisplayTitleResourceName);
    }
}
