using System.IO;
using System.Threading.Tasks;
using DevToys.Blazor.Core;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.EncodersDecoders.Certificate;
using DevToys.UnitTests.Tools.Helpers;

namespace DevToys.UnitTests.Tools.EncodersDecoders.Certificate;

public sealed class CertificateDecoderGuiToolTests : MefBasedTest
{
    private readonly CertificateDecoderGuiTool _tool;
    private readonly UIToolView _toolView;
    private readonly IUIMultiLineTextInput _inputBox;
    private readonly IUISingleLineTextInput _password;
    private readonly IUIMultiLineTextInput _outputBox;
    private readonly IUIFileSelector _fileSelector;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(CertificateDecoder));

    public CertificateDecoderGuiToolTests()
    : base(typeof(CertificateDecoderGuiTool).Assembly)
    {
        _tool = (CertificateDecoderGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "CertificateDecoder")
        .Value;

        _toolView = _tool.View;

        _inputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("certificate-decoder-input");
        _outputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("certificate-decoder-output");
        _password = (IUIPasswordInput)_toolView.GetChildElementById("certificate-decoder-password-input");
        _fileSelector = (IUIFileSelector)_toolView.GetChildElementById("certificate-decoder-file-selector");
    }

    [Fact]
    public async Task ReadCertificateFromFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "PfxWithPassword.pfx");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        using var fileStream = new BlazorSandboxedFileReader(inputFile, new MockIFileStorage());

        _fileSelector.WithFiles(fileStream);

        await Task.Delay(300); // Delay because the IUIFileSelector.SelectedFiles setter is async but we don't wait for it to finish. Could be improved in the future.
        await _tool.WorkTask;

        _outputBox.Text.Should().Be(CertificateDecoder.InvalidPasswordError);

        _password.Text("test1234");
        await _tool.WorkTask;

        string expectResult = CertificateHelperTests.CleanDateTimes(File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "CertDecoded.txt")).FullName));
        CertificateHelperTests.CleanDateTimes(_outputBox.Text).Should().Be(expectResult);
    }

    [Fact]
    public async Task ReadCertificateFromText()
    {
        string input = File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "PemCertPublic.txt")).FullName);

        _inputBox.Text(input);

        await _tool.WorkTask;

        string expectResult = CertificateHelperTests.CleanDateTimes(File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "CertDecoded.txt")).FullName));
        CertificateHelperTests.CleanDateTimes(_outputBox.Text).Should().Be(expectResult);
    }

    [Fact]
    public async Task InvalidCertificate()
    {
        _inputBox.Text("hello");

        await _tool.WorkTask;

        _outputBox.Text.Should().Be(CertificateDecoder.UnsupportedFormatError);
    }
}
