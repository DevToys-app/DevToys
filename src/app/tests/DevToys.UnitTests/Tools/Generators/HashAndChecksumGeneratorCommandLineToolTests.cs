using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Tools.Generators.HashAndChecksum;

namespace DevToys.UnitTests.Tools.Generators;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class HashAndChecksumGeneratorCommandLineToolTests : MefBasedTest
{
    private readonly HashAndChecksumGeneratorCommandLineTool _tool;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(HashAndChecksumGenerator));

    public HashAndChecksumGeneratorCommandLineToolTests()
        : base(typeof(HashAndChecksumGeneratorCommandLineTool).Assembly)
    {
        _tool = (HashAndChecksumGeneratorCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "HashAndChecksumGenerator")
            .Value;
        _tool.Silent = true;
    }

    [Fact]
    public async Task HashText()
    {
        _tool.Input = "hello";

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be("5d41402abc4b2a76b9719d911017c592");
    }

    [Fact]
    public async Task HashFile()
    {
        _tool.Input = TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "File.txt"));

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be("3e25960a79dbc69b674cd4ec67a72c62");
    }

    [Fact]
    public async Task HashNonExistentFileOutputsError()
    {
        _tool.Input = TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "DoesNotExist.txt"));
        ((MockIFileStorage)MefProvider.Import<IFileStorage>()).FileExistsResult = false;

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(-1);

        string result = consoleOutput.GetError().Trim();
        result.Should().Be(HashAndChecksumGenerator.InvalidInputOrFileCommand);
    }

    [Fact]
    public async Task HashNoAlgorithm()
    {
        _tool.Input = "Hello";
        _tool.HashAlgorithm = System.Security.Authentication.HashAlgorithmType.None;

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(-1);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HashUppercase()
    {
        _tool.Input = "Hello";
        _tool.Uppercase = true;

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be("8B1A9953C4611296A827ABF8C47804D7");
    }

    [Fact]
    public async Task HashHmac()
    {
        _tool.Input = "Hello";
        _tool.HmacSecretKey = "secret";

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be("f386803b6309719e8adad577a10d77c3");
    }

    [Fact]
    public async Task HashChecksum()
    {
        _tool.Input = "Hello";
        _tool.ChecksumVerification = "8B1A9953C4611296A827ABF8C47804D7";

        using var consoleOutput = new ConsoleOutputMonitor();
        int exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be(HashAndChecksumGenerator.ChecksumVerificationSucceeded);

        _tool.Input = "Hello";
        _tool.ChecksumVerification = "foo";

        exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(-1);

        result = consoleOutput.GetOutput().Trim();
        result.Should().Be(HashAndChecksumGenerator.ChecksumVerificationFailed);

        _tool.Input = "Hello";
        _tool.ChecksumVerification = TestDataProvider.GetFile(Path.Combine(_baseTestDataDirectory, "Checksum.txt"));

        exitCode = await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);
        exitCode.Should().Be(0);

        result = consoleOutput.GetOutput().Trim();
        result.Should().Be(HashAndChecksumGenerator.ChecksumVerificationSucceeded);
    }
}
