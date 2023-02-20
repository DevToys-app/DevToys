using System.ComponentModel.Composition;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IResourceManagerAssemblyIdentifier))]
[Name(nameof(MockResourceManagerAssemblyIdentifier))]
internal sealed class MockResourceManagerAssemblyIdentifier : IResourceManagerAssemblyIdentifier
{
}
