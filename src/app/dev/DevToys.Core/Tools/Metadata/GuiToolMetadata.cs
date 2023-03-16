using DevToys.Api;
using DevToys.Api.Tool.Metadata.Attributes;

namespace DevToys.Core.Tools.Metadata;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class GuiToolMetadata : IOrderableMetadata
{
    internal static GuiToolMetadata Create(
        string internalComponentName,
        string author,
        string iconFontName,
        string iconGlyph,
        string groupName,
        string shortDisplayTitleResourceName,
        string longDisplayTitleResourceName,
        string resourceManagerAssemblyIdentifier,
        string resourceManagerBaseName)
    {
        return new GuiToolMetadata(
            new Dictionary<string, object>
            {
                { nameof(NameAttribute.InternalComponentName), internalComponentName },
                { nameof(AuthorAttribute.Author), author },
                { nameof(ToolDisplayInformationAttribute.IconFontName), iconFontName },
                { nameof(ToolDisplayInformationAttribute.IconGlyph), iconGlyph },
                { nameof(ToolDisplayInformationAttribute.GroupName),groupName },
                { nameof(ToolDisplayInformationAttribute.ShortDisplayTitleResourceName), shortDisplayTitleResourceName },
                { nameof(ToolDisplayInformationAttribute.LongDisplayTitleResourceName), longDisplayTitleResourceName },
                { nameof(ToolDisplayInformationAttribute.ResourceManagerAssemblyIdentifier), resourceManagerAssemblyIdentifier },
                { nameof(ToolDisplayInformationAttribute.ResourceManagerBaseName), resourceManagerBaseName }
            });
    }

    public GuiToolMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Author = metadata.GetValueOrDefault(nameof(AuthorAttribute.Author)) as string ?? string.Empty;
        IconFontName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.IconFontName)) as string ?? string.Empty;
        IconGlyph = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.IconGlyph)) as string ?? string.Empty;
        ResourceManagerAssemblyIdentifier = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.ResourceManagerAssemblyIdentifier)) as string ?? string.Empty;
        ResourceManagerBaseName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.ResourceManagerBaseName)) as string ?? string.Empty;
        ShortDisplayTitleResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.ShortDisplayTitleResourceName)) as string ?? string.Empty;
        LongDisplayTitleResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.LongDisplayTitleResourceName)) as string ?? string.Empty;
        DescriptionResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.DescriptionResourceName)) as string ?? string.Empty;
        AccessibleNameResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.AccessibleNameResourceName)) as string ?? string.Empty;
        SearchKeywordsResourceName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.SearchKeywordsResourceName)) as string ?? string.Empty;
        GroupName = metadata.GetValueOrDefault(nameof(ToolDisplayInformationAttribute.GroupName)) as string ?? string.Empty;
        Before = metadata.GetValueOrDefault(nameof(OrderAttribute.Before)) as IReadOnlyList<string> ?? Array.Empty<string>();
        After = metadata.GetValueOrDefault(nameof(OrderAttribute.After)) as IReadOnlyList<string> ?? Array.Empty<string>();
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
        Guard.IsNotNullOrWhiteSpace(ShortDisplayTitleResourceName);
        if (MenuPlacement != Api.MenuPlacement.Footer)
        {
            Guard.IsNotNullOrWhiteSpace(GroupName);
        }

        if (!string.IsNullOrEmpty(ShortDisplayTitleResourceName)
            || !string.IsNullOrEmpty(LongDisplayTitleResourceName)
            || !string.IsNullOrEmpty(DescriptionResourceName)
            || !string.IsNullOrEmpty(AccessibleNameResourceName)
            || !string.IsNullOrEmpty(SearchKeywordsResourceName))
        {
            if (string.IsNullOrEmpty(ResourceManagerAssemblyIdentifier)
                || string.IsNullOrEmpty(ResourceManagerBaseName))
            {
                ThrowHelper.ThrowInvalidDataException($"The tool '{InternalComponentName}' has references to one or multiple resource name(s) but does not provide a '{nameof(ToolDisplayInformationAttribute.ResourceManagerAssemblyIdentifier)}' or '{nameof(ToolDisplayInformationAttribute.ResourceManagerBaseName)}'.");
            }
        }

        if (Before.Count > 0)
        {
            var before = new List<string>();
            for (int i = 0; i < Before.Count; i++)
            {
                if (!string.IsNullOrEmpty(Before[i]))
                {
                    before.Add(Before[i]);
                }
            }

            Before = before;
        }

        if (After.Count > 0)
        {
            var after = new List<string>();
            for (int i = 0; i < After.Count; i++)
            {
                if (!string.IsNullOrEmpty(After[i]))
                {
                    after.Add(After[i]);
                }
            }

            After = after;
        }
    }

    public string InternalComponentName { get; }

    public string Author { get; }

    public string IconFontName { get; }

    public string IconGlyph { get; }

    public string ResourceManagerAssemblyIdentifier { get; }

    public string ResourceManagerBaseName { get; }

    public string ShortDisplayTitleResourceName { get; }

    public string LongDisplayTitleResourceName { get; }

    public string DescriptionResourceName { get; }

    public string AccessibleNameResourceName { get; }

    public string SearchKeywordsResourceName { get; }

    public string GroupName { get; }

    public IReadOnlyList<string> Before { get; }

    public IReadOnlyList<string> After { get; }

    public bool NotSearchable { get; }

    public bool NotFavorable { get; }

    public bool NoCompactOverlaySupport { get; }

    public MenuPlacement? MenuPlacement { get; }

    public int? CompactOverlayHeight { get; }

    public int? CompactOverlayWidth { get; }

    public IReadOnlyList<Platform> TargetPlatforms { get; }
}
