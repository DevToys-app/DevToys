using System.Reflection;

namespace DevToys.Blazor.BuiltInTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysBlazorResourceManagerAssemblyIdentifier))]
public sealed class DevToysBlazorResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string fluentSystemIconsResourceName = "DevToys.Blazor.Assets.fonts.FluentSystemIcons-Regular.ttf";
        string devToysToolsIconsResourceName = "DevToys.Blazor.Assets.fonts.DevToys-Tools-Icons.ttf";

        Stream fluentSystemIconsResourceStream = assembly.GetManifestResourceStream(fluentSystemIconsResourceName)!;
        Stream devToysToolsIconsResourceStream = assembly.GetManifestResourceStream(devToysToolsIconsResourceName)!;
        return new ValueTask<FontDefinition[]>(
        [
            new FontDefinition("FluentSystemIcons", fluentSystemIconsResourceStream),
            new FontDefinition("DevToys-Tools-Icons", devToysToolsIconsResourceStream)
        ]);
    }
}
