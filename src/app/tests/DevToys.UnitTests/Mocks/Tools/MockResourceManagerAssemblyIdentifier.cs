using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IResourceAssemblyIdentifier))]
[Name(nameof(MockResourceManagerAssemblyIdentifier))]
internal sealed class MockResourceManagerAssemblyIdentifier : IResourceAssemblyIdentifier
{
    public ValueTask<FontDefinition[]> GetFontDefinitionsAsync()
    {
        throw new NotImplementedException();
    }
}
