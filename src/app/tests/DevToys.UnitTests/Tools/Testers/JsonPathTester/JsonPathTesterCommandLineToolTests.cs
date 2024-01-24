using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Tools.Testers.JsonPathTester;

namespace DevToys.UnitTests.Tools.Testers.JsonPathTester;

public sealed class JsonPathTesterCommandLineToolTests : MefBasedTest
{
    private readonly JsonPathTesterCommandLineTool _tool;

    public JsonPathTesterCommandLineToolTests()
        : base(typeof(JsonPathTesterCommandLineTool).Assembly)
    {
        _tool = (JsonPathTesterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JSONPathTester")
            .Value;
    }

    [Fact]
    public async Task TestJsonPathUi()
    {
        string inputJson = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.JsonPathTester.sample.json");

        _tool.InputJson = inputJson;
        _tool.InputJsonPath = "$.phoneNumbers[:1].type";

        using var consoleOutput = new ConsoleOutputMonitor();
        await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be(
            @"[
  ""iPhone""
]");
    }

    [Fact]
    public async Task TestJsonPathUiFailed()
    {
        string inputJson = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.JsonPathTester.sample.json");

        _tool.InputJson = inputJson;
        _tool.InputJsonPath = "$.TEST";

        using var consoleOutput = new ConsoleOutputMonitor();
        await _tool.InvokeAsync(new MockILogger(), CancellationToken.None);

        string result = consoleOutput.GetOutput().Trim();
        result.Should().Be("[]");
    }
}
