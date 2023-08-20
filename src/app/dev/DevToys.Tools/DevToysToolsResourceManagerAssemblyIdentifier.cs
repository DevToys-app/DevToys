using System.Reflection;

namespace DevToys.Tools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysToolsResourceManagerAssemblyIdentifier))]
public sealed class DevToysToolsResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "DevToys.Tools.Assets.fonts.DevToys-Tools-Icons.ttf";

        Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        return new ValueTask<FontDefinition[]>(new[]
        {
            new FontDefinition("DevToys-Tools-Icons", stream)
        });
    }
}
