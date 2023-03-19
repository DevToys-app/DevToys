using DevToys.Api;

namespace DevToys.CLI;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
internal sealed class CommandLineToolMetadata
{
    public string InternalComponentName { get; }

    public string Name { get; }

    public string Alias { get; }

    public string DescriptionResourceName { get; }

    public string ResourceManagerBaseName { get; }

    public IReadOnlyList<Platform> TargetPlatforms { get; }

    public CommandLineToolMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Name = metadata.GetValueOrDefault(nameof(CommandNameAttribute.Name)) as string ?? string.Empty;
        Alias = metadata.GetValueOrDefault(nameof(CommandNameAttribute.Alias)) as string ?? string.Empty;
        DescriptionResourceName = metadata.GetValueOrDefault(nameof(CommandNameAttribute.DescriptionResourceName)) as string ?? string.Empty;
        ResourceManagerBaseName = metadata.GetValueOrDefault(nameof(CommandNameAttribute.ResourceManagerBaseName)) as string ?? string.Empty;
        TargetPlatforms = metadata.GetValueOrDefault(nameof(TargetPlatformAttribute.TargetPlatform)) as IReadOnlyList<Platform> ?? Array.Empty<Platform>();
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);
        Guard.IsNotNullOrWhiteSpace(Name);
    }
}
