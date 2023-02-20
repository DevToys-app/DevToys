using DevToys.Api;

namespace DevToys.Core.Tools;

[DebuggerDisplay($"InternalComponentName = {{{nameof(InternalComponentName)}}}")]
public sealed class ResourceManagerAssemblyIdentifierMetadata
{
    public string InternalComponentName { get; }

    public ResourceManagerAssemblyIdentifierMetadata(IDictionary<string, object> metadata)
    {
        InternalComponentName = metadata.GetValueOrDefault(nameof(NameAttribute.InternalComponentName)) as string ?? string.Empty;
        Guard.IsNotNullOrWhiteSpace(InternalComponentName);
    }
}
