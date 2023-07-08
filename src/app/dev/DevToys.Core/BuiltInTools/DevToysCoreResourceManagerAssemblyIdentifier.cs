using System.Reflection;

namespace DevToys.Core.BuiltInTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysCoreResourceManagerAssemblyIdentifier))]
public sealed class DevToysCoreResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "DevToys.Core.Assets.Fonts.FluentSystemIcons-Regular.ttf";

        Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        return new ValueTask<FontDefinition[]>(new[]
        {
            new FontDefinition("FluentSystemIcons", stream)
        });
    }
}
