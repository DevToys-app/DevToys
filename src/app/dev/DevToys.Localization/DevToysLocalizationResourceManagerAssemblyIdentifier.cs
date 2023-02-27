using DevToys.Api;

namespace DevToys.Localization;

[Export(typeof(IResourceManagerAssemblyIdentifier))]
[Name(nameof(DevToysLocalizationResourceManagerAssemblyIdentifier))]
public sealed class DevToysLocalizationResourceManagerAssemblyIdentifier : IResourceManagerAssemblyIdentifier
{
}
