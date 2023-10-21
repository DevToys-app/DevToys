using System.IO;
using System.Threading.Tasks;
using DevToys.Blazor.Core;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.EncodersDecoders.Base64Image;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.EncodersDecoders.Base64Image;

public sealed class Base64ImageEncoderDecoderGuiToolTests : MefBasedTest
{
    private readonly Base64ImageEncoderDecoderGuiTool _tool;
    private readonly UIToolView _toolView;
    private readonly IUIMultiLineTextInput _inputBox;
    private readonly IUIFileSelector _fileSelector;
    private readonly IUIImageViewer _imageViewer;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(Base64ImageEncoderDecoder));

    public Base64ImageEncoderDecoderGuiToolTests()
        : base(typeof(Base64ImageEncoderDecoderGuiTool).Assembly)
    {
        _tool = (Base64ImageEncoderDecoderGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "Base64ImageEncoderDecoder")
            .Value;

        _toolView = _tool.View;

        _inputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("base64-image-input-box");
        _fileSelector = (IUIFileSelector)_toolView.GetChildElementById("base64-image-file-selector");
        _imageViewer = (IUIImageViewer)_toolView.GetChildElementById("base64-image-preview");
    }

    [Fact]
    public async Task EncodeImage()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1.png");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        using var fileStream = new BlazorSandboxedFileReader(inputFile, new MockIFileStorage());

        _fileSelector.WithFiles(fileStream);
        await _tool.WorkTask;
        _inputBox.Text.Should().Be(File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1_base64.txt")).FullName));
    }

    [Fact]
    public async Task DecodeImage()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1_base64.txt");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);

        _inputBox.Text(File.ReadAllText(inputFile.FullName));
        await _tool.WorkTask;
        _imageViewer.ImageSource.Value.Should().BeOfType<FileInfo>();
    }
}
