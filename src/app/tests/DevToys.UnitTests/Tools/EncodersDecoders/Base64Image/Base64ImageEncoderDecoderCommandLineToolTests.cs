using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Tools.EncodersDecoders.Base64Image;

namespace DevToys.UnitTests.Tools.EncodersDecoders.Base64Image;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class Base64ImageEncoderDecoderCommandLineToolTests : MefBasedTest
{
    private readonly Base64ImageEncoderDecoderCommandLineTool _tool;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(Base64ImageEncoderDecoder));

    public Base64ImageEncoderDecoderCommandLineToolTests()
        : base(typeof(Base64ImageEncoderDecoderCommandLineTool).Assembly)
    {
        _tool = (Base64ImageEncoderDecoderCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "Base64ImageEncoderDecoder")
            .Value;
    }

    [Fact]
    public async Task EncodeImage()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1.png");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);

        _tool.Input = inputFile;

        using var consoleOutput = new ConsoleOutputMonitor();
        await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

        string expectedResult = File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1_base64.txt")).FullName);
        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task DecodeImageFromText()
    {
        byte[] expectedOutput = File.ReadAllBytes(Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1.png"));
        string filePath = Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1_base64.txt");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);

        _tool.Input = File.ReadAllText(inputFile.FullName);
        _tool.OutputFile = new FileInfo(Path.Combine(_baseTestDataDirectory, "testResult.png"));

        using var consoleOutput = new ConsoleOutputMonitor();
        await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

        byte[] result = File.ReadAllBytes(_tool.OutputFile.FullName);
        expectedOutput.Should().BeEquivalentTo(result);
        _tool.OutputFile.Delete();
    }

    [Fact]
    public async Task DecodeImageFromFile()
    {
        byte[] expectedOutput = File.ReadAllBytes(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1.png")).FullName);
        string filePath = Path.Combine(_baseTestDataDirectory, "PNG_transparency_demonstration_1_base64.txt");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);

        _tool.Input = inputFile;
        _tool.OutputFile = new FileInfo(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "testResult.png")).FullName);

        using var consoleOutput = new ConsoleOutputMonitor();
        await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

        byte[] result = File.ReadAllBytes(_tool.OutputFile.FullName);
        expectedOutput.Should().BeEquivalentTo(result);
        _tool.OutputFile.Delete();
    }
}
