using DevToys.Api;

namespace DevToys.Localization;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysLocalizationResourceManagerAssemblyIdentifier))]
public sealed class DevToysLocalizationResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
