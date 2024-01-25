using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Tools.EncodersDecoders.Certificate;
using DevToys.UnitTests.Tools.Helpers;

namespace DevToys.UnitTests.Tools.EncodersDecoders.Certificate;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class CertificateDecoderCommandLineToolTests : MefBasedTest
{
    private readonly CertificateDecoderCommandLineTool _tool;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(CertificateDecoder));

    public CertificateDecoderCommandLineToolTests()
        : base(typeof(CertificateDecoderCommandLineTool).Assembly)
    {
        _tool = (CertificateDecoderCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "CertificateDecoder")
            .Value;
    }

    [Fact]
    public async Task ReadCertificateFromFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "PfxWithPassword.pfx");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);

        _tool.Input = inputFile;

        using (var consoleOutput = new ConsoleOutputMonitor())
        {
            await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

            string result = consoleOutput.GetError().Trim();
            result.Should().Be(CertificateDecoder.InvalidPasswordError);
        }

        _tool.Password = "test1234";

        using (var consoleOutput = new ConsoleOutputMonitor())
        {
            await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

            string expectedResult = CertificateHelperTests.CleanDateTimes(File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "CertDecoded.txt")).FullName)).Trim();
            string result = CertificateHelperTests.CleanDateTimes(consoleOutput.GetOutput()).Trim();
            result.Should().Be(expectedResult);
        }
    }

    [Fact]
    public async Task ReadCertificateFromText()
    {
        string input = File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "PemCertPublic.txt")).FullName);

        _tool.Input = input;

        using (var consoleOutput = new ConsoleOutputMonitor())
        {
            await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

            string expectedResult = CertificateHelperTests.CleanDateTimes(File.ReadAllText(TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "CertDecoded.txt")).FullName)).Trim();
            string result = CertificateHelperTests.CleanDateTimes(consoleOutput.GetOutput()).Trim();
            result.Should().Be(expectedResult);
        }
    }

    [Fact]
    public async Task InvalidCertificate()
    {
        _tool.Input = "hello";

        using (var consoleOutput = new ConsoleOutputMonitor())
        {
            await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

            string result = consoleOutput.GetError().Trim();
            result.Should().Be(CertificateDecoder.UnsupportedFormatError);
        }
    }
}
