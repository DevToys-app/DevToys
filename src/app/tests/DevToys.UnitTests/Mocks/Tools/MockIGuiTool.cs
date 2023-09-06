using System.ComponentModel.Composition;

namespace DevToys.UnitTests.Mocks.Tools;

[Export(typeof(IGuiTool))]
[Name("MockTool2")]
[Order(After = "MockTool")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\u0108',
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.Title),
    LongDisplayTitleResourceName = nameof(MockToolResource.Title),
    DescriptionResourceName = nameof(MockToolResource.Title),
    AccessibleNameResourceName = nameof(MockToolResource.Title),
    SearchKeywordsResourceName = nameof(MockToolResource.Title))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed class MockIGuiTool2 : IGuiTool
{
    public UIToolView View => null!;

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("MockTool")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\u0108',
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.Title),
    LongDisplayTitleResourceName = nameof(MockToolResource.Title),
    DescriptionResourceName = nameof(MockToolResource.Title),
    AccessibleNameResourceName = nameof(MockToolResource.Title),
    SearchKeywordsResourceName = nameof(MockToolResource.Title))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class MockIGuiTool : IGuiTool
{
    public UIToolView View => null!;

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("MockTool3-XMLFormatter")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\u0108',
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.TitleXmlFormatter),
    LongDisplayTitleResourceName = nameof(MockToolResource.TitleXmlFormatter),
    DescriptionResourceName = nameof(MockToolResource.TitleXmlFormatter),
    AccessibleNameResourceName = nameof(MockToolResource.TitleXmlFormatter),
    SearchKeywordsResourceName = nameof(MockToolResource.TitleXmlFormatter))]
internal sealed class MockIGuiTool3 : IGuiTool
{
    public UIToolView View => null!;

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("MockTool4-XMLValidator")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\u0108',
    GroupName = "MockGroup",
    ResourceManagerAssemblyIdentifier = nameof(MockResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.UnitTests.Mocks.Tools.MockToolResource",
    ShortDisplayTitleResourceName = nameof(MockToolResource.TitleXmlValidator),
    LongDisplayTitleResourceName = nameof(MockToolResource.TitleXmlValidator),
    DescriptionResourceName = nameof(MockToolResource.TitleXmlValidator),
    AccessibleNameResourceName = nameof(MockToolResource.TitleXmlValidator),
    SearchKeywordsResourceName = nameof(MockToolResource.TitleXmlValidator))]
internal sealed class MockIGuiTool4 : IGuiTool
{
    public UIToolView View => null!;

    public void OnDataReceived(string dataTypeName, object parsedData) { }
}
