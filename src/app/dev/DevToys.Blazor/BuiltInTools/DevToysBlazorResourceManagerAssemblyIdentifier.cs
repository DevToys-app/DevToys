using System.Reflection;

namespace DevToys.Blazor.BuiltInTools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(DevToysBlazorResourceManagerAssemblyIdentifier))]
public sealed class DevToysBlazorResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "DevToys.Blazor.Assets.fonts.FluentSystemIcons-Regular.ttf";

        Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        return new ValueTask<FontDefinition[]>(new[]
        {
            new FontDefinition("FluentSystemIcons", stream)
        });
    }
}
