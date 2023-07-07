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

[Export(typeof(GuiToolGroup))]
[Name("Encoders / Decoders")]
internal sealed class EncodersDecodersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal EncodersDecodersGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE670';
        DisplayTitle = Sample.CommandDescription;
        AccessibleName = Sample.CommandDescription;
    }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE670',
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
    //private readonly IUIMultilineLineTextInput _editor = MultilineTextInput().Language("json").CanCopyWhenEditable();
    private readonly IUIButton _button = Button();
    private readonly IUIButton _button2 = Button();
    private readonly IUIButton _button3 = Button();
    private readonly IUIButton _button4 = Button();
    private readonly IUIButton _button5 = Button();
    private readonly IUIButton _button6 = Button();
    private readonly IUIButton _button7 = Button();
    private readonly IUIButton _button8 = Button();
    private readonly IUIButton _button9 = Button();
    private readonly IUIButton _button10 = Button();
    private readonly IUIButton _button11 = Button();
    private readonly IUIButton _button12 = Button();
    private readonly IUIStack _mainStack = Stack();
    private readonly IUIStack _subStack = Stack();

    public UIToolView View =>
        new(
            isScrollable: false,
            _mainStack
                .Vertical()
                .WithChildren(
                    _button.Text("I'm in a StackPanel. Click me to disable the parent StackPanel and all its children").OnClick(OnClick),
                    _subStack
                        .Vertical()
                        .WithChildren(
                            _button2.Text("Hello !"),
                            _button3.Text("Hello !"),
                            _button4.Text("Hello !"),
                            _button5.Text("Hello !"),
                            _button6.Text("Hello !"),
                            _button7.Text("Hello !"),
                            _button8.Text("Hello !"),
                            _button9.Text("Hello !"),
                            _button10.Text("Hello !"),
                            _button11.Text("Hello !"),
                            _button12.Text("Hello !"))));

    private ValueTask OnClick()
    {
        _button.Text("Done !");
        //_button.AccentAppearance();
        _mainStack.Disable();
        //_button.Hide();
        return ValueTask.CompletedTask;
    }

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Json && parsedData is Tuple<JToken, string> strongTypedParsedData)
        {
            //_editor.Text(strongTypedParsedData.Item2);
            //_editor.Highlight(new TextSpan(3, 6));
        }
    }
}

[Export(typeof(IGuiTool))]
[Name("Sample tool2")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE670',
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
    public UIToolView View => null!;

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }
}
