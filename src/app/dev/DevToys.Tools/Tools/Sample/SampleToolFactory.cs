using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DevToys.Tools.Tools.Sample;

[Export(typeof(ICommandLineTool))]
[Name("Sample tool")]
[CommandName(
    Name = "base64",
    Alias = "b64",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    DescriptionResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "file",
        Alias = "f",
        IsRequired = true,
        DescriptionResourceName = nameof(Sample.FileOptionDescription))]
    private FileInfo? File { get; set; }

    [CommandLineOption(
        Name = "utf8",
        DescriptionResourceName = nameof(Sample.Utf8OptionDescription))]
    private bool Utf8 { get; set; } = true; // Default value is true.

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        Console.WriteLine("Dummy output.");
        return new ValueTask<int>(0);
    }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.LongTitle),
    DescriptionResourceName = nameof(Sample.Description),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class SampleGuiTool : IGuiTool
{
    private readonly IUIMultilineLineTextInput _editor = MultilineTextInput().Language("json").CanCopyWhenEditable();

    public IUIElement View
        => _editor;

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Json && parsedData is Tuple<JToken, string> strongTypedParsedData)
        {
            _editor.Text(strongTypedParsedData.Item2);
            _editor.Highlight(new TextSpan(3, 6));
        }
    }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool2")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool2 : IGuiTool
{
    private readonly IUIMultilineLineTextInput _editor = MultilineTextInput().CanCopyWhenEditable();

    public IUIElement View
        => _editor;

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool3")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool3 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool4")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool4 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool5")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool5 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool6")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool6 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool7")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool7 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool8")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool8 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool9")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool9 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool10")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool10 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool11")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool11 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool12")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool12 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool13")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool13 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool14")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool14 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool15")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool15 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool16")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool16 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool17")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.CommandDescription),
    DescriptionResourceName = nameof(Sample.CommandDescription),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool17 : IGuiTool
{
    public IUIElement View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData) { }
}

[Export(typeof(GuiToolGroup))]
[Name("Encoders / Decoders")]
internal sealed class EncodersDecodersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal EncodersDecodersGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = "\uE670";
        DisplayTitle = Sample.CommandDescription;
        AccessibleName = Sample.CommandDescription;
    }
}
