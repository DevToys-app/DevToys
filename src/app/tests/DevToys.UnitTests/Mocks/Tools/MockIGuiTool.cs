using System.ComponentModel.Composition;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IGuiTool))]
[Name("MockTool2")]
[Order(After = "MockTool")]
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

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("MockTool")]
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

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}
