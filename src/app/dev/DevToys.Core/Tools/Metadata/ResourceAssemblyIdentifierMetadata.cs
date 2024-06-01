namespace DevToys.Core.Tools.Metadata;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class ResourceAssemblyIdentifierMetadata
{
    public string InternalComponentName { get; }

    public ResourceAssemblyIdentifierMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);
    }
}
