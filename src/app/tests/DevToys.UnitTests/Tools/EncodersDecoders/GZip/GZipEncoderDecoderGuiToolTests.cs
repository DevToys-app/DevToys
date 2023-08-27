using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.EncodersDecoders.GZip;

namespace DevToys.UnitTests.Tools.EncodersDecoders.GZip;

public sealed class GZipEncoderDecoderGuiToolTests : MefBasedTest
{
    private readonly GZipEncoderDecoderGuiTool _tool;
    private readonly UIToolView _toolView;
    private readonly IUIMultiLineTextInput _inputBox;
    private readonly IUIMultiLineTextInput _outputBox;

    public GZipEncoderDecoderGuiToolTests()
        : base(typeof(GZipEncoderDecoderGuiTool).Assembly)
    {
        _tool = (GZipEncoderDecoderGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "GZipEncoderDecoder")
            .Value;

        _toolView = _tool.View;

        _inputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("gzip-input-box");
        _outputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("gzip-output-box");
    }

    [Fact]
    public async Task SwitchConversionMode()
    {
        var conversionMode = (IUISwitch)_toolView.GetChildElementById("gzip-compression-mode-switch");

        _inputBox.Text("<hello world>");
        await _tool.WorkTask;
        _outputBox.Text.Should().Be("H4sIAAAAAAAACrPJSM3JyVcozy/KSbEDAKEb3mcNAAAA");

        conversionMode.Off(); // Switch to Decompress

        await _tool.WorkTask;
        _inputBox.Text.Should().Be("H4sIAAAAAAAACrPJSM3JyVcozy/KSbEDAKEb3mcNAAAA");
        await _tool.WorkTask;
        _outputBox.Text("<hello world>");

        conversionMode.On(); // Switch to Compress

        await _tool.WorkTask;
        _inputBox.Text("<hello world>");
        await _tool.WorkTask;
        _outputBox.Text.Should().Be("H4sIAAAAAAAACrPJSM3JyVcozy/KSbEDAKEb3mcNAAAA");
    }
}
