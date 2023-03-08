using System.ComponentModel.Composition;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IGuiTool))]
[Name("MockTool2")]
[Order(After = "MockTool")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\u0108",
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.Title),
    LongDisplayTitleResourceName = nameof(MockToolResource.Title),
    DescriptionResourceName = nameof(MockToolResource.Title),
    AccessibleNameResourceName = nameof(MockToolResource.Title),
    SearchKeywordsResourceName = nameof(MockToolResource.Title))]
internal sealed class MockIGuiTool2 : IGuiTool
{
    public IUIElement View => null!;
}

[Export(typeof(IGuiTool))]
[Name("MockTool")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\u0108",
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.Title),
    LongDisplayTitleResourceName = nameof(MockToolResource.Title),
    DescriptionResourceName = nameof(MockToolResource.Title),
    AccessibleNameResourceName = nameof(MockToolResource.Title),
    SearchKeywordsResourceName = nameof(MockToolResource.Title))]
internal sealed class MockIGuiTool : IGuiTool
{
    public IUIElement View => null!;
}
