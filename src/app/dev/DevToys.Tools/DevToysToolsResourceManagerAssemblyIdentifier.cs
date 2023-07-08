namespace DevToys.Tools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysToolsResourceManagerAssemblyIdentifier))]
public sealed class DevToysToolsResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
