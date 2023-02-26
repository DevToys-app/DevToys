using DevToys.Api;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Sample;

[Export(typeof(ICommandLineTool))]
[Name("Sample tool")]
[Author("John Doe")]
[CommandName(
    Name = "base64",
    Alias = "b64",
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
    ShortDisplayTitleResourceName = nameof(Sample.CommandDescription),
    LongDisplayTitleResourceName = nameof(Sample.LongTitle),
    DescriptionResourceName = nameof(Sample.Description),
    AccessibleNameResourceName = nameof(Sample.CommandDescription),
    SearchKeywordsResourceName = nameof(Sample.CommandDescription))]
[TargetPlatform(Platform.Windows)] // Optional. Not putting any attribute means every platforms are supported.
[TargetPlatform(Platform.Linux)]
[TargetPlatform(Platform.MacCatalyst)]
[TargetPlatform(Platform.WASM)]
internal sealed class SampleGuiTool : IGuiTool
{
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool2")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool3")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool4")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool5")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool6")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool7")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool8")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool9")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool10")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool11")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool12")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool13")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool14")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool15")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool16")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
}

[Export(typeof(IGuiTool))]
[Name("Sample tool17")]
[Author("John Doe")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = "\uE670",
    GroupName = "Encoders / Decoders",
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Sample.Sample",
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
    public UIElement View => throw new NotImplementedException();
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
